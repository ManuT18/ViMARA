using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de Anclaje Persistente (World Lock) y Estabilización Adaptativa para Seguimiento por Marcador en ViMARA.
    /// Características clave:
    /// 1. Oculta automáticamente el "Preview Object" del marcador en tiempo de ejecución para que nunca se superponga la imagen 2D sobre el marcador físico.
    /// 2. Filtra micro-vibraciones (jitter/temblor de la cámara) mediante una zona muerta adaptativa (Deadzone + One-Euro style damping) en LateUpdate.
    /// 3. Se adapta tanto a marcadores apoyados sobre una mesa (horizontales) como colgados en pared (verticales), nivelando la maqueta respecto a la gravedad terrestre cuando el marcador está horizontal.
    /// 4. Mantiene la maqueta fija en el mundo (World Lock) cuando la cámara se aleja o deja de ver el marcador.
    /// </summary>
    [RequireComponent(typeof(ZapparImageTrackingTarget))]
    public class PersistentMarkerController : MonoBehaviour
    {
        private ZapparImageTrackingTarget m_imageTarget;

        [Header("Elevación y Nivelación")]
        [Tooltip("Elevación sutil sobre la superficie del marcador.")]
        public float ElevationOffset = 0.01f;

        [Tooltip("Si el marcador está apoyado horizontalmente (< 35° respecto a la gravedad), nivela la maqueta perfectamente con el suelo para evitar inclinaciones por distorsión de lente.")]
        public bool AutoLevelHorizontalMarkers = true;

        [Header("Filtro Anti-Temblor (Estabilización Adaptativa)")]
        [Tooltip("Velocidad base de suavizado cuando el marcador se mueve.")]
        public float TrackingLerpSpeed = 10f;

        [Tooltip("Umbral en metros por debajo del cual se filtran micro-vibraciones del sensor de cámara.")]
        public float PositionDeadzone = 0.008f;

        [Tooltip("Umbral en grados por debajo del cual se filtran micro-oscilaciones angulares.")]
        public float RotationDeadzoneAngle = 1.8f;

        [Header("Contenedor de la Maqueta 3D")]
        [Tooltip("Transform de la maqueta a anclar en el mundo.")]
        public Transform TargetModel;

        private bool m_hasBeenDetectedOnce = false;
        private bool m_isCurrentlySeen = false;
        private Vector3 m_lastValidWorldPos;
        private Quaternion m_lastValidWorldRot = Quaternion.identity;
        private Vector3 m_lastValidWorldScale = Vector3.one;
        private int m_seenStableFrames = 0;

        private void Awake()
        {
            m_imageTarget = GetComponent<ZapparImageTrackingTarget>();

            // 1. Ocultar cualquier objeto de vista previa del marcador (Preview Object) para que NO se renderice la imagen del marcador en AR
            HideMarkerPreviewObjects();

            if (m_imageTarget != null)
            {
                m_imageTarget.OnSeenEvent.AddListener(OnMarkerSeen);
                m_imageTarget.OnNotSeenEvent.AddListener(OnMarkerNotSeen);
            }
        }

        private void Start()
        {
            HideMarkerPreviewObjects();
            SetupModel();
        }

        private void HideMarkerPreviewObjects()
        {
            if (m_imageTarget != null && m_imageTarget.PreviewImageObject != null)
            {
                m_imageTarget.PreviewImageObject.SetActive(false);
                Renderer r = m_imageTarget.PreviewImageObject.GetComponent<Renderer>();
                if (r != null) r.enabled = false;
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.Contains("Preview"))
                {
                    child.gameObject.SetActive(false);
                    Renderer r = child.GetComponent<Renderer>();
                    if (r != null) r.enabled = false;
                }
            }
        }

        private void SetupModel()
        {
            if (TargetModel == null)
            {
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
                // Desvincular del padre para que ZapparImageTrackingTarget no sobrescriba su transform con ruido crudo cada frame,
                // permitiendo que nuestro estabilizador en LateUpdate controle su pose de mundo con precisión absoluta.
                if (TargetModel.parent == transform)
                {
                    TargetModel.SetParent(null, true);
                }

                // Ocultar la maqueta al inicio hasta que se detecte el marcador por primera vez
                TargetModel.gameObject.SetActive(false);

                // Añadir manipulación táctil
                if (TargetModel.GetComponent<TouchManipulationController>() == null)
                {
                    TargetModel.gameObject.AddComponent<TouchManipulationController>();
                }
            }
        }

        private void OnEnable()
        {
            HideMarkerPreviewObjects();
            if (m_imageTarget != null && m_imageTarget.ImageTracker != null)
            {
                Z.ImageTrackerEnabledSet(m_imageTarget.ImageTracker.Value, true);
            }
            if (TargetModel != null && m_hasBeenDetectedOnce)
            {
                TargetModel.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (m_imageTarget != null && m_imageTarget.ImageTracker != null)
            {
                Z.ImageTrackerEnabledSet(m_imageTarget.ImageTracker.Value, false);
            }
            if (TargetModel != null)
            {
                TargetModel.gameObject.SetActive(false);
            }
        }

        private void OnMarkerSeen()
        {
            m_isCurrentlySeen = true;
            m_seenStableFrames = 0;

            if (!m_hasBeenDetectedOnce)
            {
                m_hasBeenDetectedOnce = true;
                ComputeDesiredWorldPose(out m_lastValidWorldPos, out m_lastValidWorldRot);
                if (TargetModel != null)
                {
                    TargetModel.position = m_lastValidWorldPos;
                    TargetModel.rotation = m_lastValidWorldRot;
                    TargetModel.gameObject.SetActive(true);
                }
                Debug.Log("[ViMARA AR] Marcador detectado por primera vez. Maqueta anclada y estabilizada.");
            }
        }

        private void OnMarkerNotSeen()
        {
            m_isCurrentlySeen = false;
            m_seenStableFrames = 0;
            // IMPORTANTE: NO desactivamos TargetModel. Permanece fijo en m_lastValidWorldPos (World Lock).
            Debug.Log("[ViMARA AR] Marcador fuera de cuadro: Maqueta fijada en el espacio 3D (World Lock activo).");
        }

        private void ComputeDesiredWorldPose(out Vector3 desiredPos, out Quaternion desiredRot)
        {
            Vector3 markerUp = transform.up;
            Vector3 markerForward = transform.forward;

            // Si el marcador está apoyado sobre una mesa/suelo (inclinación < 38° respecto a Vector3.up),
            // estabilizamos el vector vertical con la gravedad para eliminar el cabeceo óptico de la cámara.
            if (AutoLevelHorizontalMarkers && Vector3.Angle(markerUp, Vector3.up) < 38f)
            {
                Vector3 flatForward = Vector3.ProjectOnPlane(markerForward, Vector3.up);
                if (flatForward.sqrMagnitude > 0.001f)
                {
                    desiredRot = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
                }
                else
                {
                    desiredRot = transform.rotation;
                }
                desiredPos = transform.position + (Vector3.up * ElevationOffset);
            }
            else
            {
                // Marcador en pared o inclinado: respetar orientación 3D completa
                desiredRot = transform.rotation;
                desiredPos = transform.position + (markerUp * ElevationOffset);
            }
        }

        private void LateUpdate()
        {
            if (TargetModel == null || !m_hasBeenDetectedOnce) return;

            if (m_isCurrentlySeen)
            {
                m_seenStableFrames++;
                ComputeDesiredWorldPose(out Vector3 targetWorldPos, out Quaternion targetWorldRot);

                float posDelta = Vector3.Distance(m_lastValidWorldPos, targetWorldPos);
                float rotDelta = Quaternion.Angle(m_lastValidWorldRot, targetWorldRot);

                // Factor adaptativo: si el movimiento está dentro de la zona muerta (temblor de mano/cámara),
                // amortiguamos fuertemente (0.04) para que el modelo luzca clavado como una roca.
                // Si el usuario mueve el marcador o se desplaza significativamente, interpolamos ágilmente.
                float posFactor = posDelta < PositionDeadzone
                    ? Mathf.Clamp01(Time.deltaTime * 1.5f)
                    : Mathf.Clamp01(Time.deltaTime * TrackingLerpSpeed * Mathf.Clamp(posDelta * 18f, 0.4f, 1.5f));

                float rotFactor = rotDelta < RotationDeadzoneAngle
                    ? Mathf.Clamp01(Time.deltaTime * 1.2f)
                    : Mathf.Clamp01(Time.deltaTime * TrackingLerpSpeed * Mathf.Clamp(rotDelta * 0.12f, 0.35f, 1.4f));

                // En los primeros 4 frames tras detectar por primera vez, converger rápido
                if (m_seenStableFrames <= 4)
                {
                    posFactor = 0.65f;
                    rotFactor = 0.65f;
                }

                m_lastValidWorldPos = Vector3.Lerp(m_lastValidWorldPos, targetWorldPos, posFactor);
                m_lastValidWorldRot = Quaternion.Slerp(m_lastValidWorldRot, targetWorldRot, rotFactor);
            }

            // Aplicar siempre la pose estabilizada en LateUpdate (después de Zappar y antes del render)
            TargetModel.position = m_lastValidWorldPos;
            var touchCtrl = TargetModel.GetComponent<TouchManipulationController>();
            if (touchCtrl != null)
            {
                touchCtrl.SetExternalBaseRotation(m_lastValidWorldRot);
            }
            else
            {
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
            m_seenStableFrames = 0;
            if (TargetModel != null)
            {
                TargetModel.gameObject.SetActive(false);
            }
            Debug.Log("[ViMARA AR] Anclaje de marcador reiniciado.");
        }
    }
}
