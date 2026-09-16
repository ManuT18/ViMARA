using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador para Zappar Instant Tracking.
    /// Incorpora suavizado (Damping/Lerp) y corrección de orientación respecto a la gravedad.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;
        
        [Header("Posicionamiento Relativo (Offset)")]
        [Tooltip("Distancia a la que aparece la maqueta frente a la cámara antes de anclarla.")]
        public float PreviewDistanceZ = -3f;
        public float PreviewOffsetY = 0f;

        [Header("Estabilidad y Filtrado (Damping)")]
        public bool EnableSmoothing = true;
        [Range(1f, 30f)] public float PositionLerpSpeed = 12f;
        [Range(1f, 30f)] public float RotationLerpSpeed = 12f;

        private Vector3 m_smoothedPosition;
        private Quaternion m_smoothedRotation;
        private bool m_justPlacedOrReset = true;
        
        // Nuestro estado de anclaje personalizado
        private bool m_isAnchored = false;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
        }

        private void Start()
        {
            if (m_trackingTarget != null)
            {
                m_smoothedPosition = transform.localPosition;
                m_smoothedRotation = transform.localRotation;
            }
        }
        
        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            // Para evitar que Zappar asigne la orientación torcida (MINUS_Z_AWAY_FROM_USER),
            // siempre le decimos que "ya está ubicado" internamente, así nosotros tomamos
            // el control total del modo "preview" (flotando frente a cámara).
            if (!m_trackingTarget.UserHasPlaced)
            {
                m_trackingTarget.PlaceTrackerAnchor();
            }

            HandleInput();

            // Si NO estamos anclados, forzamos la pose relativa a la cámara pero ALINEADA A LA GRAVEDAD
            if (!m_isAnchored)
            {
                // MINUS_Z_HEADING alinea el eje Y con la gravedad, y el eje -Z hacia donde mira la cámara.
                // Esto garantiza que el cubo/maqueta no se tuerza al mover el celular en diagonal.
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                    m_trackingTarget.InstantTracker.Value, 
                    0f, PreviewOffsetY, PreviewDistanceZ, 
                    Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                );
            }
        }

        private void LateUpdate()
        {
            if (m_trackingTarget == null || !EnableSmoothing) return;
            ApplySmoothing();
        }

        private void HandleInput()
        {
            bool isClickOrTouchDown = false;

            if (AllowTouchToggle)
            {
                if (Input.touchCount > 0)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        isClickOrTouchDown = true;
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    isClickOrTouchDown = true;
                }
            }

            bool isResetKeyPressed = Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R);

            if (isClickOrTouchDown)
            {
                if (!m_isAnchored)
                {
                    m_isAnchored = true;
                    m_justPlacedOrReset = true;
                    Debug.Log("[ViMARA AR] Anclaje FIJADO en el espacio.");
                }
                else
                {
                    m_isAnchored = false;
                    m_justPlacedOrReset = true;
                    Debug.Log("[ViMARA AR] Anclaje LIBERADO. El objeto vuelve a acompañar a la cámara.");
                }
            }
            else if (isResetKeyPressed && m_isAnchored)
            {
                m_isAnchored = false;
                m_justPlacedOrReset = true;
                Debug.Log("[ViMARA AR] Anclaje reiniciado vía teclado.");
            }
        }

        private void ApplySmoothing()
        {
            Vector3 rawPosition = transform.localPosition;
            Quaternion rawRotation = transform.localRotation;

            if (m_justPlacedOrReset)
            {
                m_smoothedPosition = rawPosition;
                m_smoothedRotation = rawRotation;
                m_justPlacedOrReset = false;
            }
            else
            {
                m_smoothedPosition = Vector3.Lerp(m_smoothedPosition, rawPosition, Time.deltaTime * PositionLerpSpeed);
                m_smoothedRotation = Quaternion.Slerp(m_smoothedRotation, rawRotation, Time.deltaTime * RotationLerpSpeed);
            }

            transform.localPosition = m_smoothedPosition;
            transform.localRotation = m_smoothedRotation;
        }
    }
}
