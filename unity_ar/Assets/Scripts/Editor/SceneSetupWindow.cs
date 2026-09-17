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
        
        // 1. Asegurar ARTrackingModeSwitcher
        var switcherObj = GameObject.Find("AR Mode Switcher");
        if (switcherObj == null)
        {
            switcherObj = new GameObject("AR Mode Switcher");
        }
        var switcher = switcherObj.GetComponent<ARTrackingModeSwitcher>();
        if (switcher == null) switcher = switcherObj.AddComponent<ARTrackingModeSwitcher>();

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
        persistentCtrl.ElevationOffset = 0.15f;

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

        // 5. Configurar Cubo de Instant Tracking con elevación ergonómica y manipulación táctil
        var instantCube = GameObject.Find("Cube");
        if (instantCube != null)
        {
            instantCube.transform.localPosition = new Vector3(0, 0.15f, 0);
            instantCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            if (instantCube.GetComponent<TouchManipulationController>() == null)
            {
                instantCube.AddComponent<TouchManipulationController>();
            }

            // Vincular al InstantTrackingController
            if (instantRoot != null)
            {
                var instantCtrl = instantRoot.GetComponent<InstantTrackingController>();
                if (instantCtrl != null) instantCtrl.ModelContainer = instantCube.transform;
            }
        }

        // 6. Configurar Cubo dentro de Image Tracking Target
        var existingCubeInTarget = imageTrackerObj.transform.Find("Cube_ImageTarget");
        if (existingCubeInTarget == null && instantCube != null)
        {
            var newCube = Instantiate(instantCube, imageTrackerObj.transform);
            newCube.name = "Cube_ImageTarget";
            newCube.transform.localPosition = new Vector3(0, 0.15f, 0);
            newCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            if (newCube.GetComponent<TouchManipulationController>() == null)
            {
                newCube.AddComponent<TouchManipulationController>();
            }
            persistentCtrl.TargetModel = newCube.transform;
        }
        else if (existingCubeInTarget != null)
        {
            existingCubeInTarget.localPosition = new Vector3(0, 0.15f, 0);
            existingCubeInTarget.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            if (existingCubeInTarget.GetComponent<TouchManipulationController>() == null)
            {
                existingCubeInTarget.gameObject.AddComponent<TouchManipulationController>();
            }
            persistentCtrl.TargetModel = existingCubeInTarget;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ViMARA] Escena AR reconfigurada exitosamente con Retícula de Escaneo, Anclaje Persistente y Prevención de Congelamientos.");
    }
}
