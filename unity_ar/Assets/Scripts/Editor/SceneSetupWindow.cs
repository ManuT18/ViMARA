using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using ViMARA.AR;
using Zappar;

public class SceneSetupWindow : EditorWindow
{
    [MenuItem("ViMARA/Setup AR Scene")]
    public static void SetupScene()
    {
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
        persistentCtrl.ElevationOffset = 0.02f;

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
        Debug.Log("[ViMARA] Escena AR configurada con RuntimeModelLoader (GLB/GLTF/OBJ/STL + Maquetas de Prueba).");
    }
}
