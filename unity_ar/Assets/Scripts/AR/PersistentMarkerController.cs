using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de Anclaje Persistente (World Lock) para Seguimiento por Marcador en ViMARA.
    /// Funcionamiento:
    /// 1. Cuando la cámara detecta el marcador, sincroniza la maqueta 3D con la posición del marcador en el mundo.
    /// 2. Si el usuario aleja la cámara o deja de enfocar el marcador, la maqueta NO desaparece: se queda fija en el espacio 3D real.
    /// 3. Si el usuario vuelve a enfocar el marcador, se re-calibra y actualiza la posición automáticamente y de forma suave.
    /// 4. Habilita manipulación táctil (rotación con 1 dedo, zoom con 2 dedos).
    /// </summary>
    [RequireComponent(typeof(ZapparImageTrackingTarget))]
    public class PersistentMarkerController : MonoBehaviour
    {
        private ZapparImageTrackingTarget m_imageTarget;

        [Header("Elevación y Suavizado")]
        [Tooltip("Elevación ergonómica sobre el marcador para que la base descanse a nivel Y = 0.")]
        public float ElevationOffset = 0.15f;

        [Tooltip("Velocidad de interpolación cuando el marcador es visible.")]
        public float TrackingLerpSpeed = 12f;

        [Header("Contenedor de la Maqueta 3D")]
        [Tooltip("Transform de la maqueta a anclar en el mundo.")]
        public Transform TargetModel;

        private bool m_hasBeenDetectedOnce = false;
        private bool m_isCurrentlySeen = false;
        private Vector3 m_lastValidWorldPos;
        private Quaternion m_lastValidWorldRot;

        private void Awake()
        {
            m_imageTarget = GetComponent<ZapparImageTrackingTarget>();

            if (m_imageTarget != null)
            {
                m_imageTarget.OnSeenEvent.AddListener(OnMarkerSeen);
                m_imageTarget.OnNotSeenEvent.AddListener(OnMarkerNotSeen);
            }
        }

        private void Start()
        {
            SetupModel();
        }

        private void SetupModel()
        {
            if (TargetModel == null)
            {
                // Buscar hijo que no sea el Preview Object
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (!child.name.Contains("Preview"))
                    {
                        TargetModel = child;
                        break;
                    }
                }
            }

            if (TargetModel != null)
            {
                // Ocultar la maqueta al inicio hasta que se detecte el marcador por primera vez
                TargetModel.gameObject.SetActive(false);

                // Añadir manipulación táctil
                if (TargetModel.GetComponent<TouchManipulationController>() == null)
                {
                    TargetModel.gameObject.AddComponent<TouchManipulationController>();
                }
            }
        }

        private void OnMarkerSeen()
        {
            m_isCurrentlySeen = true;

            if (!m_hasBeenDetectedOnce)
            {
                m_hasBeenDetectedOnce = true;
                if (TargetModel != null)
                {
                    TargetModel.gameObject.SetActive(true);
                }
                Debug.Log("[ViMARA AR] Marcador detectado por primera vez. Maqueta activada.");
            }
            else
            {
                Debug.Log("[ViMARA AR] Marcador re-detectado. Re-alineando anclaje.");
            }
        }

        private void OnMarkerNotSeen()
        {
            m_isCurrentlySeen = false;
            // IMPORTANTE: NO desactivamos TargetModel. Permanece fijo en m_lastValidWorldPos (World Lock).
            Debug.Log("[ViMARA AR] Marcador fuera de cuadro: Maqueta fijada en el espacio 3D (World Lock activo).");
        }

        private void Update()
        {
            if (TargetModel == null || !m_hasBeenDetectedOnce) return;

            if (m_isCurrentlySeen)
            {
                // Calcular posición de mundo deseada sobre el plano del marcador
                Vector3 targetWorldPos = transform.position + (transform.up * ElevationOffset);
                Quaternion targetWorldRot = transform.rotation;

                // Suavizar seguimiento
                m_lastValidWorldPos = Vector3.Lerp(m_lastValidWorldPos, targetWorldPos, Time.deltaTime * TrackingLerpSpeed);
                m_lastValidWorldRot = Quaternion.Slerp(m_lastValidWorldRot, targetWorldRot, Time.deltaTime * TrackingLerpSpeed);

                TargetModel.position = m_lastValidWorldPos;
                TargetModel.rotation = m_lastValidWorldRot;
            }
            else
            {
                // Mantener sólidamente la última posición de mundo calculada
                TargetModel.position = m_lastValidWorldPos;
                TargetModel.rotation = m_lastValidWorldRot;
            }
        }

        /// <summary>
        /// Reinicia el estado de detección para permitir un nuevo escaneo desde cero.
        /// </summary>
        public void ResetMarkerAnchor()
        {
            m_hasBeenDetectedOnce = false;
            m_isCurrentlySeen = false;
            if (TargetModel != null)
            {
                TargetModel.gameObject.SetActive(false);
            }
            Debug.Log("[ViMARA AR] Anclaje de marcador reiniciado.");
        }
    }
}
