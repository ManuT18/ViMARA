using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador oficial y optimizado para Zappar Instant Tracking en ViMARA.
    /// Características:
    /// 1. Utiliza el motor SLAM nativo de Zappar (Kalman Filter / Bundle Adjustment en C++ WebAssembly).
    /// 2. Retícula visual de apuntado (Aro en plano horizontal) activa en modo preview y oculta al fijar.
    /// 3. Toggle táctil fluido (Toque: Fijar / Desanclar).
    /// 4. Calibración ergonómica hacia la mesa o suelo.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Posicionamiento Relativo (Mesa / Suelo)")]
        [Tooltip("Distancia inicial frente a la cámara (en metros).")]
        public float PreviewDistanceZ = -1.4f;

        [Tooltip("Desplazamiento vertical hacia abajo para proyectar sobre el escritorio.")]
        public float PreviewOffsetY = -0.45f;

        [Header("Retícula Visual de Colocación (Placement Reticle)")]
        public bool ShowPlacementReticle = true;
        public float ReticleRadius = 0.35f;
        public Color ReticleColor = new Color(0.22f, 0.74f, 0.97f, 0.9f); // Azul cielo brillante

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
            UpdateReticleState();
        }

        private void SetupPlacementReticle()
        {
            if (!ShowPlacementReticle) return;

            // Creamos un GameObject hijo para dibujar el aro guía en el plano Y = 0
            m_reticleObject = new GameObject("PlacementReticle_Ring");
            m_reticleObject.transform.SetParent(transform, false);
            m_reticleObject.transform.localPosition = new Vector3(0, 0.005f, 0); // Ligeramente sobre el plano para evitar z-fighting
            m_reticleObject.transform.localRotation = Quaternion.identity;

            m_reticleLine = m_reticleObject.AddComponent<LineRenderer>();
            m_reticleLine.useWorldSpace = false;
            m_reticleLine.loop = true;
            m_reticleLine.startWidth = 0.015f;
            m_reticleLine.endWidth = 0.015f;

            // Shader unlit para máxima visibilidad en WebGL
            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");

            Material mat = new Material(unlitShader);
            mat.color = ReticleColor;
            m_reticleLine.material = mat;
            m_reticleLine.startColor = ReticleColor;
            m_reticleLine.endColor = ReticleColor;

            // Generar los 48 vértices del círculo
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
            if (m_reticleObject != null && m_trackingTarget != null)
            {
                // El aro sólo se muestra cuando NO está anclado
                m_reticleObject.SetActive(!m_trackingTarget.UserHasPlaced && ShowPlacementReticle);
            }
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            HandleInput();

            // Mientras esté en modo preview (no anclado), posicionamos el anclaje frente al usuario
            if (!m_trackingTarget.UserHasPlaced)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                    m_trackingTarget.InstantTracker.Value,
                    0f, PreviewOffsetY, PreviewDistanceZ,
                    Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                );

                // Animación de rotación suave en la retícula
                if (m_reticleObject != null && m_reticleObject.activeSelf)
                {
                    m_reticleObject.transform.Rotate(Vector3.up, 25f * Time.deltaTime, Space.Self);
                }
            }
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
                if (!m_trackingTarget.UserHasPlaced)
                {
                    m_trackingTarget.PlaceTrackerAnchor();
                    UpdateReticleState();
                    Debug.Log("[ViMARA AR] Anclaje FIJADO sobre el plano.");
                }
                else
                {
                    m_trackingTarget.ResetTrackerAnchor();
                    UpdateReticleState();
                    Debug.Log("[ViMARA AR] Anclaje LIBERADO. Retícula activa.");
                }
            }
            else if (isResetKeyPressed && m_trackingTarget.UserHasPlaced)
            {
                m_trackingTarget.ResetTrackerAnchor();
                UpdateReticleState();
                Debug.Log("[ViMARA AR] Anclaje reiniciado vía teclado.");
            }
        }
    }
}
