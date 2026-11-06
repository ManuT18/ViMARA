using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityEngine.Networking;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestor universal de carga de modelos 3D en tiempo de ejecución para ViMARA WebAR.
    /// Soporta:
    /// 1. Maquetas de prueba procedurales instantáneas (`preset:house`, `preset:tower`, `preset:pavilion`).
    /// 2. Modelos externos o locales `.glb` / `.gltf` mediante glTFast con conversión automática a URP/Lit.
    /// 3. Modelos de malla arquitectónica `.obj` y `.stl` (ASCII y Binario).
    /// Incluye liberación explícita de recursos dinámicos (mallas, materiales e instancias glTFast)
    /// y reutilización de jerarquía normalizada entre los contenedores de Superficie y Marcador.
    /// </summary>
    public class RuntimeModelLoader : MonoBehaviour
    {
        public static RuntimeModelLoader Instance { get; private set; }

        [Header("Contenedores de Maqueta en Escena")]
        public Transform SurfaceModelContainer;
        public Transform MarkerModelContainer;

        [Header("Material Base URP (Referencia contra Shader Stripping)")]
        [Tooltip("Material URP/Lit de referencia en la escena para garantizar soporte de texturas en WebGL.")]
        public Material BaseUrpMaterial;

        [Header("Dimensiones Normalizadas (Metros)")]
        [Tooltip("Tamaño máximo deseado para la dimensión mayor del modelo al cargarse.")]
        public float TargetNormalizedSize = 0.45f;

        private string m_currentModelPayload;
        private bool m_isLoading;

        // Seguimiento de recursos dinámicos para prevenir fugas de memoria en WebGL
        private GltfImport m_activeGltfImport;
        private readonly HashSet<Material> m_allocatedMaterials = new HashSet<Material>();
        private readonly HashSet<Mesh> m_allocatedMeshes = new HashSet<Mesh>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            CacheReferenceUrpMaterial();
        }

        private void OnDestroy()
        {
            ReleaseAllocatedResources();
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void InitializeContainers(Transform surfaceContainer, Transform markerContainer)
        {
            SurfaceModelContainer = surfaceContainer;
            MarkerModelContainer = markerContainer;
            CacheReferenceUrpMaterial();
        }

        /// <summary>
        /// Invocable desde JavaScript (SendMessage) o desde ARTrackingModeSwitcher.
        /// Formato de payload: "url|format" o "preset:house|preset".
        /// </summary>
        public async void LoadModelFromPayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return;
            if (m_isLoading && payload == m_currentModelPayload) return;

            m_currentModelPayload = payload;
            ParsePayload(payload, out string url, out string format);

            Debug.Log($"[ViMARA Loader] Cargando modelo: URL='{url}', Formato='{format}'");
            await LoadAndPopulateAsync(url, format);
        }

        private static void ParsePayload(string payload, out string url, out string format)
        {
            int pipeIdx = payload.LastIndexOf('|');
            if (pipeIdx > 0)
            {
                url = payload.Substring(0, pipeIdx).Trim();
                format = payload.Substring(pipeIdx + 1).Trim().ToLowerInvariant();
            }
            else
            {
                url = payload.Trim();
                format = InferFormatFromUrl(url);
            }
        }

        private static string InferFormatFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return "glb";
            if (url.StartsWith("preset:", StringComparison.OrdinalIgnoreCase)) return "preset";

            string cleanUrl = url.Split('?')[0].ToLowerInvariant();
            if (cleanUrl.EndsWith(".stl")) return "stl";
            if (cleanUrl.EndsWith(".obj")) return "obj";
            if (cleanUrl.EndsWith(".gltf")) return "gltf";
            return "glb";
        }

        private async Task LoadAndPopulateAsync(string url, string format)
        {
            m_isLoading = true;
            try
            {
                EnsureContainersResolved();
                CacheReferenceUrpMaterial();

                Transform primaryContainer = SurfaceModelContainer != null ? SurfaceModelContainer : MarkerModelContainer;
                Transform secondaryContainer = (primaryContainer == SurfaceModelContainer) ? MarkerModelContainer : null;

                if (primaryContainer == null)
                {
                    Debug.LogWarning("[ViMARA Loader] No se encontraron contenedores de modelo en la escena.");
                    return;
                }

                // 1. Maqueta de prueba procedural integrada
                if (url.StartsWith("preset:", StringComparison.OrdinalIgnoreCase) || format == "preset")
                {
                    string presetName = url.StartsWith("preset:", StringComparison.OrdinalIgnoreCase)
                        ? url.Substring("preset:".Length)
                        : url;

                    ResetBothContainersAndResources();
                    GameObject wrapper = PopulatePrimaryWithPreset(primaryContainer, presetName);
                    CloneWrapperToSecondary(wrapper, secondaryContainer);
                    return;
                }

                // 2. Archivo .GLB / .GLTF vía glTFast (decodificación única en WASM y clonación al contenedor secundario)
                if (format == "glb" || format == "gltf")
                {
                    byte[] glbBytes = await DownloadBytesAsync(url);
                    ResetBothContainersAndResources();

                    GameObject wrapper = (glbBytes != null && glbBytes.Length > 0)
                        ? await PopulatePrimaryWithGltfBytesAsync(primaryContainer, glbBytes, url)
                        : await PopulatePrimaryWithGltfUrlAsync(primaryContainer, url);

                    CloneWrapperToSecondary(wrapper, secondaryContainer);
                    return;
                }

                // 3. Archivo .STL (Binario o ASCII)
                if (format == "stl")
                {
                    byte[] stlBytes = await DownloadBytesAsync(url);
                    if (stlBytes == null || stlBytes.Length == 0) return;

                    Mesh stlMesh = RuntimeMeshFormats.LoadStlFromBytes(stlBytes);
                    if (stlMesh == null) return;

                    ResetBothContainersAndResources();
                    m_allocatedMeshes.Add(stlMesh);

                    GameObject wrapper = PopulatePrimaryWithMesh(primaryContainer, stlMesh, "Imported_STL_Model");
                    CloneWrapperToSecondary(wrapper, secondaryContainer);
                    return;
                }

                // 4. Archivo .OBJ
                if (format == "obj")
                {
                    byte[] objBytes = await DownloadBytesAsync(url);
                    if (objBytes == null || objBytes.Length == 0) return;

                    string objText = Encoding.UTF8.GetString(objBytes);
                    Mesh objMesh = RuntimeMeshFormats.LoadObjFromText(objText);
                    if (objMesh == null) return;

                    ResetBothContainersAndResources();
                    m_allocatedMeshes.Add(objMesh);

                    GameObject wrapper = PopulatePrimaryWithMesh(primaryContainer, objMesh, "Imported_OBJ_Model");
                    CloneWrapperToSecondary(wrapper, secondaryContainer);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ViMARA Loader] Error cargando el modelo ({url}): {ex.Message}");
            }
            finally
            {
                m_isLoading = false;
            }
        }

        private void EnsureContainersResolved()
        {
            if (SurfaceModelContainer == null)
            {
                var instantCtrl = FindFirstObjectByType<InstantTrackingController>(FindObjectsInactive.Include);
                if (instantCtrl != null && instantCtrl.ModelContainer != null)
                {
                    SurfaceModelContainer = instantCtrl.ModelContainer;
                }
            }

            if (MarkerModelContainer == null)
            {
                var markerCtrl = FindFirstObjectByType<PersistentMarkerController>(FindObjectsInactive.Include);
                if (markerCtrl != null && markerCtrl.TargetModel != null)
                {
                    MarkerModelContainer = markerCtrl.TargetModel;
                }
            }
        }

        private void CacheReferenceUrpMaterial()
        {
            if (BaseUrpMaterial != null) return;

            EnsureContainersResolved();
            if (SurfaceModelContainer != null)
            {
                MeshRenderer mr = SurfaceModelContainer.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != null)
                {
                    BaseUrpMaterial = mr.sharedMaterial;
                    return;
                }
            }

            if (MarkerModelContainer != null)
            {
                MeshRenderer mr = MarkerModelContainer.GetComponent<MeshRenderer>();
                if (mr != null && mr.sharedMaterial != null)
                {
                    BaseUrpMaterial = mr.sharedMaterial;
                }
            }
        }

        private Material CreateTrackedUrpMaterial()
        {
            CacheReferenceUrpMaterial();
            Material mat;
            if (BaseUrpMaterial != null)
            {
                mat = new Material(BaseUrpMaterial);
            }
            else
            {
                Shader litShader = Shader.Find("Universal Render Pipeline/Lit")
                                ?? Shader.Find("Universal Render Pipeline/Simple Lit")
                                ?? Shader.Find("Standard")
                                ?? Shader.Find("Sprites/Default");
                mat = new Material(litShader);
            }

            m_allocatedMaterials.Add(mat);
            return mat;
        }

        private static async Task<byte[]> DownloadBytesAsync(string url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                var operation = www.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogWarning($"[ViMARA Loader] No se pudo descargar bytes desde '{url}': {www.error}");
                    return null;
                }

                return www.downloadHandler.data;
            }
        }

        private GameObject PopulatePrimaryWithPreset(Transform container, string presetName)
        {
            GameObject wrapper = CreateNormalizedContentWrapper(container, $"Preset_{presetName}");
            GameObject model = ProceduralSampleBuilder.CreatePreset(presetName, BaseUrpMaterial);
            model.transform.SetParent(wrapper.transform, false);

            RegisterMaterialsFromHierarchy(wrapper);
            RepairAndUpgradeMaterialsToUrp(wrapper);
            NormalizeAndGroundContent(container, wrapper);
            return wrapper;
        }

        private GameObject PopulatePrimaryWithMesh(Transform container, Mesh mesh, string modelName)
        {
            GameObject wrapper = CreateNormalizedContentWrapper(container, modelName);
            GameObject meshObj = new GameObject("ArchitecturalMesh");
            meshObj.transform.SetParent(wrapper.transform, false);

            MeshFilter mf = meshObj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
            Material archMat = CreateTrackedUrpMaterial();
            Color warmWhite = new Color(0.90f, 0.91f, 0.93f);
            archMat.color = warmWhite;
            if (archMat.HasProperty("_BaseColor")) archMat.SetColor("_BaseColor", warmWhite);
            if (archMat.HasProperty("_Smoothness")) archMat.SetFloat("_Smoothness", 0.25f);
            mr.sharedMaterial = archMat;

            NormalizeAndGroundContent(container, wrapper);
            return wrapper;
        }

        private async Task<GameObject> PopulatePrimaryWithGltfBytesAsync(Transform container, byte[] data, string originalUrl)
        {
            GameObject wrapper = CreateNormalizedContentWrapper(container, "GLTF_Model");
            m_activeGltfImport = new GltfImport();

            Uri baseUri = null;
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri parsedUri) &&
                !originalUrl.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                baseUri = parsedUri;
            }

            bool success = await m_activeGltfImport.LoadGltfBinary(data, baseUri);
            if (!success)
            {
                string json = Encoding.UTF8.GetString(data);
                success = await m_activeGltfImport.LoadGltfJson(json, baseUri);
            }

            if (!success)
            {
                Debug.LogError("[ViMARA Loader] glTFast no pudo decodificar los bytes del archivo.");
                return null;
            }

            await m_activeGltfImport.InstantiateMainSceneAsync(wrapper.transform);
            RepairAndUpgradeMaterialsToUrp(wrapper);
            NormalizeAndGroundContent(container, wrapper);
            return wrapper;
        }

        private async Task<GameObject> PopulatePrimaryWithGltfUrlAsync(Transform container, string url)
        {
            GameObject wrapper = CreateNormalizedContentWrapper(container, "GLTF_Model");
            m_activeGltfImport = new GltfImport();

            bool success = await m_activeGltfImport.Load(url);
            if (!success)
            {
                Debug.LogError($"[ViMARA Loader] glTFast falló al cargar desde URL: {url}");
                return null;
            }

            await m_activeGltfImport.InstantiateMainSceneAsync(wrapper.transform);
            RepairAndUpgradeMaterialsToUrp(wrapper);
            NormalizeAndGroundContent(container, wrapper);
            return wrapper;
        }

        private static void CloneWrapperToSecondary(GameObject primaryWrapper, Transform secondaryContainer)
        {
            if (primaryWrapper == null || secondaryContainer == null) return;

            GameObject clone = Instantiate(primaryWrapper, secondaryContainer, false);
            clone.name = primaryWrapper.name;
            clone.transform.localPosition = primaryWrapper.transform.localPosition;
            clone.transform.localRotation = primaryWrapper.transform.localRotation;
            clone.transform.localScale = primaryWrapper.transform.localScale;
        }

        private void RegisterMaterialsFromHierarchy(GameObject rootObj)
        {
            if (rootObj == null) return;
            Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                foreach (Material mat in r.sharedMaterials)
                {
                    if (mat != null && mat != BaseUrpMaterial)
                    {
                        m_allocatedMaterials.Add(mat);
                    }
                }
            }
        }

        /// <summary>
        /// Repara materiales de glTFast cuyo shader haya sido eliminado en WebGL (color magenta),
        /// transfiriendo texturas PBR y propiedades al shader Universal Render Pipeline/Lit garantizado.
        /// </summary>
        private void RepairAndUpgradeMaterialsToUrp(GameObject rootObj)
        {
            if (rootObj == null) return;
            Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>(true);
            if (renderers == null) return;

            var upgradedLookup = new Dictionary<Material, Material>();

            foreach (Renderer r in renderers)
            {
                Material[] sharedMats = r.sharedMaterials;
                if (sharedMats == null || sharedMats.Length == 0) continue;

                bool updated = false;
                for (int i = 0; i < sharedMats.Length; i++)
                {
                    Material oldMat = sharedMats[i];
                    if (oldMat == null)
                    {
                        sharedMats[i] = CreateTrackedUrpMaterial();
                        updated = true;
                        continue;
                    }

                    bool needsUpgrade = oldMat.shader == null ||
                                        !oldMat.shader.isSupported ||
                                        oldMat.shader.name.Contains("Error") ||
                                        oldMat.shader.name.Contains("glTF");

                    if (!needsUpgrade) continue;

                    if (!upgradedLookup.TryGetValue(oldMat, out Material newMat))
                    {
                        newMat = ConvertMaterialToUrp(oldMat);
                        upgradedLookup[oldMat] = newMat;
                    }

                    sharedMats[i] = newMat;
                    updated = true;
                }

                if (updated)
                {
                    r.sharedMaterials = sharedMats;
                }
            }
        }

        private Material ConvertMaterialToUrp(Material oldMat)
        {
            Material newMat = CreateTrackedUrpMaterial();
            newMat.name = oldMat.name + "_URP";

            // 1. Color base
            Color baseCol = Color.white;
            if (oldMat.HasProperty("baseColorFactor")) baseCol = oldMat.GetColor("baseColorFactor");
            else if (oldMat.HasProperty("_BaseColor")) baseCol = oldMat.GetColor("_BaseColor");
            else if (oldMat.HasProperty("_Color")) baseCol = oldMat.GetColor("_Color");

            newMat.color = baseCol;
            if (newMat.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", baseCol);
            if (newMat.HasProperty("_Color")) newMat.SetColor("_Color", baseCol);

            // 2. Textura Albedo / BaseMap
            Texture albedoTex = GetFirstAvailableTexture(oldMat, "baseColorTexture", "_BaseMap", "_MainTex") ?? oldMat.mainTexture;
            if (albedoTex != null)
            {
                newMat.mainTexture = albedoTex;
                if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", albedoTex);
                if (newMat.HasProperty("_MainTex")) newMat.SetTexture("_MainTex", albedoTex);
            }

            // 3. Normal Map
            Texture normalTex = GetFirstAvailableTexture(oldMat, "normalTexture", "_BumpMap");
            if (normalTex != null && newMat.HasProperty("_BumpMap"))
            {
                newMat.SetTexture("_BumpMap", normalTex);
                newMat.EnableKeyword("_NORMALMAP");
            }

            // 4. Emissive Map y Color
            Texture emissiveTex = GetFirstAvailableTexture(oldMat, "emissiveTexture", "_EmissionMap");
            Color emissiveCol = Color.black;
            if (oldMat.HasProperty("emissiveFactor")) emissiveCol = oldMat.GetColor("emissiveFactor");
            else if (oldMat.HasProperty("_EmissionColor")) emissiveCol = oldMat.GetColor("_EmissionColor");

            if (emissiveTex != null || emissiveCol != Color.black)
            {
                newMat.EnableKeyword("_EMISSION");
                if (emissiveTex != null && newMat.HasProperty("_EmissionMap"))
                    newMat.SetTexture("_EmissionMap", emissiveTex);
                if (newMat.HasProperty("_EmissionColor"))
                    newMat.SetColor("_EmissionColor", emissiveCol == Color.black ? Color.white : emissiveCol);
            }

            // 5. Occlusion Map
            Texture occTex = GetFirstAvailableTexture(oldMat, "occlusionTexture", "_OcclusionMap");
            if (occTex != null && newMat.HasProperty("_OcclusionMap"))
            {
                newMat.SetTexture("_OcclusionMap", occTex);
            }

            // 6. Metallic y Smoothness equilibrados para URP
            float metallic = 0f;
            if (oldMat.HasProperty("metallicFactor")) metallic = oldMat.GetFloat("metallicFactor");
            else if (oldMat.HasProperty("_Metallic")) metallic = oldMat.GetFloat("_Metallic");

            float smoothness = 0.45f;
            if (oldMat.HasProperty("roughnessFactor")) smoothness = Mathf.Clamp01(1f - oldMat.GetFloat("roughnessFactor") * 0.65f);
            else if (oldMat.HasProperty("_Smoothness")) smoothness = oldMat.GetFloat("_Smoothness");

            if (newMat.HasProperty("_Metallic")) newMat.SetFloat("_Metallic", Mathf.Clamp(metallic, 0f, 0.35f));
            if (newMat.HasProperty("_Smoothness")) newMat.SetFloat("_Smoothness", smoothness);

            return newMat;
        }

        private static Texture GetFirstAvailableTexture(Material mat, params string[] propertyNames)
        {
            foreach (string prop in propertyNames)
            {
                if (mat.HasProperty(prop))
                {
                    Texture tex = mat.GetTexture(prop);
                    if (tex != null) return tex;
                }
            }
            return null;
        }

        private void ResetBothContainersAndResources()
        {
            CacheReferenceUrpMaterial();
            PrepareContainerForNewContent(SurfaceModelContainer);
            PrepareContainerForNewContent(MarkerModelContainer);
            ReleaseAllocatedResources();
        }

        private void ReleaseAllocatedResources()
        {
            if (m_activeGltfImport != null)
            {
                m_activeGltfImport.Dispose();
                m_activeGltfImport = null;
            }

            foreach (Material mat in m_allocatedMaterials)
            {
                if (mat != null && mat != BaseUrpMaterial)
                {
                    Destroy(mat);
                }
            }
            m_allocatedMaterials.Clear();

            foreach (Mesh mesh in m_allocatedMeshes)
            {
                if (mesh != null)
                {
                    Destroy(mesh);
                }
            }
            m_allocatedMeshes.Clear();
        }

        private static void PrepareContainerForNewContent(Transform container)
        {
            if (container == null) return;

            MeshRenderer rootMeshRenderer = container.GetComponent<MeshRenderer>();
            if (rootMeshRenderer != null) rootMeshRenderer.enabled = false;

            Collider rootCollider = container.GetComponent<Collider>();
            if (rootCollider != null) rootCollider.enabled = false;

            for (int i = container.childCount - 1; i >= 0; i--)
            {
                Destroy(container.GetChild(i).gameObject);
            }

            container.localPosition = Vector3.zero;
            container.localRotation = Quaternion.identity;
            container.localScale = Vector3.one;

            var touchCtrl = container.GetComponent<TouchManipulationController>();
            if (touchCtrl != null)
            {
                touchCtrl.ResetTransformation(Vector3.one);
            }
        }

        private static GameObject CreateNormalizedContentWrapper(Transform container, string name)
        {
            GameObject wrapper = new GameObject(name);
            wrapper.transform.SetParent(container, false);
            wrapper.transform.localPosition = Vector3.zero;
            wrapper.transform.localRotation = Quaternion.identity;
            wrapper.transform.localScale = Vector3.one;
            return wrapper;
        }

        /// <summary>
        /// Calcula el Bounding Box total en el espacio del contenedor, normaliza la escala
        /// y centra el modelo en X/Z apoyando su base exactamente sobre Y = 0.
        /// </summary>
        private void NormalizeAndGroundContent(Transform container, GameObject wrapper)
        {
            Renderer[] renderers = wrapper.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0) return;

            Vector3 savedPos = container.position;
            Quaternion savedRot = container.rotation;
            Vector3 savedScale = container.localScale;

            container.position = Vector3.zero;
            container.rotation = Quaternion.identity;
            container.localScale = Vector3.one;

            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }

            float maxDimension = Mathf.Max(combinedBounds.size.x, Mathf.Max(combinedBounds.size.y, combinedBounds.size.z));
            if (maxDimension > 0.0001f)
            {
                float scaleFactor = TargetNormalizedSize / maxDimension;
                wrapper.transform.localScale = Vector3.one * scaleFactor;

                combinedBounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    combinedBounds.Encapsulate(renderers[i].bounds);
                }

                Vector3 offset = new Vector3(
                    -combinedBounds.center.x,
                    -combinedBounds.min.y,
                    -combinedBounds.center.z
                );
                wrapper.transform.localPosition += offset;
            }

            container.position = savedPos;
            container.rotation = savedRot;
            container.localScale = savedScale;
        }
    }
}
