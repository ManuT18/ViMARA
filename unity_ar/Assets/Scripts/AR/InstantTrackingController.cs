using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador oficial para Zappar Instant Tracking en ViMARA.
    /// Características:
    /// 1. Visualizador de Escaneo de Superficie estilo ARKit / ARCore con matriz de puntos y retícula animada.
    /// 2. Toggle táctil fluido (Tocar pantalla para fijar o liberar).
    /// 3. Soporte de manipulación táctil (rotación y zoom) sobre la maqueta anclada.
    /// 4. Calibración ergonómica hacia mesa o suelo.
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

        [Header("Visualizador de Superficie y Detección de Plano")]
        public bool ShowPlaneVisualizer = true;
        public float ReticleRadius = 0.4f;
        public Color ThemeColor = new Color(0.14f, 0.65f, 1.0f, 0.85f); // Azul cian / cielo AR

        // Componentes visuales procedurales
        private GameObject m_visualizerRoot;
        private LineRenderer m_outerRing;
        private LineRenderer m_innerRing;
        private LineRenderer m_crosshair;
        private Material m_dotGridMaterial;
        private GameObject m_dotGridPlane;

        // Referencia a la maqueta para habilitar manipulación táctil
        private Transform m_modelTransform;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            SetupSurfaceVisualizer();
        }

        private void Start()
        {
            // Buscar la maqueta hija y añadirle soporte de rotación y zoom táctil
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child != m_visualizerRoot?.transform)
                {
                    m_modelTransform = child;
                    if (child.GetComponent<TouchManipulationController>() == null)
                    {
                        child.gameObject.AddComponent<TouchManipulationController>();
                    }
                    break;
                }
            }

            UpdateVisualizerState();
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

            Material ringMat = new Material(unlitShader);
            ringMat.color = ThemeColor;

            // 1. Aro exterior principal
            GameObject outerObj = new GameObject("OuterRing");
            outerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_outerRing = outerObj.AddComponent<LineRenderer>();
            ConfigureLine(m_outerRing, ringMat, 0.015f, 48, ReticleRadius);

            // 2. Aro interior pulsante
            GameObject innerObj = new GameObject("InnerRing");
            innerObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_innerRing = innerObj.AddComponent<LineRenderer>();
            Color innerColor = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.45f);
            Material innerMat = new Material(unlitShader) { color = innerColor };
            ConfigureLine(m_innerRing, innerMat, 0.008f, 32, ReticleRadius * 0.5f);

            // 3. Cruz central de alineación
            GameObject crosshairObj = new GameObject("Crosshair");
            crosshairObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_crosshair = crosshairObj.AddComponent<LineRenderer>();
            m_crosshair.useWorldSpace = false;
            m_crosshair.startWidth = 0.01f;
            m_crosshair.endWidth = 0.01f;
            m_crosshair.material = ringMat;
            m_crosshair.positionCount = 5;
            float ch = ReticleRadius * 0.25f;
            m_crosshair.SetPositions(new Vector3[] {
                new Vector3(-ch, 0, 0), new Vector3(ch, 0, 0),
                Vector3.zero,
                new Vector3(0, 0, -ch), new Vector3(0, 0, ch)
            });

            // 4. Plano de puntos de escaneo de superficie (Dot Grid)
            m_dotGridPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_dotGridPlane.name = "ScanningDotGrid";
            m_dotGridPlane.transform.SetParent(m_visualizerRoot.transform, false);
            m_dotGridPlane.transform.localRotation = Quaternion.Euler(90f, 0, 0);
            m_dotGridPlane.transform.localScale = Vector3.one * (ReticleRadius * 2.2f);

            // Destruir el collider innecesario
            Destroy(m_dotGridPlane.GetComponent<Collider>());

            // Cargar textura de puntos si existe en el proyecto
            Texture2D dotTex = Resources.Load<Texture2D>("PlanePatternDot");
            m_dotGridMaterial = new Material(unlitShader);
            m_dotGridMaterial.color = new Color(ThemeColor.r, ThemeColor.g, ThemeColor.b, 0.25f);
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

        private void UpdateVisualizerState()
        {
            if (m_visualizerRoot != null && m_trackingTarget != null)
            {
                m_visualizerRoot.SetActive(!m_trackingTarget.UserHasPlaced && ShowPlaneVisualizer);
            }
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            HandleInput();

            // Modo Preview: proyectar anclaje y animar el escaneo
            if (!m_trackingTarget.UserHasPlaced)
            {
                Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                    m_trackingTarget.InstantTracker.Value,
                    0f, PreviewOffsetY, PreviewDistanceZ,
                    Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                );

                // Animación de rotación del aro exterior
                if (m_visualizerRoot != null && m_visualizerRoot.activeSelf)
                {
                    m_visualizerRoot.transform.Rotate(Vector3.up, 20f * Time.deltaTime, Space.Self);

                    // Efecto de pulso en el aro interior y la grilla
                    float pulse = 0.8f + Mathf.PingPong(Time.time * 0.8f, 0.4f);
                    if (m_innerRing != null)
                    {
                        m_innerRing.transform.localScale = new Vector3(pulse, 1f, pulse);
                    }
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
                        // Si hay 2 dedos, el usuario está haciendo zoom (ignorar fijado/desfijado)
                        if (Input.touchCount == 1)
                        {
                            isClickOrTouchDown = true;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    // En desktop: shift + click o click simple cuando no se arrastra
                    isClickOrTouchDown = true;
                }
            }

            bool isResetKeyPressed = Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R);

            if (isClickOrTouchDown)
            {
                if (!m_trackingTarget.UserHasPlaced)
                {
                    m_trackingTarget.PlaceTrackerAnchor();
                    UpdateVisualizerState();
                    Debug.Log("[ViMARA AR] Plano detectado. Maqueta FIJADA sobre la superficie.");
                }
            }
            else if (isResetKeyPressed && m_trackingTarget.UserHasPlaced)
            {
                m_trackingTarget.ResetTrackerAnchor();
                UpdateVisualizerState();
                Debug.Log("[ViMARA AR] Anclaje liberado para re-posicionar.");
            }
        }

        public void ResetPlacement()
        {
            if (m_trackingTarget != null)
            {
                m_trackingTarget.ResetTrackerAnchor();
                UpdateVisualizerState();
            }
        }
    }
}
