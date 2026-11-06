using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using ViMARA.AR;
using Zappar;

namespace ViMARA.Editor
{
    /// <summary>
    /// Herramienta de configuración automática de la escena WebAR de ViMARA.
    /// Vincula los targets de Zappar, los contenedores de modelos y protege los shaders URP/glTFast contra stripping.
    /// </summary>
    public class SceneSetupWindow : EditorWindow
    {
        private static readonly string[] RequiredShaders =
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

        [MenuItem("ViMARA/Setup AR Scene")]
        public static void SetupScene()
        {
            EnsureShadersAlwaysIncluded();
            var scene = EditorSceneManager.GetActiveScene();

            // 1. Asegurar ARTrackingModeSwitcher y RuntimeModelLoader
            GameObject switcherObj = FindOrCreateGameObject("AR Mode Switcher");
            var switcher = GetOrAddComponent<ARTrackingModeSwitcher>(switcherObj);
            var modelLoader = GetOrAddComponent<RuntimeModelLoader>(switcherObj);

            // 2. Asegurar Zappar Image Tracking Target con PersistentMarkerController
            GameObject imageTrackerObj = FindOrCreateGameObject("Zappar Image Tracking Target");
            var imageTarget = GetOrAddComponent<ZapparImageTrackingTarget>(imageTrackerObj);
            imageTarget.Target = "marcador_logo.zpt";
            imageTarget.Orientation = ZapparImageTrackingTarget.PlaneOrientation.Flat;

            var persistentCtrl = GetOrAddComponent<PersistentMarkerController>(imageTrackerObj);
            persistentCtrl.ElevationOffset = 0.01f;

            HideMarkerPreviewsInEditor(imageTrackerObj, imageTarget);

            // 3. Vincular referencias en el Switcher
            GameObject instantRoot = GameObject.Find("Zappar Instant Tracking Target");
            switcher.InstantTrackingRoot = instantRoot;
            switcher.ImageTrackingRoot = imageTrackerObj;

            // 4. Asegurar InstantTrackingController en el target de superficie
            if (instantRoot != null)
            {
                var instantTarget = instantRoot.GetComponent<ZapparInstantTrackingTarget>();
                if (instantTarget != null)
                {
                    instantTarget.MoveAnchorOnZ = false;
                }
                var instantCtrl = GetOrAddComponent<InstantTrackingController>(instantRoot);
                instantCtrl.ShowPlaneVisualizer = true;
            }

            // 5. Configurar Contenedor de Modelo de Instant Tracking
            GameObject instantCube = GameObject.Find("Cube");
            if (instantCube != null)
            {
                ResetContainerTransformAndTouch(instantCube);

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
            Transform existingCubeInTarget = imageTrackerObj.transform.Find("Cube_ImageTarget");
            if (existingCubeInTarget == null && instantCube != null)
            {
                GameObject newCube = Instantiate(instantCube, imageTrackerObj.transform);
                newCube.name = "Cube_ImageTarget";
                ResetContainerTransformAndTouch(newCube);
                persistentCtrl.TargetModel = newCube.transform;
                modelLoader.MarkerModelContainer = newCube.transform;
            }
            else if (existingCubeInTarget != null)
            {
                ResetContainerTransformAndTouch(existingCubeInTarget.gameObject);
                persistentCtrl.TargetModel = existingCubeInTarget;
                modelLoader.MarkerModelContainer = existingCubeInTarget;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[ViMARA] Escena AR configurada con RuntimeModelLoader y shaders URP/glTFast protegidos.");
        }

        private static GameObject FindOrCreateGameObject(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj != null ? obj : new GameObject(name);
        }

        private static T GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            T comp = obj.GetComponent<T>();
            return comp != null ? comp : obj.AddComponent<T>();
        }

        private static void ResetContainerTransformAndTouch(GameObject containerObj)
        {
            containerObj.transform.localPosition = Vector3.zero;
            containerObj.transform.localScale = Vector3.one;
            GetOrAddComponent<TouchManipulationController>(containerObj);
        }

        private static void HideMarkerPreviewsInEditor(GameObject imageTrackerObj, ZapparImageTrackingTarget imageTarget)
        {
            if (imageTarget.PreviewImageObject != null)
            {
                imageTarget.PreviewImageObject.SetActive(false);
                var previewRenderer = imageTarget.PreviewImageObject.GetComponent<Renderer>();
                if (previewRenderer != null) previewRenderer.enabled = false;
            }

            Transform previewChild = imageTrackerObj.transform.Find("Preview Object");
            if (previewChild != null)
            {
                previewChild.gameObject.SetActive(false);
                var previewRenderer = previewChild.GetComponent<Renderer>();
                if (previewRenderer != null) previewRenderer.enabled = false;
            }
        }

        private static void EnsureShadersAlwaysIncluded()
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if (graphicsSettingsObj == null) return;

            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            if (arrayProp == null) return;

            bool modified = false;
            foreach (string shaderName in RequiredShaders)
            {
                Shader shader = Shader.Find(shaderName);
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
}
