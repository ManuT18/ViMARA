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
        
        // 1. Ensure ARTrackingModeSwitcher exists
        var switcherObj = GameObject.Find("AR Mode Switcher");
        if (switcherObj == null)
        {
            switcherObj = new GameObject("AR Mode Switcher");
        }
        var switcher = switcherObj.GetComponent<ARTrackingModeSwitcher>();
        if (switcher == null) switcher = switcherObj.AddComponent<ARTrackingModeSwitcher>();

        // 2. Ensure Zappar Image Tracking Target exists and has the correct target filename
        var imageTrackerObj = GameObject.Find("Zappar Image Tracking Target");
        if (imageTrackerObj == null)
        {
            imageTrackerObj = new GameObject("Zappar Image Tracking Target");
        }
        var imageTarget = imageTrackerObj.GetComponent<ZapparImageTrackingTarget>();
        if (imageTarget == null) imageTarget = imageTrackerObj.AddComponent<ZapparImageTrackingTarget>();
        
        // Asignar el marcador solicitado
        imageTarget.Target = "marcador_logo.zpt";
        imageTarget.Orientation = ZapparImageTrackingTarget.PlaneOrientation.Flat;

        // 3. Vincular referencias en el Switcher
        switcher.InstantTrackingRoot = GameObject.Find("Zappar Instant Tracking Target");
        switcher.ImageTrackingRoot = imageTrackerObj;

        // 4. Asegurar que haya un modelo / cubo de prueba dentro del Image Tracker si no existe aún
        var existingCubeInTarget = imageTrackerObj.transform.Find("Cube_ImageTarget");
        if (existingCubeInTarget == null)
        {
            var instantCube = GameObject.Find("Cube");
            if (instantCube != null)
            {
                var newCube = Instantiate(instantCube, imageTrackerObj.transform);
                newCube.name = "Cube_ImageTarget";
                newCube.transform.localPosition = new Vector3(0, 0, 0);
                newCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[ViMARA] Escena configurada con éxito para Seguimiento Dual (Instant + Image Target: marcador_logo.zpt).");
    }
}
