using System;
using UnityEngine;
using UnityEngine.Networking;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestiona el modo activo de Realidad Aumentada (Superficie vs Marcador)
    /// y coordina la carga del modelo 3D seleccionado desde la URL o desde mensajes de React.
    /// </summary>
    public class ARTrackingModeSwitcher : MonoBehaviour
    {
        [Header("Referencias a los Targets")]
        [Tooltip("Objeto raíz para el seguimiento por superficie/plano (Instant Tracking).")]
        public GameObject InstantTrackingRoot;

        [Tooltip("Objeto raíz para el seguimiento por marcador/imagen (Image Tracking).")]
        public GameObject ImageTrackingRoot;

        [Header("Configuración por Defecto")]
        public TrackingMode DefaultMode = TrackingMode.Surface;

        public enum TrackingMode
        {
            Surface,
            Marker
        }

        private TrackingMode m_currentMode;
        private RuntimeModelLoader m_modelLoader;
        private string m_initialModelPayload;

        private void Awake()
        {
            Input.simulateMouseWithTouches = false;
            Application.targetFrameRate = -1;

            m_modelLoader = GetComponent<RuntimeModelLoader>();
            if (m_modelLoader == null)
            {
                m_modelLoader = gameObject.AddComponent<RuntimeModelLoader>();
            }

            ParseParametersFromURL();

            // CRÍTICO: Desactivar el tracker que NO se usa en Awake() ANTES de que Unity ejecute Start()
            // en ZapparInstantTrackingTarget o ZapparImageTrackingTarget. Así evitamos que ambos trackers
            // se registren e inicialicen simultáneamente en el pipeline nativo C++/WASM de Zappar.
            if (m_currentMode == TrackingMode.Marker)
            {
                if (InstantTrackingRoot != null) InstantTrackingRoot.SetActive(false);
                if (ImageTrackingRoot != null) ImageTrackingRoot.SetActive(true);
            }
            else
            {
                if (ImageTrackingRoot != null) ImageTrackingRoot.SetActive(false);
                if (InstantTrackingRoot != null) InstantTrackingRoot.SetActive(true);
            }
        }

        private void Start()
        {
            ResolveModelContainers();
            ApplyTrackingMode(m_currentMode);

            if (!string.IsNullOrEmpty(m_initialModelPayload))
            {
                m_modelLoader.LoadModelFromPayload(m_initialModelPayload);
            }
            else
            {
                // Si no se especificó modelo en la URL, cargar por defecto la Casa Moderna en lugar del cubo básico
                m_modelLoader.LoadModelFromPayload("preset:house|preset");
            }
        }

        private void ResolveModelContainers()
        {
            Transform surfaceContainer = null;
            Transform markerContainer = null;

            if (InstantTrackingRoot != null)
            {
                var instantCtrl = InstantTrackingRoot.GetComponent<InstantTrackingController>();
                if (instantCtrl != null) surfaceContainer = instantCtrl.ModelContainer;
            }

            if (ImageTrackingRoot != null)
            {
                var markerCtrl = ImageTrackingRoot.GetComponent<PersistentMarkerController>();
                if (markerCtrl != null) markerContainer = markerCtrl.TargetModel;
            }

            m_modelLoader.InitializeContainers(surfaceContainer, markerContainer);
        }

        private void ParseParametersFromURL()
        {
            string url = Application.absoluteURL;
            Debug.Log($"[ViMARA AR] URL del visor: {url}");

            if (string.IsNullOrEmpty(url))
            {
                m_currentMode = DefaultMode;
                return;
            }

            if (url.IndexOf("mode=marker", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                m_currentMode = TrackingMode.Marker;
                Debug.Log("[ViMARA AR] Modo activo: SEGUIMIENTO POR MARCADOR (World Lock).");
            }
            else
            {
                m_currentMode = TrackingMode.Surface;
                Debug.Log("[ViMARA AR] Modo activo: SEGUIMIENTO POR SUPERFICIE (Plano).");
            }

            string modelParam = GetQueryParam(url, "model");
            string formatParam = GetQueryParam(url, "format");

            if (!string.IsNullOrEmpty(modelParam))
            {
                m_initialModelPayload = !string.IsNullOrEmpty(formatParam)
                    ? $"{modelParam}|{formatParam}"
                    : modelParam;
            }
        }

        private string GetQueryParam(string fullUrl, string key)
        {
            int qIndex = fullUrl.IndexOf('?');
            if (qIndex < 0 || qIndex + 1 >= fullUrl.Length) return null;

            string query = fullUrl.Substring(qIndex + 1);
            int hashIndex = query.IndexOf('#');
            if (hashIndex >= 0) query = query.Substring(0, hashIndex);

            string[] pairs = query.Split('&');
            foreach (string pair in pairs)
            {
                int eqIdx = pair.IndexOf('=');
                if (eqIdx > 0)
                {
                    string k = pair.Substring(0, eqIdx);
                    if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                    {
                        string val = pair.Substring(eqIdx + 1);
                        return UnityWebRequest.UnEscapeURL(val);
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Llamado desde JavaScript mediante unityInstance.SendMessage('AR Mode Switcher', 'LoadModelFromJS', payload)
        /// </summary>
        public void LoadModelFromJS(string payload)
        {
            if (m_modelLoader != null)
            {
                ResolveModelContainers();
                m_modelLoader.LoadModelFromPayload(payload);
            }
        }

        public void SetMode(string modeName)
        {
            if (string.Equals(modeName, "marker", StringComparison.OrdinalIgnoreCase))
            {
                ApplyTrackingMode(TrackingMode.Marker);
            }
            else
            {
                ApplyTrackingMode(TrackingMode.Surface);
            }
        }

        private void ApplyTrackingMode(TrackingMode mode)
        {
            m_currentMode = mode;

            if (mode == TrackingMode.Marker)
            {
                if (InstantTrackingRoot != null)
                {
                    var instantTarget = InstantTrackingRoot.GetComponent<ZapparInstantTrackingTarget>();
                    if (instantTarget != null && instantTarget.InstantTracker != null)
                    {
                        Z.InstantWorldTrackerEnabledSet(instantTarget.InstantTracker.Value, false);
                    }
                    InstantTrackingRoot.SetActive(false);
                }

                if (ImageTrackingRoot != null)
                {
                    ImageTrackingRoot.SetActive(true);
                    var imageTarget = ImageTrackingRoot.GetComponent<ZapparImageTrackingTarget>();
                    if (imageTarget != null && imageTarget.ImageTrackerPtr != null)
                    {
                        Z.ImageTrackerEnabledSet(imageTarget.ImageTrackerPtr.Value, true);
                    }

                    if (ZapparCamera.Instance != null)
                    {
                        ZapparCamera.Instance.AnchorOrigin = null;
                        ZapparCamera.Instance.CameraAttitudeFromGyro = true;
                    }
                }
            }
            else
            {
                if (ImageTrackingRoot != null)
                {
                    var imageTarget = ImageTrackingRoot.GetComponent<ZapparImageTrackingTarget>();
                    if (imageTarget != null && imageTarget.ImageTrackerPtr != null)
                    {
                        Z.ImageTrackerEnabledSet(imageTarget.ImageTrackerPtr.Value, false);
                    }
                    ImageTrackingRoot.SetActive(false);
                }

                if (InstantTrackingRoot != null)
                {
                    InstantTrackingRoot.SetActive(true);
                    var instantTargetComp = InstantTrackingRoot.GetComponent<ZapparInstantTrackingTarget>();
                    if (instantTargetComp != null && instantTargetComp.InstantTracker != null)
                    {
                        Z.InstantWorldTrackerEnabledSet(instantTargetComp.InstantTracker.Value, true);
                    }

                    if (ZapparCamera.Instance != null)
                    {
                        ZapparCamera.Instance.AnchorOrigin = instantTargetComp;
                        ZapparCamera.Instance.CameraAttitudeFromGyro = false;
                    }
                }
            }

            Debug.Log($"[ViMARA AR] Modo aplicado: {m_currentMode}");
        }
    }
}
