using UnityEngine;

namespace ViMARA.AR
{
    /// <summary>
    /// Generador de maquetas y modelos 3D procedurales integrados (sin descargas externas).
    /// Provee distintos prototipos arquitectónicos de prueba listos para usar instantáneamente.
    /// </summary>
    public static class ProceduralSampleBuilder
    {
        public static GameObject CreatePreset(string presetId)
        {
            if (string.IsNullOrEmpty(presetId)) return CreateModernHouse();

            string id = presetId.ToLowerInvariant();
            if (id.Contains("tower") || id.Contains("edificio") || id.Contains("building"))
            {
                return CreateApartmentTower();
            }
            if (id.Contains("pavilion") || id.Contains("pabellon") || id.Contains("plaza"))
            {
                return CreateArchPavilion();
            }
            return CreateModernHouse();
        }

        private static Material CreateArchMaterial(Color color, float smoothness = 0.35f)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material mat = new Material(shader) { color = color };
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        public static GameObject CreateModernHouse()
        {
            GameObject root = new GameObject("Sample_ModernHouse");

            Material matConcrete = CreateArchMaterial(new Color(0.92f, 0.93f, 0.95f), 0.2f);
            Material matWood = CreateArchMaterial(new Color(0.68f, 0.45f, 0.26f), 0.3f);
            Material matDark = CreateArchMaterial(new Color(0.18f, 0.22f, 0.28f), 0.4f);
            Material matGlass = CreateArchMaterial(new Color(0.35f, 0.72f, 0.95f), 0.85f);
            Material matGrass = CreateArchMaterial(new Color(0.28f, 0.58f, 0.36f), 0.15f);
            Material matPool = CreateArchMaterial(new Color(0.12f, 0.68f, 0.88f), 0.9f);

            // Terreno base
            AddBox(root.transform, "TerrainBase", new Vector3(0f, 0.025f, 0f), new Vector3(1.2f, 0.05f, 1.0f), matGrass);
            // Deck / Terraza de madera
            AddBox(root.transform, "WoodDeck", new Vector3(0.15f, 0.055f, 0.1f), new Vector3(0.75f, 0.02f, 0.65f), matWood);
            // Piscina
            AddBox(root.transform, "SwimmingPool", new Vector3(0.28f, 0.062f, 0.22f), new Vector3(0.38f, 0.015f, 0.28f), matPool);

            // Planta baja (Volumen principal de hormigón)
            AddBox(root.transform, "GroundFloor", new Vector3(-0.18f, 0.18f, -0.05f), new Vector3(0.62f, 0.26f, 0.55f), matConcrete);
            // Ventanal planta baja
            AddBox(root.transform, "GlassFacade_GF", new Vector3(0.135f, 0.17f, -0.05f), new Vector3(0.02f, 0.21f, 0.42f), matGlass);

            // Planta alta en voladizo (Cantilever)
            AddBox(root.transform, "UpperFloorCantilever", new Vector3(0.02f, 0.41f, -0.08f), new Vector3(0.72f, 0.22f, 0.48f), matConcrete);
            // Revestimiento madera planta alta
            AddBox(root.transform, "UpperWoodAccent", new Vector3(-0.12f, 0.41f, 0.165f), new Vector3(0.42f, 0.18f, 0.02f), matWood);
            // Ventanal planta alta
            AddBox(root.transform, "GlassFacade_UF", new Vector3(0.22f, 0.41f, 0.165f), new Vector3(0.24f, 0.18f, 0.02f), matGlass);

            // Cubierta / Losa superior oscura
            AddBox(root.transform, "RoofSlab", new Vector3(0.02f, 0.535f, -0.08f), new Vector3(0.76f, 0.03f, 0.52f), matDark);

            // Columnas de soporte del voladizo
            AddCylinder(root.transform, "Column_1", new Vector3(0.32f, 0.18f, -0.26f), new Vector3(0.03f, 0.12f, 0.03f), matDark);
            AddCylinder(root.transform, "Column_2", new Vector3(0.32f, 0.18f, 0.10f), new Vector3(0.03f, 0.12f, 0.03f), matDark);

            return root;
        }

        public static GameObject CreateApartmentTower()
        {
            GameObject root = new GameObject("Sample_ApartmentTower");

            Material matBase = CreateArchMaterial(new Color(0.24f, 0.27f, 0.32f), 0.3f);
            Material matConcrete = CreateArchMaterial(new Color(0.88f, 0.90f, 0.93f), 0.25f);
            Material matGlass = CreateArchMaterial(new Color(0.22f, 0.55f, 0.82f), 0.85f);
            Material matAccent = CreateArchMaterial(new Color(0.85f, 0.58f, 0.28f), 0.4f);

            // Plazoleta urbana
            AddBox(root.transform, "UrbanPlaza", new Vector3(0f, 0.02f, 0f), new Vector3(1.0f, 0.04f, 1.0f), matBase);

            // Basamento comercial (Podium)
            AddBox(root.transform, "PodiumBase", new Vector3(0f, 0.12f, 0f), new Vector3(0.68f, 0.16f, 0.58f), matConcrete);
            AddBox(root.transform, "PodiumGlass", new Vector3(0f, 0.11f, 0.295f), new Vector3(0.58f, 0.12f, 0.015f), matGlass);

            // Núcleo acristalado de la torre
            float towerHeight = 0.95f;
            AddBox(root.transform, "TowerGlassCore", new Vector3(-0.04f, 0.20f + towerHeight * 0.5f, -0.02f), new Vector3(0.38f, towerHeight, 0.36f), matGlass);

            // Losas de entrepiso (8 niveles)
            int floors = 8;
            for (int i = 0; i <= floors; i++)
            {
                float y = 0.22f + i * (towerHeight / floors);
                AddBox(root.transform, $"FloorSlab_{i}", new Vector3(-0.04f, y, -0.02f), new Vector3(0.42f, 0.018f, 0.40f), matConcrete);

                // Balcones alternados
                if (i > 0 && i < floors)
                {
                    float offsetX = (i % 2 == 0) ? 0.14f : -0.14f;
                    AddBox(root.transform, $"Balcony_{i}", new Vector3(offsetX, y + 0.015f, 0.19f), new Vector3(0.18f, 0.025f, 0.06f), matAccent);
                }
            }

            // Remate / Pérgola superior
            AddBox(root.transform, "RoofCrown", new Vector3(-0.04f, 0.22f + towerHeight + 0.05f, -0.02f), new Vector3(0.28f, 0.08f, 0.26f), matConcrete);

            return root;
        }

        public static GameObject CreateArchPavilion()
        {
            GameObject root = new GameObject("Sample_ArchPavilion");

            Material matStone = CreateArchMaterial(new Color(0.85f, 0.83f, 0.78f), 0.2f);
            Material matWood = CreateArchMaterial(new Color(0.74f, 0.48f, 0.24f), 0.35f);
            Material matSteel = CreateArchMaterial(new Color(0.20f, 0.24f, 0.28f), 0.6f);
            Material matWater = CreateArchMaterial(new Color(0.18f, 0.70f, 0.85f), 0.9f);

            // Plataforma escalonada
            AddBox(root.transform, "Step_1", new Vector3(0f, 0.02f, 0f), new Vector3(1.1f, 0.04f, 0.9f), matStone);
            AddBox(root.transform, "Step_2", new Vector3(0f, 0.055f, 0f), new Vector3(0.95f, 0.03f, 0.75f), matStone);

            // Espejo de agua lateral
            AddBox(root.transform, "WaterMirror", new Vector3(0f, 0.072f, 0.24f), new Vector3(0.82f, 0.01f, 0.18f), matWater);

            // Columnata perimetral
            float[] colX = { -0.36f, -0.12f, 0.12f, 0.36f };
            float[] colZ = { -0.26f, 0.08f };
            foreach (float x in colX)
            {
                foreach (float z in colZ)
                {
                    AddCylinder(root.transform, $"Pillar_{x}_{z}", new Vector3(x, 0.24f, z), new Vector3(0.035f, 0.17f, 0.035f), matSteel);
                }
            }

            // Pérgola de listones de madera
            int slats = 9;
            for (int i = 0; i < slats; i++)
            {
                float z = -0.32f + i * 0.06f;
                AddBox(root.transform, $"WoodBaffle_{i}", new Vector3(0f, 0.43f, z), new Vector3(0.88f, 0.035f, 0.025f), matWood);
            }

            // Volumen escultórico central
            AddCylinder(root.transform, "CentralSculpture", new Vector3(0f, 0.18f, -0.09f), new Vector3(0.16f, 0.11f, 0.16f), matWood);

            return root;
        }

        private static GameObject AddBox(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Collider col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return go;
        }

        private static GameObject AddCylinder(Transform parent, string name, Vector3 localPos, Vector3 scale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Collider col = go.GetComponent<Collider>();
            if (col != null) UnityEngine.Object.Destroy(col);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
            return go;
        }
    }
}
