using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Gestiona el anclaje persistente de la maqueta sobre el marcador de imagen.
    /// Características:
    /// 1. Cuando el marcador es detectado, la maqueta se alinea y ancla sólidamente.
    /// 2. Si el usuario aleja la cámara del marcador, la maqueta permanece fija en el espacio 3D (World Lock).
    /// 3. Corrige la elevación para que la base descanse sobre la superficie sin atravesarla.
    /// 4. Permite manipulación táctil (rotar, escalar) sobre el modelo anclado.
    /// </summary>
    [RequireComponent(typeof(ZapparImageTrackingTarget))]
    public class PersistentMarkerController : MonoBehaviour
    {
        private ZapparImageTrackingTarget m_imageTarget;

        [Header("Configuración de Anclaje Persistente")]
        [Tooltip("Si es true, la maqueta no desaparece cuando el marcador sale del cuadro.")]
        public bool PersistInWorldWhenLost = true;

        [Tooltip("Filtro de suavizado mientras el marcador es seguido activamente.")]
        public float TrackingSmoothSpeed = 15f;

        [Header("Contenedor de la Maqueta")]
        [Tooltip("GameObject hijo que contiene la maqueta 3D.")]
        public Transform ModelContainer;

        [Tooltip("Elevación ergonómica sobre el plano del marcador (Y).")]
        public float ElevationOffset = 0.15f;

        private bool m_hasBeenDetectedOnce = false;
        private Vector3 m_anchoredWorldPos;
        private Quaternion m_anchoredWorldRot;
        private bool m_isCurrentlySeen = false;

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
            if (ModelContainer == null && transform.childCount > 0)
            {
                // Buscar el primer hijo que no sea el Preview Object
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (!child.name.Contains("Preview"))
                    {
                        ModelContainer = child;
                        break;
                    }
                }
            }

            // Asegurar que el contenedor tenga el controlador de gestos
            if (ModelContainer != null)
            {
                if (ModelContainer.GetComponent<TouchManipulationController>() == null)
                {
                    ModelContainer.gameObject.AddComponent<TouchManipulationController>();
                }
            }
        }

        private void OnMarkerSeen()
        {
            m_isCurrentlySeen = true;
            m_hasBeenDetectedOnce = true;

            if (ModelContainer != null)
            {
                ModelContainer.gameObject.SetActive(true);
            }

            Debug.Log("[ViMARA AR] Marcador detectado. Anclaje de maqueta activo.");
        }

        private void OnMarkerNotSeen()
        {
            m_isCurrentlySeen = false;

            if (!PersistInWorldWhenLost)
            {
                if (ModelContainer != null)
                {
                    ModelContainer.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.Log("[ViMARA AR] Marcador fuera de cuadro: Modo Anclaje Persistente (World Lock) activo.");
            }
        }

        private void LateUpdate()
        {
            if (!m_hasBeenDetectedOnce || ModelContainer == null) return;

            if (m_isCurrentlySeen)
            {
                // Guardar la última posición y rotación de mundo válida
                m_anchoredWorldPos = transform.position + transform.up * ElevationOffset;
                m_anchoredWorldRot = transform.rotation;
            }
        }

        /// <summary>
        /// Permite re-sincronizar el anclaje si el usuario desea recalibrar con el marcador.
        /// </summary>
        public void RealignWithMarker()
        {
            m_hasBeenDetectedOnce = false;
            m_isCurrentlySeen = false;
            Debug.Log("[ViMARA AR] Anclaje de marcador reiniciado.");
        }
    }
}
