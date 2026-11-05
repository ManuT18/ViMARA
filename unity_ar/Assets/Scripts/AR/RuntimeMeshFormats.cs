using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

namespace ViMARA.AR
{
    /// <summary>
    /// Parser en tiempo de ejecución para archivos .OBJ y .STL (ASCII y Binario).
    /// Permite importar maquetas exportadas desde SketchUp, AutoCAD, Revit o Blender directamente en WebAR.
    /// </summary>
    public static class RuntimeMeshFormats
    {
        public static Mesh LoadStlFromBytes(byte[] data)
        {
            if (data == null || data.Length < 84) return null;

            // Verificar si es binario comparando el tamaño esperado: 80 header + 4 count + count * 50 bytes
            uint triCount = BitConverter.ToUInt32(data, 80);
            long expectedBinaryLength = 84L + (long)triCount * 50L;

            if (expectedBinaryLength == data.Length)
            {
                return LoadBinaryStl(data, (int)triCount);
            }

            // Verificar si comienza con "solid" pero podría ser binario con header "solid"
            string header = Encoding.ASCII.GetString(data, 0, Math.Min(80, data.Length));
            if (header.StartsWith("solid", StringComparison.OrdinalIgnoreCase))
            {
                string fullText = Encoding.ASCII.GetString(data);
                if (fullText.IndexOf("facet", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return LoadAsciiStl(fullText);
                }
            }

            // Fallback a binario si el archivo tiene al menos el espacio para los triángulos
            if (triCount > 0 && expectedBinaryLength <= data.Length)
            {
                return LoadBinaryStl(data, (int)triCount);
            }

            return LoadAsciiStl(Encoding.ASCII.GetString(data));
        }

        private static Mesh LoadBinaryStl(byte[] data, int triangleCount)
        {
            int vertexCount = triangleCount * 3;
            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            int[] triangles = new int[vertexCount];

            int offset = 84;
            for (int i = 0; i < triangleCount; i++)
            {
                if (offset + 50 > data.Length) break;

                // En STL Z suele ser arriba (CAD/SketchUp). Convertimos (x, z, y) a sistema de coordenadas de Unity (Y arriba)
                float nx = BitConverter.ToSingle(data, offset);
                float ny = BitConverter.ToSingle(data, offset + 4);
                float nz = BitConverter.ToSingle(data, offset + 8);
                Vector3 normal = new Vector3(-nx, nz, -ny);

                int vIdx = i * 3;
                for (int v = 0; v < 3; v++)
                {
                    int vOffset = offset + 12 + (v * 12);
                    float vx = BitConverter.ToSingle(data, vOffset);
                    float vy = BitConverter.ToSingle(data, vOffset + 4);
                    float vz = BitConverter.ToSingle(data, vOffset + 8);

                    vertices[vIdx + v] = new Vector3(-vx, vz, -vy);
                    normals[vIdx + v] = normal;
                }

                // Invertir orden de vértices por cambio de handedness
                triangles[vIdx] = vIdx;
                triangles[vIdx + 1] = vIdx + 2;
                triangles[vIdx + 2] = vIdx + 1;

                offset += 50;
            }

            Mesh mesh = new Mesh
            {
                name = "Imported_STL_Binary",
                indexFormat = vertexCount > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static Mesh LoadAsciiStl(string text)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            using (StringReader reader = new StringReader(text))
            {
                string line;
                List<Vector3> facetVerts = new List<Vector3>(3);
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.StartsWith("vertex", StringComparison.OrdinalIgnoreCase))
                    {
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4 &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            facetVerts.Add(new Vector3(-x, z, -y));
                            if (facetVerts.Count == 3)
                            {
                                int baseIdx = vertices.Count;
                                vertices.Add(facetVerts[0]);
                                vertices.Add(facetVerts[1]);
                                vertices.Add(facetVerts[2]);

                                triangles.Add(baseIdx);
                                triangles.Add(baseIdx + 2);
                                triangles.Add(baseIdx + 1);
                                facetVerts.Clear();
                            }
                        }
                    }
                }
            }

            if (vertices.Count == 0) return null;

            Mesh mesh = new Mesh
            {
                name = "Imported_STL_ASCII",
                indexFormat = vertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh LoadObjFromText(string objText)
        {
            if (string.IsNullOrEmpty(objText)) return null;

            List<Vector3> rawPositions = new List<Vector3>();
            List<Vector3> rawNormals = new List<Vector3>();
            List<Vector2> rawUVs = new List<Vector2>();

            List<Vector3> outVertices = new List<Vector3>();
            List<Vector3> outNormals = new List<Vector3>();
            List<Vector2> outUVs = new List<Vector2>();
            List<int> outTriangles = new List<int>();

            using (StringReader reader = new StringReader(objText))
            {
                string line;
                List<int> faceVertIndices = new List<int>(8);

                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length == 0 || line[0] == '#') continue;

                    if (line.StartsWith("v "))
                    {
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4 &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                        {
                            rawPositions.Add(new Vector3(-x, y, z));
                        }
                    }
                    else if (line.StartsWith("vn "))
                    {
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4 &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float nx) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float ny) &&
                            float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float nz))
                        {
                            rawNormals.Add(new Vector3(-nx, ny, nz));
                        }
                    }
                    else if (line.StartsWith("vt "))
                    {
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 3 &&
                            float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float u) &&
                            float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        {
                            rawUVs.Add(new Vector2(u, v));
                        }
                    }
                    else if (line.StartsWith("f "))
                    {
                        string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 4) continue;

                        faceVertIndices.Clear();
                        for (int i = 1; i < parts.Length; i++)
                        {
                            string[] sub = parts[i].Split('/');
                            if (!int.TryParse(sub[0], out int vIdx)) continue;

                            int posIndex = vIdx > 0 ? vIdx - 1 : rawPositions.Count + vIdx;
                            if (posIndex < 0 || posIndex >= rawPositions.Count) continue;

                            int uvIndex = -1;
                            if (sub.Length > 1 && !string.IsNullOrEmpty(sub[1]) && int.TryParse(sub[1], out int vtIdx))
                            {
                                uvIndex = vtIdx > 0 ? vtIdx - 1 : rawUVs.Count + vtIdx;
                            }

                            int normIndex = -1;
                            if (sub.Length > 2 && !string.IsNullOrEmpty(sub[2]) && int.TryParse(sub[2], out int vnIdx))
                            {
                                normIndex = vnIdx > 0 ? vnIdx - 1 : rawNormals.Count + vnIdx;
                            }

                            int newIdx = outVertices.Count;
                            outVertices.Add(rawPositions[posIndex]);
                            outUVs.Add(uvIndex >= 0 && uvIndex < rawUVs.Count ? rawUVs[uvIndex] : Vector2.zero);
                            outNormals.Add(normIndex >= 0 && normIndex < rawNormals.Count ? rawNormals[normIndex] : Vector3.up);
                            faceVertIndices.Add(newIdx);
                        }

                        // Triangulación tipo abanico (invertida por cambio de X -> -X)
                        for (int i = 1; i + 1 < faceVertIndices.Count; i++)
                        {
                            outTriangles.Add(faceVertIndices[0]);
                            outTriangles.Add(faceVertIndices[i + 1]);
                            outTriangles.Add(faceVertIndices[i]);
                        }
                    }
                }
            }

            if (outVertices.Count == 0) return null;

            Mesh mesh = new Mesh
            {
                name = "Imported_OBJ",
                indexFormat = outVertices.Count > 65535 ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16
            };
            mesh.SetVertices(outVertices);
            mesh.SetUVs(0, outUVs);
            mesh.SetTriangles(outTriangles, 0);
            if (rawNormals.Count > 0)
            {
                mesh.SetNormals(outNormals);
            }
            else
            {
                mesh.RecalculateNormals();
            }
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
