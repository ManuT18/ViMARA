using UnityEngine;

namespace ViMARA.AR
{
    /// <summary>
    /// Generador de maquetas y modelos 3D procedurales integrados (sin descargas externas).
    /// Provee prototipos arquitectónicos de prueba listos para usar instantáneamente.
    /// </summary>
    public static class ProceduralSampleBuilder
    {
        public static GameObject CreatePreset(string presetId, Material templateMaterial = null)
        {
            if (string.IsNullOrEmpty(presetId)) return CreateModernHouse(templateMaterial);

            string id = presetId.ToLowerInvariant();
            if (id.Contains("tower") || id.Contains("edificio") || id.Contains("building"))
            {
                return CreateApartmentTower(templateMaterial);
            }
            if (id.Contains("pavilion") || id.Contains("pabellon") || id.Contains("plaza"))
            {
                return CreateArchPavilion(templateMaterial);
            }
            return CreateModernHouse(templateMaterial);
        }

        private static Material CreateArchMaterial(Color color, float smoothness, Material templateMaterial)
        {
            Material mat;
            if (templateMaterial != null)
            {
                mat = new Material(templateMaterial);
            }
            else
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit")
                             ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                             ?? Shader.Find("Standard")
                             ?? Shader.Find("Sprites/Default");
                mat = new Material(shader);
            }

            mat.color = color;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);
            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            return mat;
        }

        public static GameObject CreateModernHouse(Material templateMaterial = null)
        {
            GameObject root = new GameObject("Sample_ModernHouse");

            Material matConcrete = CreateArchMaterial(new Color(0.92f, 0.93f, 0.95f), 0.2f, templateMaterial);
            Material matWood = CreateArchMaterial(new Color(0.68f, 0.45f, 0.26f), 0.3f, templateMaterial);
            Material matDark = CreateArchMaterial(new Color(0.18f, 0.22f, 0.28f), 0.4f, templateMaterial);
            Material matGlass = CreateArchMaterial(new Color(0.35f, 0.72f, 0.95f), 0.85f, templateMaterial);
            Material matGrass = CreateArchMaterial(new Color(0.28f, 0.58f, 0.36f), 0.15f, templateMaterial);
            Material matPool = CreateArchMaterial(new Color(0.12f, 0.68f, 0.88f), 0.9f, templateMaterial);

            // Terreno base, deck de madera y piscina
            AddPrimitive(PrimitiveType.Cube, root.transform, "TerrainBase", new Vector3(0f, 0.025f, 0f), new Vector3(1.2f, 0.05f, 1.0f), matGrass);
            AddPrimitive(PrimitiveType.Cube, root.transform, "WoodDeck", new Vector3(0.15f, 0.055f, 0.1f), new Vector3(0.75f, 0.02f, 0.65f), matWood);
            AddPrimitive(PrimitiveType.Cube, root.transform, "SwimmingPool", new Vector3(0.28f, 0.062f, 0.22f), new Vector3(0.38f, 0.015f, 0.28f), matPool);

            // Planta baja (Volumen principal de hormigón y ventanal)
            AddPrimitive(PrimitiveType.Cube, root.transform, "GroundFloor", new Vector3(-0.18f, 0.18f, -0.05f), new Vector3(0.62f, 0.26f, 0.55f), matConcrete);
            AddPrimitive(PrimitiveType.Cube, root.transform, "GlassFacade_GF", new Vector3(0.135f, 0.17f, -0.05f), new Vector3(0.02f, 0.21f, 0.42f), matGlass);

            // Planta alta en voladizo (Cantilever)
            AddPrimitive(PrimitiveType.Cube, root.transform, "UpperFloorCantilever", new Vector3(0.02f, 0.41f, -0.08f), new Vector3(0.72f, 0.22f, 0.48f), matConcrete);
            AddPrimitive(PrimitiveType.Cube, root.transform, "UpperWoodAccent", new Vector3(-0.12f, 0.41f, 0.165f), new Vector3(0.42f, 0.18f, 0.02f), matWood);
            AddPrimitive(PrimitiveType.Cube, root.transform, "GlassFacade_UF", new Vector3(0.22f, 0.41f, 0.165f), new Vector3(0.24f, 0.18f, 0.02f), matGlass);

            // Cubierta superior y columnas de soporte
            AddPrimitive(PrimitiveType.Cube, root.transform, "RoofSlab", new Vector3(0.02f, 0.535f, -0.08f), new Vector3(0.76f, 0.03f, 0.52f), matDark);
            AddPrimitive(PrimitiveType.Cylinder, root.transform, "Column_1", new Vector3(0.32f, 0.18f, -0.26f), new Vector3(0.03f, 0.12f, 0.03f), matDark);
            AddPrimitive(PrimitiveType.Cylinder, root.transform, "Column_2", new Vector3(0.32f, 0.18f, 0.10f), new Vector3(0.03f, 0.12f, 0.03f), matDark);

            return root;
        }

        public static GameObject CreateApartmentTower(Material templateMaterial = null)
        {
            GameObject root = new GameObject("Sample_ApartmentTower");

            Material matBase = CreateArchMaterial(new Color(0.24f, 0.27f, 0.32f), 0.3f, templateMaterial);
            Material matConcrete = CreateArchMaterial(new Color(0.88f, 0.90f, 0.93f), 0.25f, templateMaterial);
            Material matGlass = CreateArchMaterial(new Color(0.22f, 0.55f, 0.82f), 0.85f, templateMaterial);
            Material matAccent = CreateArchMaterial(new Color(0.85f, 0.58f, 0.28f), 0.4f, templateMaterial);

            // Plazoleta urbana y basamento comercial (Podium)
            AddPrimitive(PrimitiveType.Cube, root.transform, "UrbanPlaza", new Vector3(0f, 0.02f, 0f), new Vector3(1.0f, 0.04f, 1.0f), matBase);
            AddPrimitive(PrimitiveType.Cube, root.transform, "PodiumBase", new Vector3(0f, 0.12f, 0f), new Vector3(0.68f, 0.16f, 0.58f), matConcrete);
            AddPrimitive(PrimitiveType.Cube, root.transform, "PodiumGlass", new Vector3(0f, 0.11f, 0.295f), new Vector3(0.58f, 0.12f, 0.015f), matGlass);

            // Núcleo acristalado de la torre y losas de entrepiso
            const float towerHeight = 0.95f;
            const int floors = 8;
            AddPrimitive(PrimitiveType.Cube, root.transform, "TowerGlassCore", new Vector3(-0.04f, 0.20f + towerHeight * 0.5f, -0.02f), new Vector3(0.38f, towerHeight, 0.36f), matGlass);

            for (int i = 0; i <= floors; i++)
            {
                float y = 0.22f + i * (towerHeight / floors);
                AddPrimitive(PrimitiveType.Cube, root.transform, $"FloorSlab_{i}", new Vector3(-0.04f, y, -0.02f), new Vector3(0.42f, 0.018f, 0.40f), matConcrete);

                if (i > 0 && i < floors)
                {
                    float offsetX = (i % 2 == 0) ? 0.14f : -0.14f;
                    AddPrimitive(PrimitiveType.Cube, root.transform, $"Balcony_{i}", new Vector3(offsetX, y + 0.015f, 0.19f), new Vector3(0.18f, 0.025f, 0.06f), matAccent);
                }
            }

            AddPrimitive(PrimitiveType.Cube, root.transform, "RoofCrown", new Vector3(-0.04f, 0.22f + towerHeight + 0.05f, -0.02f), new Vector3(0.28f, 0.08f, 0.26f), matConcrete);

            return root;
        }

        public static GameObject CreateArchPavilion(Material templateMaterial = null)
        {
            GameObject root = new GameObject("Sample_ArchPavilion");

            Material matStone = CreateArchMaterial(new Color(0.85f, 0.83f, 0.78f), 0.2f, templateMaterial);
            Material matWood = CreateArchMaterial(new Color(0.74f, 0.48f, 0.24f), 0.35f, templateMaterial);
            Material matSteel = CreateArchMaterial(new Color(0.20f, 0.24f, 0.28f), 0.6f, templateMaterial);
            Material matWater = CreateArchMaterial(new Color(0.18f, 0.70f, 0.85f), 0.9f, templateMaterial);

            // Plataforma escalonada y espejo de agua
            AddPrimitive(PrimitiveType.Cube, root.transform, "Step_1", new Vector3(0f, 0.02f, 0f), new Vector3(1.1f, 0.04f, 0.9f), matStone);
            AddPrimitive(PrimitiveType.Cube, root.transform, "Step_2", new Vector3(0f, 0.055f, 0f), new Vector3(0.95f, 0.03f, 0.75f), matStone);
            AddPrimitive(PrimitiveType.Cube, root.transform, "WaterMirror", new Vector3(0f, 0.072f, 0.24f), new Vector3(0.82f, 0.01f, 0.18f), matWater);

            // Columnata perimetral
            float[] colX = { -0.36f, -0.12f, 0.12f, 0.36f };
            float[] colZ = { -0.26f, 0.08f };
            foreach (float x in colX)
            {
                foreach (float z in colZ)
                {
                    AddPrimitive(PrimitiveType.Cylinder, root.transform, $"Pillar_{x}_{z}", new Vector3(x, 0.24f, z), new Vector3(0.035f, 0.17f, 0.035f), matSteel);
                }
            }

            // Pérgola de listones de madera y escultura central
            const int slats = 9;
            for (int i = 0; i < slats; i++)
            {
                float z = -0.32f + i * 0.06f;
                AddPrimitive(PrimitiveType.Cube, root.transform, $"WoodBaffle_{i}", new Vector3(0f, 0.43f, z), new Vector3(0.88f, 0.035f, 0.025f), matWood);
            }

            AddPrimitive(PrimitiveType.Cylinder, root.transform, "CentralSculpture", new Vector3(0f, 0.18f, -0.09f), new Vector3(0.16f, 0.11f, 0.16f), matWood);

            return root;
        }

        private static GameObject AddPrimitive(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 localPos,
            Vector3 scale,
            Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;

            Collider col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;

            return go;
        }
    }
}
