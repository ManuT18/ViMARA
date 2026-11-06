using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de Anclaje Persistente (World Lock) y Estabilización Adaptativa para Seguimiento por Marcador.
    /// Características:
    /// 1. Oculta automáticamente el "Preview Object" del marcador para no superponer la textura 2D en cámara.
    /// 2. Filtra micro-vibraciones (jitter) mediante zona muerta adaptativa (Deadzone) en LateUpdate.
    /// 3. Nivela automáticamente la maqueta respecto a la gravedad cuando el marcador reposa horizontalmente sobre una mesa.
    /// 4. Mantiene la maqueta anclada en el espacio (World Lock) si el marcador sale del encuadre.
    /// </summary>
    [RequireComponent(typeof(ZapparImageTrackingTarget))]
    public class PersistentMarkerController : MonoBehaviour
    {
        private const float HorizontalMarkerMaxTiltDegrees = 38f;
        private const int FastConvergenceFrameCount = 4;
        private const float FastConvergenceLerpFactor = 0.65f;

        private ZapparImageTrackingTarget m_imageTarget;
        private TouchManipulationController m_touchController;

        [Header("Elevación y Nivelación")]
        [Tooltip("Elevación sutil sobre la superficie del marcador.")]
        public float ElevationOffset = 0.01f;

        [Tooltip("Si el marcador está apoyado horizontalmente (< 38° respecto a la gravedad), nivela la maqueta con el suelo para evitar inclinaciones ópticas.")]
        public bool AutoLevelHorizontalMarkers = true;

        [Header("Filtro Anti-Temblor (Estabilización Adaptativa)")]
        [Tooltip("Velocidad base de suavizado cuando el marcador se desplaza.")]
        public float TrackingLerpSpeed = 10f;

        [Tooltip("Umbral en metros por debajo del cual se amortiguan micro-vibraciones de cámara.")]
        public float PositionDeadzone = 0.008f;

        [Tooltip("Umbral en grados por debajo del cual se amortiguan micro-oscilaciones angulares.")]
        public float RotationDeadzoneAngle = 1.8f;

        [Header("Contenedor de la Maqueta 3D")]
        [Tooltip("Transform de la maqueta a anclar en el mundo.")]
        public Transform TargetModel;

        private bool m_hasBeenDetectedOnce;
        private bool m_isCurrentlySeen;
        private Vector3 m_lastValidWorldPos;
        private Quaternion m_lastValidWorldRot = Quaternion.identity;
        private int m_seenStableFrames;

        private void Awake()
        {
            m_imageTarget = GetComponent<ZapparImageTrackingTarget>();
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

        private void OnEnable()
        {
            HideMarkerPreviewObjects();
            if (m_imageTarget != null)
            {
                if (m_imageTarget.ImageTrackerPtr != null)
                {
                    Z.ImageTrackerEnabledSet(m_imageTarget.ImageTrackerPtr.Value, true);
                }
                else if (ZapparCamera.Instance != null && ZapparCamera.Instance.PipelineIsInitialized)
                {
                    m_imageTarget.OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
                }
            }

            if (TargetModel != null && m_hasBeenDetectedOnce)
            {
                TargetModel.gameObject.SetActive(true);
            }
        }

        private void OnDisable()
        {
            if (m_imageTarget != null && m_imageTarget.ImageTrackerPtr != null)
            {
                Z.ImageTrackerEnabledSet(m_imageTarget.ImageTrackerPtr.Value, false);
            }

            if (TargetModel != null)
            {
                TargetModel.gameObject.SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (m_imageTarget != null)
            {
                m_imageTarget.OnSeenEvent.RemoveListener(OnMarkerSeen);
                m_imageTarget.OnNotSeenEvent.RemoveListener(OnMarkerNotSeen);
            }
        }

        private void HideMarkerPreviewObjects()
        {
            if (m_imageTarget != null && m_imageTarget.PreviewImageObject != null)
            {
                DisablePreviewObject(m_imageTarget.PreviewImageObject);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.name.Contains("Preview"))
                {
                    DisablePreviewObject(child.gameObject);
                }
            }
        }

        private static void DisablePreviewObject(GameObject previewObj)
        {
            previewObj.SetActive(false);
            Renderer r = previewObj.GetComponent<Renderer>();
            if (r != null) r.enabled = false;
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

            if (TargetModel == null) return;

            // Desvincular del padre para evitar que ZapparImageTrackingTarget sobrescriba su transform con ruido crudo cada frame
            if (TargetModel.parent == transform)
            {
                TargetModel.SetParent(null, true);
            }

            TargetModel.gameObject.SetActive(false);

            m_touchController = TargetModel.GetComponent<TouchManipulationController>();
            if (m_touchController == null)
            {
                m_touchController = TargetModel.gameObject.AddComponent<TouchManipulationController>();
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
            }
        }

        private void OnMarkerNotSeen()
        {
            m_isCurrentlySeen = false;
            m_seenStableFrames = 0;
            // World Lock: el modelo permanece activo en m_lastValidWorldPos cuando el marcador sale de vista
        }

        private void ComputeDesiredWorldPose(out Vector3 desiredPos, out Quaternion desiredRot)
        {
            Vector3 markerUp = transform.up;
            Vector3 markerForward = transform.forward;

            if (AutoLevelHorizontalMarkers && Vector3.Angle(markerUp, Vector3.up) < HorizontalMarkerMaxTiltDegrees)
            {
                Vector3 flatForward = Vector3.ProjectOnPlane(markerForward, Vector3.up);
                desiredRot = flatForward.sqrMagnitude > 0.001f
                    ? Quaternion.LookRotation(flatForward.normalized, Vector3.up)
                    : transform.rotation;
                desiredPos = transform.position + (Vector3.up * ElevationOffset);
            }
            else
            {
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

                float posFactor = posDelta < PositionDeadzone
                    ? Mathf.Clamp01(Time.deltaTime * 1.5f)
                    : Mathf.Clamp01(Time.deltaTime * TrackingLerpSpeed * Mathf.Clamp(posDelta * 18f, 0.4f, 1.5f));

                float rotFactor = rotDelta < RotationDeadzoneAngle
                    ? Mathf.Clamp01(Time.deltaTime * 1.2f)
                    : Mathf.Clamp01(Time.deltaTime * TrackingLerpSpeed * Mathf.Clamp(rotDelta * 0.12f, 0.35f, 1.4f));

                if (m_seenStableFrames <= FastConvergenceFrameCount)
                {
                    posFactor = FastConvergenceLerpFactor;
                    rotFactor = FastConvergenceLerpFactor;
                }

                m_lastValidWorldPos = Vector3.Lerp(m_lastValidWorldPos, targetWorldPos, posFactor);
                m_lastValidWorldRot = Quaternion.Slerp(m_lastValidWorldRot, targetWorldRot, rotFactor);
            }

            TargetModel.position = m_lastValidWorldPos;

            if (m_touchController == null)
            {
                m_touchController = TargetModel.GetComponent<TouchManipulationController>();
            }

            if (m_touchController != null)
            {
                m_touchController.SetExternalBaseRotation(m_lastValidWorldRot);
            }
            else
            {
                TargetModel.rotation = m_lastValidWorldRot;
            }
        }

        public void ResetMarkerAnchor()
        {
            m_hasBeenDetectedOnce = false;
            m_isCurrentlySeen = false;
            m_seenStableFrames = 0;
            if (TargetModel != null)
            {
                TargetModel.gameObject.SetActive(false);
            }
        }
    }
}
