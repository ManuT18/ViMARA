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
        
        // Ensure ARTrackingModeSwitcher exists
        var switcherObj = GameObject.Find("AR Mode Switcher");
        if (switcherObj == null)
        {
            switcherObj = new GameObject("AR Mode Switcher");
        }
        var switcher = switcherObj.GetComponent<ARTrackingModeSwitcher>();
        if (switcher == null) switcher = switcherObj.AddComponent<ARTrackingModeSwitcher>();

        // Ensure Zappar Image Tracking Target exists
        var imageTrackerObj = GameObject.Find("Zappar Image Tracking Target");
        if (imageTrackerObj == null)
        {
            imageTrackerObj = new GameObject("Zappar Image Tracking Target");
        }
        var imageTarget = imageTrackerObj.GetComponent<ZapparImageTrackingTarget>();
        if (imageTarget == null) imageTarget = imageTrackerObj.AddComponent<ZapparImageTrackingTarget>();
        
        // Link them
        switcher.InstantTrackingRoot = GameObject.Find("Zappar Instant Tracking Target");
        switcher.ImageTrackingRoot = imageTrackerObj;

        // Set up the Cube inside Image Tracker
        var cube = GameObject.Find("Cube");
        if (cube != null)
        {
            var newCube = Instantiate(cube, imageTrackerObj.transform);
            newCube.name = "Cube_ImageTarget";
            newCube.transform.localPosition = new Vector3(0, 0, 0);
            newCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Scene configured for Dual Tracking (Instant + Image) successfully.");
    }
}
