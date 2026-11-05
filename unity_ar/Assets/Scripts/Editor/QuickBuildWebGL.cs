using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ViMARA.Editor
{
    /// <summary>
    /// Herramienta de compilación optimizada para ViMARA WebAR.
    /// Exporta directamente a public/unity_ar/ sin abrir ventanas del Explorador de archivos.
    /// </summary>
    public static class QuickBuildWebGL
    {
        private const string ScenePath = "Assets/Scenes/WebAR_InstantTracking.unity";
        private const string OutputRelativePath = "../public/unity_ar";

        [MenuItem("ViMARA/⚡ Compilar WebGL Rápido (Desarrollo <30s) %#b", priority = 1)]
        public static void BuildWebGLFast()
        {
            // Forzar modo Development para omitir el pase pesado de wasm-opt y compilar en ~20-30s
            PerformBuild(BuildOptions.Development, "Desarrollo Rápido (<30s)");
        }

        [MenuItem("ViMARA/📦 Compilar WebGL Producción (Release minificado)", priority = 2)]
        public static void BuildWebGLRelease()
        {
            PerformBuild(BuildOptions.None, "Release / Producción");
        }

        private static void PerformBuild(BuildOptions options, string buildType)
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputPath = Path.GetFullPath(Path.Combine(projectRoot, OutputRelativePath));

            Debug.Log($"[ViMARA Build] Iniciando compilación WebGL ({buildType}) hacia: {outputPath}");

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                StripWasmCustomSections(outputPath);
                Debug.Log($"<color=#4ade80><b>[ViMARA Build] ¡Compilación WebGL completada!</b></color> Tiempo: {summary.totalTime.TotalSeconds:F1}s | Tamaño: {summary.totalSize / (1024 * 1024)} MB.");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"<color=#ef4444><b>[ViMARA Build] Error durante la compilación:</b></color> {summary.totalErrors} errores encontrados.");
            }
        }

        /// <summary>
        /// Elimina la sección custom "name" (id=0) generada en builds rápidos para mantener unity_ar.wasm por debajo de los 50 MB de GitHub.
        /// </summary>
        private static void StripWasmCustomSections(string outputPath)
        {
            try
            {
                string wasmPath = Path.Combine(outputPath, "Build", "unity_ar.wasm");
                if (!File.Exists(wasmPath)) return;

                byte[] bytes = File.ReadAllBytes(wasmPath);
                int oldLen = bytes.Length;
                int pos = 8;
                int truncateAt = -1;

                while (pos < bytes.Length)
                {
                    int secStart = pos;
                    byte id = bytes[pos++];
                    uint len = 0;
                    int shift = 0;
                    while (pos < bytes.Length)
                    {
                        byte b = bytes[pos++];
                        len |= (uint)(b & 0x7F) << shift;
                        if ((b & 0x80) == 0) break;
                        shift += 7;
                    }

                    if (id == 0)
                    {
                        truncateAt = secStart;
                        break;
                    }
                    pos += (int)len;
                }

                if (truncateAt > 8 && truncateAt < oldLen)
                {
                    using (FileStream fs = new FileStream(wasmPath, FileMode.Open, FileAccess.Write))
                    {
                        fs.SetLength(truncateAt);
                    }
                    string loaderPath = Path.Combine(outputPath, "Build", "unity_ar.loader.js");
                    if (File.Exists(loaderPath))
                    {
                        string loaderText = File.ReadAllText(loaderPath);
                        loaderText = loaderText.Replace($"wasmFileSize: {oldLen}", $"wasmFileSize: {truncateAt}");
                        File.WriteAllText(loaderPath, loaderText);
                    }
                    Debug.Log($"[ViMARA Build] Optimizado unity_ar.wasm de {oldLen / (1024f * 1024f):F2} MB a {truncateAt / (1024f * 1024f):F2} MB (<50MB GitHub limit).");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[ViMARA Build] No se pudo optimizar sección debug de WASM: {ex.Message}");
            }
        }
    }
}
