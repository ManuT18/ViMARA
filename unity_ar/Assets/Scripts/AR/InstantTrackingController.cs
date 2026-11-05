using UnityEngine;
using UnityEngine.EventSystems;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador optimizado para Zappar Instant Tracking en ViMARA.
    /// Características:
    /// 1. Muestra la retícula circular de puntos de escaneo de superficie antes de colocar.
    /// 2. Oculta la maqueta 3D hasta que el usuario toca la pantalla para fijarla.
    /// 3. Si la maqueta ya está colocada y el usuario toca nuevamente la pantalla (toque corto sin arrastrar ni hacer zoom),
    ///    el modelo se "destraba" y vuelve al modo de escaneo para reubicarlo en otro punto.
    /// 4. Habilita manipulación táctil (rotación 360° con 1 dedo y zoom con 2 dedos) sobre la maqueta fijada.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Calibración Ergonómica Inicial")]
        public Vector3 SurfaceOffset = new Vector3(0, -0.45f, -1.4f);

        [Header("Visualizador de Superficie (Dot Grid / Scanning Reticle)")]
        public bool ShowPlaneVisualizer = true;
        public float ReticleRadius = 0.45f;
        public Color ThemeColor = new Color(0.14f, 0.65f, 1.0f, 0.9f);

        [Header("Referencia a la Maqueta / Modelo 3D")]
        public Transform ModelContainer;

        // Visualizadores procedurales originales (retícula circular + malla de puntos)
        private GameObject m_visualizerRoot;
        private LineRenderer m_outerRing;
        private LineRenderer m_innerRing;
        private LineRenderer m_crosshair;
        private GameObject m_dotGridPlane;
        private Material m_ringMaterial;
        private Material m_innerMaterial;
        private Material m_dotGridMaterial;

        // Detección de toque simple (Tap) vs Arrastre/Zoom para destrabar el modelo
        private Vector2 m_touchStartPos;
        private float m_touchStartTime;
        private bool m_potentialTap = false;
        private bool m_multiTouchOccurred = false;
        private float m_lastPlacementChangeTime = -1f;

        private Vector3 m_mouseStartPos;
        private float m_mouseStartTime;
        private bool m_potentialMouseTap = false;

        private void Awake()
        {
            Input.simulateMouseWithTouches = false;
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            if (m_trackingTarget != null)
            {
                // Permitir que ZapparInstantTrackingTarget gestione nativamente el offset del anclaje antes de colocar
                m_trackingTarget.MoveAnchorOnZ = false;
            }
            SetupSurfaceVisualizer();
        }

        private void OnEnable()
        {
            if (m_trackingTarget != null && m_trackingTarget.InstantTracker != null)
            {
                Z.InstantWorldTrackerEnabledSet(m_trackingTarget.InstantTracker.Value, true);
            }
        }

        private void OnDisable()
        {
            if (m_trackingTarget != null && m_trackingTarget.InstantTracker != null)
            {
                Z.InstantWorldTrackerEnabledSet(m_trackingTarget.InstantTracker.Value, false);
            }
        }

        private void Start()
        {
            FindAndSetupModel();
            UpdateVisualState();
        }

        private void FindAndSetupModel()
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

            if (ModelContainer != null)
            {
                if (ModelContainer.GetComponent<TouchManipulationController>() == null)
                {
                    ModelContainer.gameObject.AddComponent<TouchManipulationController>();
                }
            }
        }

        private void SetupSurfaceVisualizer()
        {
            if (!ShowPlaneVisualizer) return;

            m_visualizerRoot = new GameObject("SurfaceScanningVisualizer");
            m_visualizerRoot.transform.SetParent(transform, false);
            m_visualizerRoot.transform.localPosition = new Vector3(0, 0.005f, 0);
            m_visualizerRoot.transform.localRotation = Quaternion.identity;

            Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlitShader == null) unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader == null) unlitShader = Shader.Find("Unlit/Color");

            m_ringMaterial = new Material(unlitShader) { color = ThemeColor };
            m_innerMaterial = new Material(unlitShader) { color = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.45f) };

            // 1. Aro exterior
            GameObject outerObj = new GameObject("OuterRing");
            outerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_outerRing = outerObj.AddComponent<LineRenderer>();
            ConfigureLine(m_outerRing, m_ringMaterial, 0.015f, 48, ReticleRadius);

            // 2. Aro interior pulsante
            GameObject innerObj = new GameObject("InnerRing");
            innerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_innerRing = innerObj.AddComponent<LineRenderer>();
            ConfigureLine(m_innerRing, m_innerMaterial, 0.008f, 32, ReticleRadius * 0.5f);

            // 3. Cruz central
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_crosshair = crosshairObj.AddComponent<LineRenderer>();
            m_crosshair.useWorldSpace = false;
            m_crosshair.startWidth = 0.01f;
            m_crosshair.endWidth = 0.01f;
            m_crosshair.material = m_ringMaterial;
            m_crosshair.positionCount = 5;
            float ch = ReticleRadius * 0.22f;
            m_crosshair.SetPositions(new Vector3[]
            {
                new Vector3(-ch, 0, 0),
                new Vector3(ch, 0, 0),
                new Vector3(0, 0, 0),
                new Vector3(0, 0, -ch),
                new Vector3(0, 0, ch)
            });

            // 4. Malla de puntos (Dot Grid Plane)
            m_dotGridPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_dotGridPlane.name = "DotGridPlane";
            m_dotGridPlane.transform.SetParent(m_visualizerRoot.transform, false);
            m_dotGridPlane.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            m_dotGridPlane.transform.localScale = Vector3.one * (ReticleRadius * 2.2f);
            Destroy(m_dotGridPlane.GetComponent<Collider>());

            Texture2D dotTex = Resources.Load<Texture2D>("PlanePatternDot");
            m_dotGridMaterial = new Material(unlitShader) { color = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.35f) };
            if (dotTex != null)
            {
                m_dotGridMaterial.mainTexture = dotTex;
            }
            m_dotGridPlane.GetComponent<MeshRenderer>().material = m_dotGridMaterial;
        }

        private void ConfigureLine(LineRenderer lr, Material mat, float width, int segments, float radius)
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
                lr.SetPosition(i, new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius));
            }
        }

        private void UpdateVisualState()
        {
            bool isPlaced = m_trackingTarget != null && m_trackingTarget.UserHasPlaced;

            // Retícula de escaneo: activa SOLO cuando NO está fijado
            if (m_visualizerRoot != null)
            {
                m_visualizerRoot.SetActive(!isPlaced && ShowPlaneVisualizer);
            }

            // Maqueta 3D: activa SOLO cuando el usuario la ha FIJADO sobre el plano
            if (ModelContainer != null)
            {
                ModelContainer.gameObject.SetActive(isPlaced);
            }
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            HandleInput();

            // Animación suave de la retícula de escaneo mientras busca superficie
            if (m_visualizerRoot != null && m_visualizerRoot.activeSelf)
            {
                m_visualizerRoot.transform.Rotate(Vector3.up, 20f * Time.deltaTime, Space.Self);

                float pulse = 0.8f + Mathf.PingPong(Time.time * 0.7f, 0.35f);
                if (m_innerRing != null)
                {
                    m_innerRing.transform.localScale = new Vector3(pulse, 1f, pulse);
                }
            }
        }

        private void HandleInput()
        {
            bool isResetKeyPressed = Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R);
            if (isResetKeyPressed && m_trackingTarget.UserHasPlaced)
            {
                ResetPlacement();
                return;
            }

            if (!AllowTouchToggle) return;

            // --- 1. Entrada Táctil (Móvil) ---
            if (Input.touchCount > 0)
            {
                m_potentialMouseTap = false;

                if (Input.touchCount >= 2)
                {
                    m_multiTouchOccurred = true;
                    m_potentialTap = false;
                    return;
                }

                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
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
                        // Si aún NO está colocado, fijar inmediatamente al tocar la pantalla
                        if (Time.unscaledTime - m_lastPlacementChangeTime > 0.25f)
                        {
                            m_trackingTarget.PlaceTrackerAnchor();
                            m_lastPlacementChangeTime = Time.unscaledTime;
                            UpdateVisualState();
                        }
                        m_potentialTap = false;
                    }
                    else
                    {
                        // Si YA está colocado, esperamos a TouchPhase.Ended para distinguir un toque corto (destrabar) de un arrastre (rotar)
                        m_potentialTap = true;
                    }
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    if (m_potentialTap && Vector2.Distance(touch.position, m_touchStartPos) > 18f)
                    {
                        // El usuario está deslizando el dedo para rotar el modelo, no es un toque de destrabe
                        m_potentialTap = false;
                    }
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (m_potentialTap && !m_multiTouchOccurred && m_trackingTarget.UserHasPlaced)
                    {
                        float duration = Time.unscaledTime - m_touchStartTime;
                        float moveDist = Vector2.Distance(touch.position, m_touchStartPos);

                        if (duration < 0.35f && moveDist <= 18f && (Time.unscaledTime - m_lastPlacementChangeTime > 0.25f))
                        {
                            ResetPlacement();
                        }
                    }
                    m_potentialTap = false;
                }
                else if (touch.phase == TouchPhase.Canceled)
                {
                    m_potentialTap = false;
                }

                return;
            }
            else
            {
                m_multiTouchOccurred = false;
            }

            // --- 2. Entrada de Mouse (Desktop) ---
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
                    if (Time.unscaledTime - m_lastPlacementChangeTime > 0.25f)
                    {
                        m_trackingTarget.PlaceTrackerAnchor();
                        m_lastPlacementChangeTime = Time.unscaledTime;
                        UpdateVisualState();
                    }
                    m_potentialMouseTap = false;
                }
                else
                {
                    m_potentialMouseTap = true;
                }
            }
            else if (Input.GetMouseButton(0) && m_potentialMouseTap)
            {
                if (Vector3.Distance(Input.mousePosition, m_mouseStartPos) > 10f)
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

                    if (duration < 0.35f && moveDist <= 10f && (Time.unscaledTime - m_lastPlacementChangeTime > 0.25f))
                    {
                        ResetPlacement();
                    }
                }
                m_potentialMouseTap = false;
            }
        }

        public void ResetPlacement()
        {
            if (m_trackingTarget != null)
            {
                m_trackingTarget.ResetTrackerAnchor();
                m_lastPlacementChangeTime = Time.unscaledTime;
                UpdateVisualState();
            }
        }

        private void OnDestroy()
        {
            if (m_ringMaterial != null) Destroy(m_ringMaterial);
            if (m_innerMaterial != null) Destroy(m_innerMaterial);
            if (m_dotGridMaterial != null) Destroy(m_dotGridMaterial);
        }
    }
}
