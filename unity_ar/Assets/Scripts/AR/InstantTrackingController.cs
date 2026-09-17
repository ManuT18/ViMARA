using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador optimizado para Zappar Instant Tracking en ViMARA.
    /// Características:
    /// 1. Muestra ÚNICAMENTE la retícula de puntos de escaneo de superficie antes de colocar.
    /// 2. Oculta la maqueta 3D hasta que el usuario toca la pantalla para fijarla.
    /// 3. Elimina llamadas duplicadas a la API nativa de Zappar para evitar saturación de memoria y congelamientos.
    /// 4. Habilita manipulación táctil (rotación 360° y zoom) sobre la maqueta fijada.
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

        // Visualizadores procedurales
        private GameObject m_visualizerRoot;
        private LineRenderer m_outerRing;
        private LineRenderer m_innerRing;
        private LineRenderer m_crosshair;
        private GameObject m_dotGridPlane;
        private Material m_ringMaterial;
        private Material m_innerMaterial;
        private Material m_dotGridMaterial;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            SetupSurfaceVisualizer();
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
            m_crosshair.SetPositions(new Vector3[] {
                new Vector3(-ch, 0, 0), new Vector3(ch, 0, 0),
                Vector3.zero,
                new Vector3(0, 0, -ch), new Vector3(0, 0, ch)
            });

            // 4. Matriz de puntos de escaneo de plano
            m_dotGridPlane = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_dotGridPlane.name = "ScanningDotGrid";
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
            bool isClickOrTouchDown = false;

            if (AllowTouchToggle)
            {
                if (Input.touchCount == 1)
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
                    // Fijar anclaje sobre el plano detectado
                    m_trackingTarget.PlaceTrackerAnchor();
                    UpdateVisualState();
                    Debug.Log("[ViMARA AR] Superficie seleccionada. Maqueta fijada.");
                }
            }
            else if (isResetKeyPressed && m_trackingTarget.UserHasPlaced)
            {
                ResetPlacement();
            }
        }

        public void ResetPlacement()
        {
            if (m_trackingTarget != null)
            {
                m_trackingTarget.ResetTrackerAnchor();
                UpdateVisualState();
                Debug.Log("[ViMARA AR] Anclaje liberado. Escaneando superficie...");
            }
        }
    }
}
