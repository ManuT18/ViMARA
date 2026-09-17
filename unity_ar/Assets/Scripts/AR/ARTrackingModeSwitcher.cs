using System;
using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestiona el modo activo de Realidad Aumentada (Superficie vs Marcador).
    /// Configura dinámicamente ZapparCamera, optimiza el consumo térmico y coordina el anclaje.
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

        private void Awake()
        {
            // IMPORTANT: In WebGL, forcing targetFrameRate can cause spin-loops that starve the browser's 
            // video decoding thread, causing the AR camera feed to freeze after a few minutes. 
            // We set it to -1 to let the browser natively sync via requestAnimationFrame.
            Application.targetFrameRate = -1;

            DetectModeFromURL();
        }

        private void Start()
        {
            ApplyTrackingMode(m_currentMode);
        }

        private void DetectModeFromURL()
        {
            string url = Application.absoluteURL;
            Debug.Log($"[ViMARA AR] URL del visor: {url}");

            if (!string.IsNullOrEmpty(url) && url.IndexOf("mode=marker", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                m_currentMode = TrackingMode.Marker;
                Debug.Log("[ViMARA AR] Modo activo: SEGUIMIENTO POR MARCADOR (World Lock).");
            }
            else
            {
                m_currentMode = TrackingMode.Surface;
                Debug.Log("[ViMARA AR] Modo activo: SEGUIMIENTO POR SUPERFICIE (Plano).");
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
                if (InstantTrackingRoot != null) InstantTrackingRoot.SetActive(false);
                if (ImageTrackingRoot != null)
                {
                    ImageTrackingRoot.SetActive(true);
                    if (ZapparCamera.Instance != null)
                    {
                        // En modo marcador con World-Lock, la cámara utiliza el giroscopio para mirar alrededor
                        ZapparCamera.Instance.AnchorOrigin = null;
                        ZapparCamera.Instance.CameraAttitudeFromGyro = true;
                    }
                }
            }
            else
            {
                if (ImageTrackingRoot != null) ImageTrackingRoot.SetActive(false);
                if (InstantTrackingRoot != null)
                {
                    InstantTrackingRoot.SetActive(true);
                    if (ZapparCamera.Instance != null)
                    {
                        var instantTargetComp = InstantTrackingRoot.GetComponent<ZapparInstantTrackingTarget>();
                        ZapparCamera.Instance.AnchorOrigin = instantTargetComp;
                        ZapparCamera.Instance.CameraAttitudeFromGyro = false;
                    }
                }
            }

            Debug.Log($"[ViMARA AR] Modo aplicado: {m_currentMode}");
        }
    }
}
