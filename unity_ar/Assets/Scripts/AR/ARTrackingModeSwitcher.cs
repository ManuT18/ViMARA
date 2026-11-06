using System;
using UnityEngine;
using UnityEngine.Networking;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestiona el modo activo de Realidad Aumentada (Superficie vs Marcador)
    /// y coordina la carga del modelo 3D seleccionado desde la URL inicial o vía mensajes desde React.
    /// </summary>
    public class ARTrackingModeSwitcher : MonoBehaviour
    {
        private const string DefaultPresetPayload = "preset:house|preset";

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

            // Desactivar el tracker inactivo en Awake() antes de que Unity ejecute Start()
            // en ZapparInstantTrackingTarget o ZapparImageTrackingTarget, evitando inicializar ambos en WASM.
            bool isMarker = m_currentMode == TrackingMode.Marker;
            if (InstantTrackingRoot != null) InstantTrackingRoot.SetActive(!isMarker);
            if (ImageTrackingRoot != null) ImageTrackingRoot.SetActive(isMarker);
        }

        private void Start()
        {
            ResolveModelContainers();
            ApplyTrackingMode(m_currentMode);

            string payloadToLoad = !string.IsNullOrEmpty(m_initialModelPayload)
                ? m_initialModelPayload
                : DefaultPresetPayload;

            m_modelLoader.LoadModelFromPayload(payloadToLoad);
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
            if (string.IsNullOrEmpty(url))
            {
                m_currentMode = DefaultMode;
                return;
            }

            string modeParam = GetQueryParam(url, "mode");
            m_currentMode = IsMarkerModeName(modeParam) ? TrackingMode.Marker : TrackingMode.Surface;

            string modelParam = GetQueryParam(url, "model");
            string formatParam = GetQueryParam(url, "format");

            if (!string.IsNullOrEmpty(modelParam))
            {
                m_initialModelPayload = !string.IsNullOrEmpty(formatParam)
                    ? $"{modelParam}|{formatParam}"
                    : modelParam;
            }
        }

        private static bool IsMarkerModeName(string modeName)
        {
            return string.Equals(modeName, "marker", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetQueryParam(string fullUrl, string key)
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
                if (eqIdx <= 0) continue;

                string k = pair.Substring(0, eqIdx);
                if (string.Equals(k, key, StringComparison.OrdinalIgnoreCase))
                {
                    string val = pair.Substring(eqIdx + 1);
                    return UnityWebRequest.UnEscapeURL(val);
                }
            }
            return null;
        }

        /// <summary>
        /// Invocado desde JavaScript mediante unityInstance.SendMessage("AR Mode Switcher", "LoadModelFromJS", payload)
        /// </summary>
        public void LoadModelFromJS(string payload)
        {
            if (m_modelLoader == null) return;
            ResolveModelContainers();
            m_modelLoader.LoadModelFromPayload(payload);
        }

        /// <summary>
        /// Invocado desde JavaScript mediante unityInstance.SendMessage("AR Mode Switcher", "SetMode", modeName)
        /// </summary>
        public void SetMode(string modeName)
        {
            ApplyTrackingMode(IsMarkerModeName(modeName) ? TrackingMode.Marker : TrackingMode.Surface);
        }

        private void ApplyTrackingMode(TrackingMode mode)
        {
            m_currentMode = mode;

            if (mode == TrackingMode.Marker)
            {
                SetInstantTrackerActive(false);
                SetImageTrackerActive(true);
            }
            else
            {
                SetImageTrackerActive(false);
                SetInstantTrackerActive(true);
            }

            Debug.Log($"[ViMARA AR] Modo activo: {m_currentMode}");
        }

        private void SetInstantTrackerActive(bool active)
        {
            if (InstantTrackingRoot == null) return;

            var instantTarget = InstantTrackingRoot.GetComponent<ZapparInstantTrackingTarget>();
            if (!active)
            {
                if (instantTarget != null && instantTarget.InstantTracker != null)
                {
                    Z.InstantWorldTrackerEnabledSet(instantTarget.InstantTracker.Value, false);
                }
                InstantTrackingRoot.SetActive(false);
                return;
            }

            InstantTrackingRoot.SetActive(true);
            if (instantTarget != null && instantTarget.InstantTracker != null)
            {
                Z.InstantWorldTrackerEnabledSet(instantTarget.InstantTracker.Value, true);
            }

            if (ZapparCamera.Instance != null)
            {
                ZapparCamera.Instance.AnchorOrigin = instantTarget;
                ZapparCamera.Instance.CameraAttitudeFromGyro = false;
            }
        }

        private void SetImageTrackerActive(bool active)
        {
            if (ImageTrackingRoot == null) return;

            var imageTarget = ImageTrackingRoot.GetComponent<ZapparImageTrackingTarget>();
            if (!active)
            {
                if (imageTarget != null && imageTarget.ImageTrackerPtr != null)
                {
                    Z.ImageTrackerEnabledSet(imageTarget.ImageTrackerPtr.Value, false);
                }
                ImageTrackingRoot.SetActive(false);
                return;
            }

            ImageTrackingRoot.SetActive(true);
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
}
