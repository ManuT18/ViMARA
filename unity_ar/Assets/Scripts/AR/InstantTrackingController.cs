using UnityEngine;
using UnityEngine.EventSystems;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de seguimiento por superficie (Instant World Tracking SLAM) para ViMARA.
    /// Gestiona:
    /// 1. Retícula circular animada y malla de puntos durante la fase de escaneo.
    /// 2. Fijación de la maqueta al tocar la superficie y destrabe mediante toque corto (Tap vs Drag/Pinch).
    /// 3. Activación de manipulación táctil (rotación 360° y pellizco de escala) cuando la maqueta está colocada.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private const float TapMaxDurationSeconds = 0.35f;
        private const float TouchTapMaxDistancePx = 18f;
        private const float MouseTapMaxDistancePx = 10f;
        private const float PlacementCooldownSeconds = 0.25f;

        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Visualizador de Superficie (Dot Grid / Scanning Reticle)")]
        public bool ShowPlaneVisualizer = true;
        public float ReticleRadius = 0.45f;
        public Color ThemeColor = new Color(0.14f, 0.65f, 1.0f, 0.9f);

        [Header("Referencia a la Maqueta / Modelo 3D")]
        public Transform ModelContainer;

        // Componentes de la retícula visual de escaneo
        private GameObject m_visualizerRoot;
        private LineRenderer m_outerRing;
        private LineRenderer m_innerRing;
        private LineRenderer m_crosshair;
        private GameObject m_dotGridPlane;
        private Material m_ringMaterial;
        private Material m_innerMaterial;
        private Material m_dotGridMaterial;

        // Estado para discriminar un toque corto (Tap de destrabe) frente a arrastre o zoom
        private Vector2 m_touchStartPos;
        private float m_touchStartTime;
        private bool m_potentialTap;
        private bool m_multiTouchOccurred;
        private float m_lastPlacementChangeTime = -1f;

        private Vector3 m_mouseStartPos;
        private float m_mouseStartTime;
        private bool m_potentialMouseTap;

        private void Awake()
        {
            Input.simulateMouseWithTouches = false;
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            if (m_trackingTarget != null)
            {
                m_trackingTarget.MoveAnchorOnZ = false;
            }
            SetupSurfaceVisualizer();
        }

        private void OnEnable()
        {
            SetNativeInstantTrackerEnabled(true);
        }

        private void OnDisable()
        {
            SetNativeInstantTrackerEnabled(false);
        }

        private void Start()
        {
            ResolveModelContainer();
            UpdateVisualState();
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            HandleInput();
            AnimateScanningReticle();
        }

        private void SetNativeInstantTrackerEnabled(bool enabled)
        {
            if (m_trackingTarget != null && m_trackingTarget.InstantTracker != null)
            {
                Z.InstantWorldTrackerEnabledSet(m_trackingTarget.InstantTracker.Value, enabled);
            }
        }

        private void ResolveModelContainer()
        {
            if (ModelContainer == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    Transform child = transform.GetChild(i);
                    if (child != m_visualizerRoot?.transform)
                    {
                        ModelContainer = child;
                        break;
                    }
                }
            }

            if (ModelContainer != null && ModelContainer.GetComponent<TouchManipulationController>() == null)
            {
                ModelContainer.gameObject.AddComponent<TouchManipulationController>();
            }
        }

        #region Visualizador de Escaneo de Superficie

        private void SetupSurfaceVisualizer()
        {
            if (!ShowPlaneVisualizer) return;

            m_visualizerRoot = new GameObject("SurfaceScanningVisualizer");
            m_visualizerRoot.transform.SetParent(transform, false);
            m_visualizerRoot.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            m_visualizerRoot.transform.localRotation = Quaternion.identity;

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                              ?? Shader.Find("Sprites/Default")
                              ?? Shader.Find("Unlit/Color");

            m_ringMaterial = new Material(unlitShader) { color = ThemeColor };
            m_innerMaterial = new Material(unlitShader)
            {
                color = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.45f)
            };

            // 1. Aro exterior
            GameObject outerObj = new GameObject("OuterRing");
            outerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_outerRing = outerObj.AddComponent<LineRenderer>();
            ConfigureCircleLine(m_outerRing, m_ringMaterial, 0.015f, 48, ReticleRadius);

            // 2. Aro interior pulsante
            GameObject innerObj = new GameObject("InnerRing");
            innerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_innerRing = innerObj.AddComponent<LineRenderer>();
            ConfigureCircleLine(m_innerRing, m_innerMaterial, 0.008f, 32, ReticleRadius * 0.5f);

            // 3. Cruz central
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_crosshair = crosshairObj.AddComponent<LineRenderer>();
            m_crosshair.useWorldSpace = false;
            m_crosshair.startWidth = 0.01f;
            m_crosshair.endWidth = 0.01f;
            m_crosshair.material = m_ringMaterial;
            m_crosshair.positionCount = 5;
            float halfArm = ReticleRadius * 0.22f;
            m_crosshair.SetPositions(new[]
            {
                new Vector3(-halfArm, 0f, 0f),
                new Vector3(halfArm, 0f, 0f),
                Vector3.zero,
                new Vector3(0f, 0f, -halfArm),
                new Vector3(0f, 0f, halfArm)
            });

            // 4. Malla de puntos (Dot Grid Plane)
            m_dotGridPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_dotGridPlane.name = "DotGridPlane";
            m_dotGridPlane.transform.SetParent(m_visualizerRoot.transform, false);
            m_dotGridPlane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            m_dotGridPlane.transform.localScale = Vector3.one * (ReticleRadius * 2.2f);
            Destroy(m_dotGridPlane.GetComponent<Collider>());

            Texture2D dotTex = Resources.Load<Texture2D>("PlanePatternDot");
            m_dotGridMaterial = new Material(unlitShader)
            {
                color = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.35f)
            };
            if (dotTex != null)
            {
                m_dotGridMaterial.mainTexture = dotTex;
            }
            m_dotGridPlane.GetComponent<MeshRenderer>().material = m_dotGridMaterial;
        }

        private static void ConfigureCircleLine(LineRenderer lr, Material mat, float width, int segments, float radius)
        {
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.material = mat;
            lr.positionCount = segments;

            float step = 360f / segments;
            for (int i = 0; i < segments; i++)
            {
                float rad = Mathf.Deg2Rad * (i * step);
                lr.SetPosition(i, new Vector3(Mathf.Sin(rad) * radius, 0f, Mathf.Cos(rad) * radius));
            }
        }

        private void AnimateScanningReticle()
        {
            if (m_visualizerRoot == null || !m_visualizerRoot.activeSelf) return;

            m_visualizerRoot.transform.Rotate(Vector3.up, 20f * Time.deltaTime, Space.Self);

            if (m_innerRing != null)
            {
                float pulse = 0.8f + Mathf.PingPong(Time.time * 0.7f, 0.35f);
                m_innerRing.transform.localScale = new Vector3(pulse, 1f, pulse);
            }
        }

        private void UpdateVisualState()
        {
            bool isPlaced = m_trackingTarget != null && m_trackingTarget.UserHasPlaced;

            if (m_visualizerRoot != null)
            {
                m_visualizerRoot.SetActive(!isPlaced && ShowPlaneVisualizer);
            }

            if (ModelContainer != null)
            {
                ModelContainer.gameObject.SetActive(isPlaced);
            }
        }

        #endregion

        #region Entrada de Usuario (Colocación y Destrabe por Toque Corto)

        private void HandleInput()
        {
            if ((Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R)) && m_trackingTarget.UserHasPlaced)
            {
                ResetPlacement();
                return;
            }

            if (!AllowTouchToggle) return;

            if (Input.touchCount > 0)
            {
                m_potentialMouseTap = false;
                HandleMobileTouchInput();
                return;
            }

            m_multiTouchOccurred = false;
            HandleDesktopMouseInput();
        }

        private void HandleMobileTouchInput()
        {
            if (Input.touchCount >= 2)
            {
                m_multiTouchOccurred = true;
                m_potentialTap = false;
                return;
            }

            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        m_potentialTap = false;
                        return;
                    }

                    m_multiTouchOccurred = false;
                    m_touchStartPos = touch.position;
                    m_touchStartTime = Time.unscaledTime;

                    if (!m_trackingTarget.UserHasPlaced)
                    {
                        TryPlaceAnchor();
                        m_potentialTap = false;
                    }
                    else
                    {
                        m_potentialTap = true;
                    }
                    break;

                case TouchPhase.Moved:
                    if (m_potentialTap && Vector2.Distance(touch.position, m_touchStartPos) > TouchTapMaxDistancePx)
                    {
                        m_potentialTap = false;
                    }
                    break;

                case TouchPhase.Ended:
                    if (m_potentialTap && !m_multiTouchOccurred && m_trackingTarget.UserHasPlaced)
                    {
                        float duration = Time.unscaledTime - m_touchStartTime;
                        float moveDist = Vector2.Distance(touch.position, m_touchStartPos);

                        if (duration < TapMaxDurationSeconds &&
                            moveDist <= TouchTapMaxDistancePx &&
                            HasPassedPlacementCooldown())
                        {
                            ResetPlacement();
                        }
                    }
                    m_potentialTap = false;
                    break;

                case TouchPhase.Canceled:
                    m_potentialTap = false;
                    break;
            }
        }

        private void HandleDesktopMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    m_potentialMouseTap = false;
                    return;
                }

                m_mouseStartPos = Input.mousePosition;
                m_mouseStartTime = Time.unscaledTime;

                if (!m_trackingTarget.UserHasPlaced)
                {
                    TryPlaceAnchor();
                    m_potentialMouseTap = false;
                }
                else
                {
                    m_potentialMouseTap = true;
                }
            }
            else if (Input.GetMouseButton(0) && m_potentialMouseTap)
            {
                if (Vector3.Distance(Input.mousePosition, m_mouseStartPos) > MouseTapMaxDistancePx)
                {
                    m_potentialMouseTap = false;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (m_potentialMouseTap && m_trackingTarget.UserHasPlaced)
                {
                    float duration = Time.unscaledTime - m_mouseStartTime;
                    float moveDist = Vector3.Distance(Input.mousePosition, m_mouseStartPos);

                    if (duration < TapMaxDurationSeconds &&
                        moveDist <= MouseTapMaxDistancePx &&
                        HasPassedPlacementCooldown())
                    {
                        ResetPlacement();
                    }
                }
                m_potentialMouseTap = false;
            }
        }

        private bool HasPassedPlacementCooldown()
        {
            return Time.unscaledTime - m_lastPlacementChangeTime > PlacementCooldownSeconds;
        }

        private void TryPlaceAnchor()
        {
            if (!HasPassedPlacementCooldown()) return;

            m_trackingTarget.PlaceTrackerAnchor();
            m_lastPlacementChangeTime = Time.unscaledTime;
            UpdateVisualState();
        }

        public void ResetPlacement()
        {
            if (m_trackingTarget == null) return;

            m_trackingTarget.ResetTrackerAnchor();
            m_lastPlacementChangeTime = Time.unscaledTime;
            UpdateVisualState();
        }

        #endregion

        private void OnDestroy()
        {
            if (m_ringMaterial != null) Destroy(m_ringMaterial);
            if (m_innerMaterial != null) Destroy(m_innerMaterial);
            if (m_dotGridMaterial != null) Destroy(m_dotGridMaterial);
        }
    }
}
