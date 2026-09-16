using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador avanzado para Zappar Instant Tracking en ViMARA.
    /// Incorpora:
    /// 1. Suavizado inteligente tanto en modo Camera-at-Origin como Target-at-Origin (Damping / Slerp).
    /// 2. Filtro contra saltos bruscos (Outlier Clamping) ante movimientos rápidos de cámara.
    /// 3. Alineación con la gravedad terrestre (MINUS_Z_HEADING) para mantener la maqueta horizontal.
    /// 4. Toggle táctil para anclar y desanclar libremente.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Posicionamiento Relativo (Offset)")]
        [Tooltip("Distancia inicial frente a la cámara antes de fijar el anclaje.")]
        public float PreviewDistanceZ = -3f;
        public float PreviewOffsetY = 0f;

        [Header("Estabilidad y Filtrado (Damping)")]
        public bool EnableSmoothing = true;

        [Tooltip("Velocidad de interpolación de posición. Valores de 12-18 brindan estabilidad sin sensación de retraso.")]
        [Range(1f, 35f)] public float PositionLerpSpeed = 16f;

        [Tooltip("Velocidad de interpolación de rotación.")]
        [Range(1f, 35f)] public float RotationLerpSpeed = 16f;

        [Tooltip("Distancia máxima permitida de salto por frame para filtrar pérdidas momentáneas de tracking.")]
        public float MaxStepDistance = 1.5f;

        private Vector3 m_smoothedPosition;
        private Quaternion m_smoothedRotation;
        private bool m_justPlacedOrReset = true;
        private bool m_isAnchored = false;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
        }

        private void Start()
        {
            Transform targetTransform = GetActiveMovingTransform();
            if (targetTransform != null)
            {
                m_smoothedPosition = targetTransform.localPosition;
                m_smoothedRotation = targetTransform.localRotation;
            }
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            // Mantenemos al componente de Zappar en estado 'Placed' para gobernar nosotros la orientación
            if (!m_trackingTarget.UserHasPlaced)
            {
                m_trackingTarget.PlaceTrackerAnchor();
            }

            HandleInput();

            // Si NO está anclado, proyectamos el modelo frente a la cámara alineado a la gravedad
            if (!m_isAnchored)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                    m_trackingTarget.InstantTracker.Value,
                    0f, PreviewOffsetY, PreviewDistanceZ,
                    Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                );
            }
        }

        private void LateUpdate()
        {
            if (!EnableSmoothing) return;
            ApplyActiveSmoothing();
        }

        private Transform GetActiveMovingTransform()
        {
            // Si la cámara tiene asignado el Target como Origin, la cámara es el objeto que se mueve en el mundo
            if (ZapparCamera.Instance != null && ZapparCamera.Instance.TrackerAtOrigin != null)
            {
                return ZapparCamera.Instance.transform;
            }
            // De lo contrario, el Target es el objeto que se desplaza frente a la cámara
            return transform;
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
                    Debug.Log("[ViMARA AR] Anclaje FIJADO sobre la superficie.");
                }
                else
                {
                    m_isAnchored = false;
                    m_justPlacedOrReset = true;
                    Debug.Log("[ViMARA AR] Anclaje LIBERADO. Modo previsualización activo.");
                }
            }
            else if (isResetKeyPressed && m_isAnchored)
            {
                m_isAnchored = false;
                m_justPlacedOrReset = true;
                Debug.Log("[ViMARA AR] Anclaje reiniciado vía teclado.");
            }
        }

        private void ApplyActiveSmoothing()
        {
            Transform targetTransform = GetActiveMovingTransform();
            if (targetTransform == null) return;

            Vector3 rawPosition = targetTransform.localPosition;
            Quaternion rawRotation = targetTransform.localRotation;

            if (m_justPlacedOrReset)
            {
                m_smoothedPosition = rawPosition;
                m_smoothedRotation = rawRotation;
                m_justPlacedOrReset = false;
            }
            else
            {
                // Filtro contra saltos atípicos (outliers) por motion blur extremo
                float deltaDist = Vector3.Distance(m_smoothedPosition, rawPosition);
                if (deltaDist > MaxStepDistance)
                {
                    rawPosition = m_smoothedPosition + (rawPosition - m_smoothedPosition).normalized * MaxStepDistance;
                }

                // Interpolación exponencial amortiguada
                m_smoothedPosition = Vector3.Lerp(m_smoothedPosition, rawPosition, Time.deltaTime * PositionLerpSpeed);
                m_smoothedRotation = Quaternion.Slerp(m_smoothedRotation, rawRotation, Time.deltaTime * RotationLerpSpeed);
            }

            targetTransform.localPosition = m_smoothedPosition;
            targetTransform.localRotation = m_smoothedRotation;
        }
    }
}
