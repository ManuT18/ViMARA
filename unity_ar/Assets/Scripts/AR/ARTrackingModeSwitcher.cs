using System;
using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestiona el modo activo de Realidad Aumentada (Superficie vs Marcador).
    /// Detecta el modo desde los parámetros URL de React (?mode=marker o ?mode=surface),
    /// sincroniza el AnchorOrigin de ZapparCamera y optimiza el consumo térmico/batería.
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
            // Optimización térmica y de batería en WebGL móvil (evita 120fps descontrolados)
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            DetectModeFromURL();
            ApplyTrackingMode(m_currentMode);
        }

        private void Start()
        {
            // Reaplicar después de que ZapparCamera se haya inicializado
            ApplyTrackingMode(m_currentMode);
        }

        private void DetectModeFromURL()
        {
            string url = Application.absoluteURL;
            Debug.Log($"[ViMARA AR] URL actual del visor: {url}");

            if (!string.IsNullOrEmpty(url) && url.IndexOf("mode=marker", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                m_currentMode = TrackingMode.Marker;
                Debug.Log("[ViMARA AR] Modo detectado desde URL: SEGUIMIENTO POR MARCADOR (Image Tracking).");
            }
            else
            {
                m_currentMode = TrackingMode.Surface;
                Debug.Log("[ViMARA AR] Modo detectado desde URL: SEGUIMIENTO POR SUPERFICIE (Instant Tracking).");
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
                        var imageTargetComp = ImageTrackingRoot.GetComponent<ZapparTrackingTarget>();
                        ZapparCamera.Instance.AnchorOrigin = imageTargetComp;
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
                        var instantTargetComp = InstantTrackingRoot.GetComponent<ZapparTrackingTarget>();
                        ZapparCamera.Instance.AnchorOrigin = instantTargetComp;
                    }
                }
            }

            Debug.Log($"[ViMARA AR] Modo de tracking aplicado: {m_currentMode} | AnchorOrigin: {(ZapparCamera.Instance != null ? ZapparCamera.Instance.AnchorOrigin?.name : "N/A")}");
        }
    }
}
