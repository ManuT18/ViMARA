using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador avanzado para Zappar Instant Tracking en ViMARA.
    /// Incorpora:
    /// 1. Retícula visual de colocación en el suelo (Placement Reticle Ring).
    /// 2. Calibración de proyección para superficies de escritorio y suelo (Offset ergonómico).
    /// 3. Suavizado inteligente (Camera / Target Damping).
    /// 4. Filtro contra saltos bruscos (Outlier Clamping).
    /// 5. Alineación con la gravedad terrestre (MINUS_Z_HEADING).
    /// 6. Toggle táctil para anclar y desanclar libremente.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Posicionamiento Relativo (Offset de Mesa / Suelo)")]
        [Tooltip("Distancia inicial frente a la cámara. -1.4m es ideal para ver maquetas sobre escritorios o mesas.")]
        public float PreviewDistanceZ = -1.4f;

        [Tooltip("Desplazamiento vertical hacia abajo. -0.45m inclina el punto de anclaje de forma natural hacia la superficie.")]
        public float PreviewOffsetY = -0.45f;

        [Header("Retícula Visual de Colocación (Placement Reticle)")]
        public bool ShowPlacementReticle = true;
        public float ReticleRadius = 0.35f;
        public Color ReticleColor = new Color(0.22f, 0.74f, 0.97f, 0.85f); // Sky blue neon

        [Header("Estabilidad y Filtrado (Damping)")]
        public bool EnableSmoothing = true;

        [Tooltip("Velocidad de interpolación de posición.")]
        [Range(1f, 35f)] public float PositionLerpSpeed = 16f;

        [Tooltip("Velocidad de interpolación de rotación.")]
        [Range(1f, 35f)] public float RotationLerpSpeed = 16f;

        [Tooltip("Distancia máxima permitida de salto por frame para filtrar pérdidas momentáneas de tracking.")]
        public float MaxStepDistance = 1.5f;

        private Vector3 m_smoothedPosition;
        private Quaternion m_smoothedRotation;
        private bool m_justPlacedOrReset = true;
        private bool m_isAnchored = false;

        // Referencia a la retícula procedural
        private GameObject m_reticleObject;
        private LineRenderer m_reticleLine;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            SetupPlacementReticle();
        }

        private void Start()
        {
            Transform targetTransform = GetActiveMovingTransform();
            if (targetTransform != null)
            {
                m_smoothedPosition = targetTransform.localPosition;
                m_smoothedRotation = targetTransform.localRotation;
            }

            UpdateReticleState();
        }

        private void SetupPlacementReticle()
        {
            if (!ShowPlacementReticle) return;

            // Creamos un GameObject hijo para dibujar el aro guía en el plano Y = 0
            m_reticleObject = new GameObject("PlacementReticle_Ring");
            m_reticleObject.transform.SetParent(transform, false);
            m_reticleObject.transform.localPosition = new Vector3(0, 0.005f, 0); // Ligeramente arriba de 0 para evitar z-fighting
            m_reticleObject.transform.localRotation = Quaternion.identity;

            m_reticleLine = m_reticleObject.AddComponent<LineRenderer>();
            m_reticleLine.useWorldSpace = false;
            m_reticleLine.loop = true;
            m_reticleLine.startWidth = 0.015f;
            m_reticleLine.endWidth = 0.015f;

            // Shader estándar de partículas o unlit para visibilidad clara
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");

            Material mat = new Material(unlitShader);
            mat.color = ReticleColor;
            m_reticleLine.material = mat;
            m_reticleLine.startColor = ReticleColor;
            m_reticleLine.endColor = ReticleColor;

            // Generar vértices del círculo
            int segments = 48;
            m_reticleLine.positionCount = segments;
            float angleStep = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * angleStep);
                float x = Mathf.Sin(rad) * ReticleRadius;
                float z = Mathf.Cos(rad) * ReticleRadius;
                m_reticleLine.SetPosition(i, new Vector3(x, 0, z));
            }
        }

        private void UpdateReticleState()
        {
            if (m_reticleObject != null)
            {
                // El aro sólo se muestra cuando NO está anclado (modo previsualización / apuntado)
                m_reticleObject.SetActive(!m_isAnchored && ShowPlacementReticle);
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

            // Si NO está anclado, proyectamos el modelo frente a la cámara alineado a la gravedad sobre el plano
            if (!m_isAnchored)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                    m_trackingTarget.InstantTracker.Value,
                    0f, PreviewOffsetY, PreviewDistanceZ,
                    Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                );

                // Pulso sutil de rotación o respiración en la retícula mientras apunta
                if (m_reticleObject != null && m_reticleObject.activeSelf)
                {
                    m_reticleObject.transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.Self);
                }
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
                    UpdateReticleState();
                    Debug.Log("[ViMARA AR] Anclaje FIJADO sobre la superficie física.");
                }
                else
                {
                    m_isAnchored = false;
                    m_justPlacedOrReset = true;
                    UpdateReticleState();
                    Debug.Log("[ViMARA AR] Anclaje LIBERADO. Retícula visual de apuntado activa.");
                }
            }
            else if (isResetKeyPressed && m_isAnchored)
            {
                m_isAnchored = false;
                m_justPlacedOrReset = true;
                UpdateReticleState();
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
