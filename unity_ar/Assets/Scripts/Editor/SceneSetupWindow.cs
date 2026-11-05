using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.Rendering;
using ViMARA.AR;
using Zappar;

public class SceneSetupWindow : EditorWindow
{
    [MenuItem("ViMARA/Setup AR Scene")]
    public static void SetupScene()
    {
        EnsureShadersAlwaysIncluded();
        var scene = EditorSceneManager.GetActiveScene();
        
        // 1. Asegurar ARTrackingModeSwitcher y RuntimeModelLoader
        var switcherObj = GameObject.Find("AR Mode Switcher");
        if (switcherObj == null)
        {
            switcherObj = new GameObject("AR Mode Switcher");
        }
        var switcher = switcherObj.GetComponent<ARTrackingModeSwitcher>();
        if (switcher == null) switcher = switcherObj.AddComponent<ARTrackingModeSwitcher>();

        var modelLoader = switcherObj.GetComponent<RuntimeModelLoader>();
        if (modelLoader == null) modelLoader = switcherObj.AddComponent<RuntimeModelLoader>();

        // 2. Asegurar Zappar Image Tracking Target con PersistentMarkerController
        var imageTrackerObj = GameObject.Find("Zappar Image Tracking Target");
        if (imageTrackerObj == null)
        {
            imageTrackerObj = new GameObject("Zappar Image Tracking Target");
        }
        var imageTarget = imageTrackerObj.GetComponent<ZapparImageTrackingTarget>();
        if (imageTarget == null) imageTarget = imageTrackerObj.AddComponent<ZapparImageTrackingTarget>();
        
        imageTarget.Target = "marcador_logo.zpt";
        imageTarget.Orientation = ZapparImageTrackingTarget.PlaneOrientation.Flat;

        var persistentCtrl = imageTrackerObj.GetComponent<PersistentMarkerController>();
        if (persistentCtrl == null) persistentCtrl = imageTrackerObj.AddComponent<PersistentMarkerController>();
        persistentCtrl.ElevationOffset = 0.01f;

        // Ocultar Preview Object del marcador para que no se proyecte la imagen 2D del marcador en el visor
        if (imageTarget.PreviewImageObject != null)
        {
            imageTarget.PreviewImageObject.SetActive(false);
            var previewRenderer = imageTarget.PreviewImageObject.GetComponent<Renderer>();
            if (previewRenderer != null) previewRenderer.enabled = false;
        }
        var previewChild = imageTrackerObj.transform.Find("Preview Object");
        if (previewChild != null)
        {
            previewChild.gameObject.SetActive(false);
            var previewRenderer = previewChild.GetComponent<Renderer>();
            if (previewRenderer != null) previewRenderer.enabled = false;
        }

        // 3. Vincular referencias en el Switcher
        var instantRoot = GameObject.Find("Zappar Instant Tracking Target");
        switcher.InstantTrackingRoot = instantRoot;
        switcher.ImageTrackingRoot = imageTrackerObj;

        // 4. Asegurar InstantTrackingController en el target de superficie
        if (instantRoot != null)
        {
            var instantCtrl = instantRoot.GetComponent<InstantTrackingController>();
            if (instantCtrl == null) instantCtrl = instantRoot.AddComponent<InstantTrackingController>();
            instantCtrl.ShowPlaneVisualizer = true;
        }

        // 5. Configurar Contenedor de Modelo de Instant Tracking
        var instantCube = GameObject.Find("Cube");
        if (instantCube != null)
        {
            instantCube.transform.localPosition = Vector3.zero;
            instantCube.transform.localScale = Vector3.one;
            if (instantCube.GetComponent<TouchManipulationController>() == null)
            {
                instantCube.AddComponent<TouchManipulationController>();
            }

            if (instantRoot != null)
            {
                var instantCtrl = instantRoot.GetComponent<InstantTrackingController>();
                if (instantCtrl != null) instantCtrl.ModelContainer = instantCube.transform;
            }
            modelLoader.SurfaceModelContainer = instantCube.transform;
            var cubeMr = instantCube.GetComponent<MeshRenderer>();
            if (cubeMr != null && cubeMr.sharedMaterial != null)
            {
                modelLoader.BaseUrpMaterial = cubeMr.sharedMaterial;
            }
        }

        // 6. Configurar Contenedor de Modelo dentro de Image Tracking Target
        var existingCubeInTarget = imageTrackerObj.transform.Find("Cube_ImageTarget");
        if (existingCubeInTarget == null && instantCube != null)
        {
            var newCube = Instantiate(instantCube, imageTrackerObj.transform);
            newCube.name = "Cube_ImageTarget";
            newCube.transform.localPosition = Vector3.zero;
            newCube.transform.localScale = Vector3.one;
            if (newCube.GetComponent<TouchManipulationController>() == null)
            {
                newCube.AddComponent<TouchManipulationController>();
            }
            persistentCtrl.TargetModel = newCube.transform;
            modelLoader.MarkerModelContainer = newCube.transform;
        }
        else if (existingCubeInTarget != null)
        {
            existingCubeInTarget.localPosition = Vector3.zero;
            existingCubeInTarget.localScale = Vector3.one;
            if (existingCubeInTarget.GetComponent<TouchManipulationController>() == null)
            {
                existingCubeInTarget.gameObject.AddComponent<TouchManipulationController>();
            }
            persistentCtrl.TargetModel = existingCubeInTarget;
            modelLoader.MarkerModelContainer = existingCubeInTarget;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ViMARA] Escena AR configurada con RuntimeModelLoader (GLB/GLTF/OBJ/STL + Maquetas de Prueba y Shaders URP/glTFast protegidos).");
    }

    private static void EnsureShadersAlwaysIncluded()
    {
        string[] shaderNames = new[]
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Shader Graphs/glTF-pbrMetallicRoughness",
            "Shader Graphs/glTF-pbrSpecularGlossiness",
            "Shader Graphs/glTF-unlit",
            "glTF/PbrMetallicRoughness",
            "glTF/PbrSpecularGlossiness",
            "glTF/Unlit"
        };

        var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettingsObj == null) return;

        var serializedObject = new SerializedObject(graphicsSettingsObj);
        var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
        if (arrayProp == null) return;

        bool modified = false;
        foreach (string sName in shaderNames)
        {
            Shader shader = Shader.Find(sName);
            if (shader == null) continue;

            bool exists = false;
            for (int i = 0; i < arrayProp.arraySize; i++)
            {
                if (arrayProp.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                int idx = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(idx);
                arrayProp.GetArrayElementAtIndex(idx).objectReferenceValue = shader;
                modified = true;
            }
        }

        if (modified)
        {
            serializedObject.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
