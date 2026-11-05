using System;
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
    /// 2. Modelos externos o locales en formato `.glb` / `.gltf` mediante glTFast (con conversión automática a URP/Lit para evitar texturas rosas en WebGL).
    /// 3. Modelos de malla arquitectónica `.obj` y `.stl` (ASCII y Binario) exportados desde SketchUp, Revit, Rhino o Blender.
    /// Normaliza automáticamente el tamaño (Bounding Box) y apoya la base del modelo exactamente en Y = 0
    /// tanto en el contenedor de Seguimiento por Superficie como en el de Seguimiento por Marcador.
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
        private bool m_isLoading = false;

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

        private Material CreateCleanUrpMaterial()
        {
            CacheReferenceUrpMaterial();
            if (BaseUrpMaterial != null)
            {
                return new Material(BaseUrpMaterial);
            }

            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader == null) litShader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (litShader == null) litShader = Shader.Find("Standard");
            if (litShader == null) litShader = Shader.Find("Sprites/Default");

            return new Material(litShader);
        }

        public void InitializeContainers(Transform surfaceContainer, Transform markerContainer)
        {
            SurfaceModelContainer = surfaceContainer;
            MarkerModelContainer = markerContainer;
            CacheReferenceUrpMaterial();
        }

        /// <summary>
        /// Método invocable desde JavaScript (SendMessage) o desde ARTrackingModeSwitcher.
        /// Formato de payload aceptado:
        /// - "url|format" (ej. "blob:https://...|stl" o "https://.../model.glb|glb")
        /// - "preset:house|preset"
        /// </summary>
        public async void LoadModelFromPayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return;
            if (m_isLoading && payload == m_currentModelPayload) return;

            m_currentModelPayload = payload;
            string url = payload;
            string format = "";

            int pipeIdx = payload.LastIndexOf('|');
            if (pipeIdx > 0)
            {
                url = payload.Substring(0, pipeIdx).Trim();
                format = payload.Substring(pipeIdx + 1).Trim().ToLowerInvariant();
            }
            else
            {
                format = InferFormatFromUrl(url);
            }

            Debug.Log($"[ViMARA Loader] Iniciando carga de modelo: URL='{url}', Formato='{format}'");
            await LoadAndPopulateAsync(url, format);
        }

        private string InferFormatFromUrl(string url)
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

                // 1. Caso: Modelo de prueba procedural integrado
                if (url.StartsWith("preset:", StringComparison.OrdinalIgnoreCase) || format == "preset")
                {
                    string presetName = url.StartsWith("preset:", StringComparison.OrdinalIgnoreCase)
                        ? url.Substring("preset:".Length)
                        : url;

                    PopulateContainerWithPreset(SurfaceModelContainer, presetName);
                    PopulateContainerWithPreset(MarkerModelContainer, presetName);
                    Debug.Log($"[ViMARA Loader] Maqueta de prueba '{presetName}' generada y normalizada.");
                    return;
                }

                // 2. Caso: Archivo .GLB / .GLTF vía glTFast
                if (format == "glb" || format == "gltf")
                {
                    byte[] glbBytes = await DownloadBytesAsync(url);
                    if (glbBytes != null && glbBytes.Length > 0)
                    {
                        await PopulateContainerWithGltfBytesAsync(SurfaceModelContainer, glbBytes, url);
                        await PopulateContainerWithGltfBytesAsync(MarkerModelContainer, glbBytes, url);
                        Debug.Log("[ViMARA Loader] Modelo GLB/GLTF cargado e instanciado en ambos modos.");
                    }
                    else
                    {
                        await PopulateContainerWithGltfUrlAsync(SurfaceModelContainer, url);
                        await PopulateContainerWithGltfUrlAsync(MarkerModelContainer, url);
                    }
                    return;
                }

                // 3. Caso: Archivo .STL (Binario o ASCII)
                if (format == "stl")
                {
                    byte[] stlBytes = await DownloadBytesAsync(url);
                    if (stlBytes != null && stlBytes.Length > 0)
                    {
                        Mesh stlMesh = RuntimeMeshFormats.LoadStlFromBytes(stlBytes);
                        if (stlMesh != null)
                        {
                            PopulateContainerWithMesh(SurfaceModelContainer, stlMesh, "Imported_STL_Model");
                            PopulateContainerWithMesh(MarkerModelContainer, stlMesh, "Imported_STL_Model");
                            Debug.Log("[ViMARA Loader] Modelo STL cargado y normalizado exitosamente.");
                        }
                    }
                    return;
                }

                // 4. Caso: Archivo .OBJ
                if (format == "obj")
                {
                    byte[] objBytes = await DownloadBytesAsync(url);
                    if (objBytes != null && objBytes.Length > 0)
                    {
                        string objText = Encoding.UTF8.GetString(objBytes);
                        Mesh objMesh = RuntimeMeshFormats.LoadObjFromText(objText);
                        if (objMesh != null)
                        {
                            PopulateContainerWithMesh(SurfaceModelContainer, objMesh, "Imported_OBJ_Model");
                            PopulateContainerWithMesh(MarkerModelContainer, objMesh, "Imported_OBJ_Model");
                            Debug.Log("[ViMARA Loader] Modelo OBJ cargado y normalizado exitosamente.");
                        }
                    }
                    return;
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

        private async Task<byte[]> DownloadBytesAsync(string url)
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

        private void PopulateContainerWithPreset(Transform container, string presetName)
        {
            if (container == null) return;
            PrepareContainerForNewContent(container);

            GameObject wrapper = CreateNormalizedContentWrapper(container, $"Preset_{presetName}");
            GameObject model = ProceduralSampleBuilder.CreatePreset(presetName);
            model.transform.SetParent(wrapper.transform, false);

            RepairAndUpgradeMaterialsToUrp(wrapper);
            NormalizeAndGroundContent(container, wrapper);
        }

        private void PopulateContainerWithMesh(Transform container, Mesh mesh, string modelName)
        {
            if (container == null || mesh == null) return;
            PrepareContainerForNewContent(container);

            GameObject wrapper = CreateNormalizedContentWrapper(container, modelName);
            GameObject meshObj = new GameObject("ArchitecturalMesh");
            meshObj.transform.SetParent(wrapper.transform, false);

            MeshFilter mf = meshObj.AddComponent<MeshFilter>();
            mf.sharedMesh = mesh;

            MeshRenderer mr = meshObj.AddComponent<MeshRenderer>();
            Material archMat = CreateCleanUrpMaterial();
            Color warmWhite = new Color(0.90f, 0.91f, 0.93f);
            archMat.color = warmWhite;
            if (archMat.HasProperty("_BaseColor")) archMat.SetColor("_BaseColor", warmWhite);
            if (archMat.HasProperty("_Smoothness")) archMat.SetFloat("_Smoothness", 0.25f);
            mr.sharedMaterial = archMat;

            NormalizeAndGroundContent(container, wrapper);
        }

        private async Task PopulateContainerWithGltfBytesAsync(Transform container, byte[] data, string originalUrl)
        {
            if (container == null) return;
            PrepareContainerForNewContent(container);

            GameObject wrapper = CreateNormalizedContentWrapper(container, "GLTF_Model");
            var gltf = new GltfImport();
            Uri baseUri = null;
            if (Uri.TryCreate(originalUrl, UriKind.Absolute, out Uri parsedUri) && !originalUrl.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
            {
                baseUri = parsedUri;
            }

            bool success = await gltf.LoadGltfBinary(data, baseUri);
            if (!success)
            {
                string json = Encoding.UTF8.GetString(data);
                success = await gltf.LoadGltfJson(json, baseUri);
            }

            if (success)
            {
                await gltf.InstantiateMainSceneAsync(wrapper.transform);
                RepairAndUpgradeMaterialsToUrp(wrapper);
                NormalizeAndGroundContent(container, wrapper);
            }
            else
            {
                Debug.LogError("[ViMARA Loader] glTFast no pudo decodificar los bytes del archivo.");
            }
        }

        private async Task PopulateContainerWithGltfUrlAsync(Transform container, string url)
        {
            if (container == null) return;
            PrepareContainerForNewContent(container);

            GameObject wrapper = CreateNormalizedContentWrapper(container, "GLTF_Model");
            var gltf = new GltfImport();
            bool success = await gltf.Load(url);
            if (success)
            {
                await gltf.InstantiateMainSceneAsync(wrapper.transform);
                RepairAndUpgradeMaterialsToUrp(wrapper);
                NormalizeAndGroundContent(container, wrapper);
            }
            else
            {
                Debug.LogError($"[ViMARA Loader] glTFast falló al cargar desde URL: {url}");
            }
        }

        /// <summary>
        /// Repara cualquier material de glTFast cuyo shader haya sido eliminado en WebGL (color magenta/rosa)
        /// transfiriendo todas sus texturas PBR (BaseMap, NormalMap, Metallic, Emission, Occlusion) y colores
        /// al shader Universal Render Pipeline/Lit garantizado en escena.
        /// </summary>
        private void RepairAndUpgradeMaterialsToUrp(GameObject rootObj)
        {
            if (rootObj == null) return;
            Renderer[] renderers = rootObj.GetComponentsInChildren<Renderer>(true);
            if (renderers == null) return;

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
                        sharedMats[i] = CreateCleanUrpMaterial();
                        updated = true;
                        continue;
                    }

                    bool needsUpgrade = oldMat.shader == null ||
                                        !oldMat.shader.isSupported ||
                                        oldMat.shader.name.Contains("Error") ||
                                        oldMat.shader.name.Contains("glTF");

                    if (!needsUpgrade) continue;

                    Material newMat = CreateCleanUrpMaterial();
                    newMat.name = oldMat.name + "_URP";

                    // 1. Extraer color base
                    Color baseCol = Color.white;
                    if (oldMat.HasProperty("baseColorFactor")) baseCol = oldMat.GetColor("baseColorFactor");
                    else if (oldMat.HasProperty("_BaseColor")) baseCol = oldMat.GetColor("_BaseColor");
                    else if (oldMat.HasProperty("_Color")) baseCol = oldMat.GetColor("_Color");

                    newMat.color = baseCol;
                    if (newMat.HasProperty("_BaseColor")) newMat.SetColor("_BaseColor", baseCol);
                    if (newMat.HasProperty("_Color")) newMat.SetColor("_Color", baseCol);

                    // 2. Extraer textura principal (Albedo / BaseColor)
                    Texture albedoTex = null;
                    if (oldMat.HasProperty("baseColorTexture")) albedoTex = oldMat.GetTexture("baseColorTexture");
                    if (albedoTex == null && oldMat.HasProperty("_BaseMap")) albedoTex = oldMat.GetTexture("_BaseMap");
                    if (albedoTex == null && oldMat.HasProperty("_MainTex")) albedoTex = oldMat.GetTexture("_MainTex");
                    if (albedoTex == null) albedoTex = oldMat.mainTexture;

                    if (albedoTex != null)
                    {
                        newMat.mainTexture = albedoTex;
                        if (newMat.HasProperty("_BaseMap")) newMat.SetTexture("_BaseMap", albedoTex);
                        if (newMat.HasProperty("_MainTex")) newMat.SetTexture("_MainTex", albedoTex);
                    }

                    // 3. Extraer Normal Map
                    Texture normalTex = null;
                    if (oldMat.HasProperty("normalTexture")) normalTex = oldMat.GetTexture("normalTexture");
                    if (normalTex == null && oldMat.HasProperty("_BumpMap")) normalTex = oldMat.GetTexture("_BumpMap");
                    if (normalTex != null && newMat.HasProperty("_BumpMap"))
                    {
                        newMat.SetTexture("_BumpMap", normalTex);
                        newMat.EnableKeyword("_NORMALMAP");
                    }

                    // 4. Extraer Emissive Map y Color
                    Texture emissiveTex = null;
                    if (oldMat.HasProperty("emissiveTexture")) emissiveTex = oldMat.GetTexture("emissiveTexture");
                    if (emissiveTex == null && oldMat.HasProperty("_EmissionMap")) emissiveTex = oldMat.GetTexture("_EmissionMap");
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

                    // 5. Extraer Occlusion Map
                    Texture occTex = null;
                    if (oldMat.HasProperty("occlusionTexture")) occTex = oldMat.GetTexture("occlusionTexture");
                    if (occTex == null && oldMat.HasProperty("_OcclusionMap")) occTex = oldMat.GetTexture("_OcclusionMap");
                    if (occTex != null && newMat.HasProperty("_OcclusionMap"))
                    {
                        newMat.SetTexture("_OcclusionMap", occTex);
                    }

                    // 6. Ajustar Metallic y Smoothness
                    float metallic = 0f;
                    if (oldMat.HasProperty("metallicFactor")) metallic = oldMat.GetFloat("metallicFactor");
                    else if (oldMat.HasProperty("_Metallic")) metallic = oldMat.GetFloat("_Metallic");

                    float smoothness = 0.45f;
                    if (oldMat.HasProperty("roughnessFactor")) smoothness = Mathf.Clamp01(1f - oldMat.GetFloat("roughnessFactor") * 0.65f);
                    else if (oldMat.HasProperty("_Smoothness")) smoothness = oldMat.GetFloat("_Smoothness");

                    // Si tiene textura metálica pero URP usa el canal Alpha en lugar de G, usamos valores equilibrados para evitar que se vea negro
                    if (newMat.HasProperty("_Metallic")) newMat.SetFloat("_Metallic", Mathf.Clamp(metallic, 0f, 0.35f));
                    if (newMat.HasProperty("_Smoothness")) newMat.SetFloat("_Smoothness", smoothness);

                    sharedMats[i] = newMat;
                    updated = true;
                }

                if (updated)
                {
                    r.sharedMaterials = sharedMats;
                }
            }
        }

        /// <summary>
        /// Oculta el MeshRenderer del cubo de prueba original y elimina modelos cargados previamente.
        /// </summary>
        private void PrepareContainerForNewContent(Transform container)
        {
            CacheReferenceUrpMaterial();

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

        private GameObject CreateNormalizedContentWrapper(Transform container, string name)
        {
            GameObject wrapper = new GameObject(name);
            wrapper.transform.SetParent(container, false);
            wrapper.transform.localPosition = Vector3.zero;
            wrapper.transform.localRotation = Quaternion.identity;
            wrapper.transform.localScale = Vector3.one;
            return wrapper;
        }

        /// <summary>
        /// Calcula el Bounding Box total en espacio del contenedor, normaliza la escala
        /// y centra el modelo en X/Z haciendo que la base inferior descanse exactamente en Y = 0.
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
