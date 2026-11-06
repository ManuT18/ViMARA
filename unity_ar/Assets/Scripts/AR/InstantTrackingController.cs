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

        // Componentes del visualizador de escaneo ("Mantel de Puntos" + guía central sutil)
        private GameObject m_visualizerRoot;
        private LineRenderer m_outerRing;
        private LineRenderer m_innerRing;
        private LineRenderer m_crosshair;
        private GameObject m_dotGridPlane;
        private Material m_ringMaterial;
        private Material m_innerMaterial;
        private Material m_dotGridMaterial;
        private Texture2D m_dotGridTexture;

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

        #region Visualizador de Escaneo de Superficie ("Mantel de Puntos")

        private void SetupSurfaceVisualizer()
        {
            if (!ShowPlaneVisualizer) return;

            m_visualizerRoot = new GameObject("SurfaceScanningVisualizer");
            m_visualizerRoot.transform.SetParent(transform, false);
            m_visualizerRoot.transform.localPosition = new Vector3(0f, 0.005f, 0f);
            m_visualizerRoot.transform.localRotation = Quaternion.identity;

            // Sprites/Default está siempre incluido en GraphicsSettings y soporta transparencia alfa real
            // en WebGL + URP sin generar el cuadrado opaco que producía Universal Render Pipeline/Unlit.
            Shader transparentShader = Shader.Find("Sprites/Default")
                                    ?? Shader.Find("Universal Render Pipeline/Unlit")
                                    ?? Shader.Find("Unlit/Transparent");

            Color ringColor = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.55f);
            Color innerColor = new Color(1f, 1f, 1f, 0.35f);

            m_ringMaterial = new Material(transparentShader) { color = ringColor };
            m_innerMaterial = new Material(transparentShader) { color = innerColor };

            // 1. Mantel de Puntos procedural (Dot Grid Tablecloth) con desvanecimiento radial sin bordes cuadrados
            m_dotGridPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_dotGridPlane.name = "DotGridTablecloth";
            m_dotGridPlane.transform.SetParent(m_visualizerRoot.transform, false);
            m_dotGridPlane.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            float tableclothSize = ReticleRadius * 2.6f;
            m_dotGridPlane.transform.localScale = new Vector3(tableclothSize, tableclothSize, 1f);
            Destroy(m_dotGridPlane.GetComponent<Collider>());

            m_dotGridTexture = CreateDotTableclothTexture(256, 13);
            m_dotGridMaterial = new Material(transparentShader)
            {
                color = new Color(1f, 1f, 1f, 0.92f),
                mainTexture = m_dotGridTexture
            };

            MeshRenderer planeRenderer = m_dotGridPlane.GetComponent<MeshRenderer>();
            planeRenderer.sharedMaterial = m_dotGridMaterial;
            planeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            planeRenderer.receiveShadows = false;

            // 2. Anillo perimetral sutil que enmarca el núcleo del mantel de puntos
            GameObject outerObj = new GameObject("OuterRing");
            outerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_outerRing = outerObj.AddComponent<LineRenderer>();
            ConfigureCircleLine(m_outerRing, m_ringMaterial, 0.006f, 64, ReticleRadius * 0.68f);

            // 3. Anillo interior pulsante de enfoque
            GameObject innerObj = new GameObject("InnerRing");
            innerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_innerRing = innerObj.AddComponent<LineRenderer>();
            ConfigureCircleLine(m_innerRing, m_innerMaterial, 0.004f, 40, ReticleRadius * 0.24f);

            // 4. Guía central minimalista (Crosshair)
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_crosshair = crosshairObj.AddComponent<LineRenderer>();
            m_crosshair.useWorldSpace = false;
            m_crosshair.startWidth = 0.004f;
            m_crosshair.endWidth = 0.004f;
            m_crosshair.material = m_innerMaterial;
            m_crosshair.positionCount = 5;
            float halfArm = ReticleRadius * 0.1f;
            m_crosshair.SetPositions(new[]
            {
                new Vector3(-halfArm, 0f, 0f),
                new Vector3(halfArm, 0f, 0f),
                Vector3.zero,
                new Vector3(0f, 0f, -halfArm),
                new Vector3(0f, 0f, halfArm)
            });
        }

        /// <summary>
        /// Genera proceduralmente una textura de retícula de puntos ("Mantel de Puntos") con caída radial suave
        /// de opacidad y tamaño hacia los bordes para que jamás se perciba el contorno cuadrado del Quad.
        /// </summary>
        private Texture2D CreateDotTableclothTexture(int resolution, int gridCount)
        {
            Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false)
            {
                name = "ProceduralDotTablecloth",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };

            Color32[] pixels = new Color32[resolution * resolution];
            float invRes = 1f / (resolution - 1);

            for (int y = 0; y < resolution; y++)
            {
                float ny = (y * invRes) * 2f - 1f; // [-1, 1]
                for (int x = 0; x < resolution; x++)
                {
                    float nx = (x * invRes) * 2f - 1f; // [-1, 1]
                    float radialDist = Mathf.Sqrt(nx * nx + ny * ny);

                    // Fuera del círculo unitario: 100% transparente (elimina cualquier borde cuadrado)
                    if (radialDist >= 0.96f)
                    {
                        pixels[y * resolution + x] = new Color32(0, 0, 0, 0);
                        continue;
                    }

                    // Caída radial suave (smoothstep inverso desde r=0.25 hasta r=0.96)
                    float radialFalloff = 1f - Mathf.SmoothStep(0.22f, 0.96f, radialDist);

                    // Coordenadas de celda dentro de la retícula de gridCount x gridCount
                    float gx = (nx * 0.5f + 0.5f) * gridCount;
                    float gy = (ny * 0.5f + 0.5f) * gridCount;

                    float cellX = gx - Mathf.Floor(gx) - 0.5f; // [-0.5, 0.5]
                    float cellY = gy - Mathf.Floor(gy) - 0.5f; // [-0.5, 0.5]
                    float cellDist = Mathf.Sqrt(cellX * cellX + cellY * cellY);

                    // Los puntos cercanos al centro son ligeramente más grandes y nítidos que los periféricos
                    float dotRadius = Mathf.Lerp(0.11f, 0.21f, radialFalloff);
                    float aaWidth = 0.065f;

                    float dotMask = 1f - Mathf.SmoothStep(dotRadius - aaWidth, dotRadius + aaWidth, cellDist);

                    // Sub-retícula sutil (líneas entrecortadas muy tenues entre puntos para dar cohesión de mantel)
                    float gridLineX = 1f - Mathf.SmoothStep(0.015f, 0.045f, Mathf.Abs(cellX));
                    float gridLineY = 1f - Mathf.SmoothStep(0.015f, 0.045f, Mathf.Abs(cellY));
                    float subtleMesh = Mathf.Max(gridLineX, gridLineY) * 0.12f * radialFalloff;

                    float alpha = Mathf.Clamp01(dotMask * radialFalloff * 0.95f + subtleMesh);

                    // Mezcla cromática: centro más luminoso (blanco/cian) y periferia con el ThemeColor
                    float centerHighlight = Mathf.Clamp01(1f - radialDist * 1.15f) * 0.45f;
                    float r = Mathf.Lerp(ThemeColor.r, 1f, centerHighlight);
                    float g = Mathf.Lerp(ThemeColor.g, 1f, centerHighlight);
                    float b = Mathf.Lerp(ThemeColor.b, 1f, centerHighlight);

                    pixels[y * resolution + x] = new Color(r, g, b, alpha);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply(false, false);
            return tex;
        }

        private static void ConfigureCircleLine(LineRenderer lr, Material mat, float width, int segments, float radius)
        {
            lr.useWorldSpace = false;
            lr.loop = true;
            lr.startWidth = width;
            lr.endWidth = width;
            lr.material = mat;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
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

            if (m_outerRing != null)
            {
                m_outerRing.transform.Rotate(Vector3.up, 14f * Time.deltaTime, Space.Self);
            }

            if (m_innerRing != null)
            {
                float pulse = 0.85f + Mathf.PingPong(Time.time * 0.6f, 0.3f);
                m_innerRing.transform.localScale = new Vector3(pulse, 1f, pulse);
            }

            if (m_dotGridPlane != null)
            {
                float baseScale = ReticleRadius * 2.6f;
                float breathe = 1f + Mathf.Sin(Time.time * 2.2f) * 0.035f;
                m_dotGridPlane.transform.localScale = new Vector3(baseScale * breathe, baseScale * breathe, 1f);
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
            if (m_dotGridTexture != null) Destroy(m_dotGridTexture);
        }
    }
}
