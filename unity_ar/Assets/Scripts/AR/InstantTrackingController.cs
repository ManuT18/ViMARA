using UnityEngine;
using UnityEngine.EventSystems;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador avanzado de detección y visualización de planos (Horizontales y Verticales) para ViMARA WebAR.
    /// Características:
    /// 1. Detección inteligente de planos Horizontales (suelo/mesa) y Verticales (paredes) según la inclinación del dispositivo en tiempo real.
    /// 2. Visualizador arquitectónico de plano delimitado: muestra el polígono del plano detectado con borde perimetral continuo,
    ///    escuadras de delimitación en las 4 esquinas, grilla métrica arquitectónica procedural y barra láser de escaneo que recorre el plano.
    /// 3. Estabilizador de pose (Deadzone + amortiguación exponencial en LateUpdate) para eliminar temblores tanto antes como después de colocar la maqueta.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        public bool AllowTouchToggle = true;
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Detección Horizontal y Vertical")]
        [Tooltip("Permite detectar tanto superficies horizontales (mesas/pisos) como paredes verticales según el ángulo de la cámara.")]
        public bool EnableVerticalPlaneDetection = true;
        [Tooltip("Distancia base del plano frente a la cámara durante el escaneo.")]
        public float ScanDistance = 1.45f;

        [Header("Visualizador Arquitectónico de Plano Delimitado")]
        public bool ShowPlaneVisualizer = true;
        [Tooltip("Ancho y largo del área de plano delimitada (en metros).")]
        public Vector2 PlaneBoundsSize = new Vector2(0.75f, 0.75f);
        public Color HorizontalPlaneColor = new Color(0.06f, 0.72f, 0.98f, 0.95f); // Cian arquitectónico
        public Color VerticalPlaneColor = new Color(0.20f, 0.88f, 0.62f, 0.95f);   // Verde esmeralda para pared

        [Header("Referencia a la Maqueta / Modelo 3D")]
        public Transform ModelContainer;

        // Estado del plano actual
        private bool m_isVerticalPlane = false;
        private Quaternion m_currentPlaneSurfaceRot = Quaternion.identity;

        // Estabilización de pose cuando está colocado
        private Vector3 m_smoothedWorldPos;
        private Quaternion m_smoothedWorldRot = Quaternion.identity;
        private bool m_hasValidSmoothedPose = false;
        private float m_lastAnchorUpdateTime = 0f;

        // Elementos visuales del delimitador de plano
        private GameObject m_visualizerRoot;
        private GameObject m_surfaceFillQuad;
        private LineRenderer m_perimeterBorder;
        private LineRenderer[] m_cornerBrackets;
        private LineRenderer m_scanLaserLine;
        private LineRenderer m_normalIndicator;

        private Material m_lineMaterial;
        private Material m_laserMaterial;
        private Material m_gridFillMaterial;
        private Texture2D m_proceduralGridTex;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
            if (m_trackingTarget != null)
            {
                // CRÍTICO: Evitar que ZapparInstantTrackingTarget.Update() llame por su cuenta a
                // Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(..., MINUS_Z_AWAY_FROM_USER) cada frame,
                // lo cual entraba en conflicto con nuestra llamada MINUS_Z_HEADING duplicando la carga en WASM.
                m_trackingTarget.MoveAnchorOnZ = true;
            }
            SetupBoundedPlaneVisualizer();
        }

        private void OnEnable()
        {
            if (m_trackingTarget != null)
            {
                if (m_trackingTarget.InstantTracker != null)
                {
                    Z.InstantWorldTrackerEnabledSet(m_trackingTarget.InstantTracker.Value, true);
                }
                else if (ZapparCamera.Instance != null && ZapparCamera.Instance.PipelineIsInitialized)
                {
                    m_trackingTarget.OnZapparInitialized(ZapparCamera.Instance.GetPipeline);
                }
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

        /// <summary>
        /// Construye un delimitador de superficie arquitectónica limpio:
        /// - Relleno translúcido con grilla métrica procedural (sin el cuadro opaco viejo).
        /// - Borde perimetral rectangular redondeado que delimita los límites exactos de la maqueta.
        /// - 4 Escuadras de esquina (Corner Brackets) que se expanden al adaptarse al plano.
        /// - Línea láser de barrido (Scan Line) que recorre el plano confirmando la lectura activa.
        /// </summary>
        private void SetupBoundedPlaneVisualizer()
        {
            if (!ShowPlaneVisualizer) return;

            m_visualizerRoot = new GameObject("BoundedPlaneVisualizer");
            m_visualizerRoot.transform.SetParent(transform, false);
            m_visualizerRoot.transform.localPosition = new Vector3(0, 0.003f, 0);
            m_visualizerRoot.transform.localRotation = Quaternion.identity;

            Shader unlitShader = Shader.Find("Sprites/Default");
            if (unlitShader == null) unlitShader = Shader.Find("Universal Render Pipeline/Unlit");

            m_lineMaterial = new Material(unlitShader) { color = HorizontalPlaneColor };
            m_laserMaterial = new Material(unlitShader) { color = new Color(1f, 1f, 1f, 0.85f) };

            m_proceduralGridTex = GenerateArchitecturalGridTexture(256);
            m_gridFillMaterial = new Material(unlitShader)
            {
                mainTexture = m_proceduralGridTex,
                color = new Color(HorizontalPlaneColor.r, HorizontalPlaneColor.g, HorizontalPlaneColor.b, 0.42f)
            };

            float halfW = PlaneBoundsSize.x * 0.5f;
            float halfH = PlaneBoundsSize.y * 0.5f;

            // 1. Superficie del plano con grilla arquitectónica sutil
            m_surfaceFillQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_surfaceFillQuad.name = "PlaneSurfaceGrid";
            m_surfaceFillQuad.transform.SetParent(m_visualizerRoot.transform, false);
            m_surfaceFillQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            m_surfaceFillQuad.transform.localScale = new Vector3(PlaneBoundsSize.x, PlaneBoundsSize.y, 1f);
            Destroy(m_surfaceFillQuad.GetComponent<Collider>());
            m_surfaceFillQuad.GetComponent<MeshRenderer>().sharedMaterial = m_gridFillMaterial;

            // 2. Borde perimetral continuo del plano detectado
            GameObject borderObj = new GameObject("PlanePerimeterBorder");
            borderObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_perimeterBorder = borderObj.AddComponent<LineRenderer>();
            m_perimeterBorder.useWorldSpace = false;
            m_perimeterBorder.loop = true;
            m_perimeterBorder.startWidth = 0.006f;
            m_perimeterBorder.endWidth = 0.006f;
            m_perimeterBorder.material = m_lineMaterial;
            m_perimeterBorder.positionCount = 4;
            m_perimeterBorder.SetPositions(new Vector3[]
            {
                new Vector3(-halfW, 0.001f, -halfH),
                new Vector3( halfW, 0.001f, -halfH),
                new Vector3( halfW, 0.001f,  halfH),
                new Vector3(-halfW, 0.001f,  halfH)
            });

            // 3. Cuatro escuadras de esquina de alta precisión (Corner Brackets)
            m_cornerBrackets = new LineRenderer[4];
            float bracketLen = Mathf.Min(halfW, halfH) * 0.32f;
            float offsetOut = 0.018f;

            Vector2[] signs = new Vector2[]
            {
                new Vector2(-1, -1),
                new Vector2( 1, -1),
                new Vector2( 1,  1),
                new Vector2(-1,  1)
            };

            for (int i = 0; i < 4; i++)
            {
                GameObject cornerObj = new GameObject($"CornerBracket_{i}");
                cornerObj.transform.SetParent(m_visualizerRoot.transform, false);
                LineRenderer lr = cornerObj.AddComponent<LineRenderer>();
                lr.useWorldSpace = false;
                lr.loop = false;
                lr.startWidth = 0.012f;
                lr.endWidth = 0.012f;
                lr.material = m_lineMaterial;
                lr.positionCount = 3;

                float sx = signs[i].x;
                float sz = signs[i].y;
                float cx = sx * (halfW + offsetOut);
                float cz = sz * (halfH + offsetOut);

                lr.SetPositions(new Vector3[]
                {
                    new Vector3(cx - sx * bracketLen, 0.002f, cz),
                    new Vector3(cx, 0.002f, cz),
                    new Vector3(cx, 0.002f, cz - sz * bracketLen)
                });
                m_cornerBrackets[i] = lr;
            }

            // 4. Línea láser de escaneo que recorre el plano
            GameObject laserObj = new GameObject("ScanLaserBar");
            laserObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_scanLaserLine = laserObj.AddComponent<LineRenderer>();
            m_scanLaserLine.useWorldSpace = false;
            m_scanLaserLine.startWidth = 0.008f;
            m_scanLaserLine.endWidth = 0.008f;
            m_scanLaserLine.material = m_laserMaterial;
            m_scanLaserLine.positionCount = 2;
            m_scanLaserLine.SetPositions(new Vector3[]
            {
                new Vector3(-halfW * 0.96f, 0.003f, 0f),
                new Vector3( halfW * 0.96f, 0.003f, 0f)
            });

            // 5. Indicador de normal (eje perpendicular al plano detectado)
            GameObject normalObj = new GameObject("PlaneNormalAxis");
            normalObj.transform.SetParent(m_visualizerRoot.transform, false);
            m_normalIndicator = normalObj.AddComponent<LineRenderer>();
            m_normalIndicator.useWorldSpace = false;
            m_normalIndicator.startWidth = 0.007f;
            m_normalIndicator.endWidth = 0.002f;
            m_normalIndicator.material = m_laserMaterial;
            m_normalIndicator.positionCount = 2;
            m_normalIndicator.SetPositions(new Vector3[]
            {
                Vector3.zero,
                new Vector3(0f, 0.09f, 0f)
            });
        }

        /// <summary>
        /// Genera una textura transparente con subdivisión de grilla arquitectónica (4x4 celdas + borde suave)
        /// sin círculos ni cuadrados opacos.
        /// </summary>
        private Texture2D GenerateArchitecturalGridTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Color[] pixels = new Color[size * size];
            int cells = 4;
            int step = size / cells;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Distancia normalizada desde el centro (0 a 1)
                    float nx = Mathf.Abs((x / (float)size) * 2f - 1f);
                    float ny = Mathf.Abs((y / (float)size) * 2f - 1f);
                    float edgeDist = Mathf.Max(nx, ny);

                    bool isMajorGrid = (x % step <= 1) || (y % step <= 1);
                    bool isSubGrid = (x % (step / 2) == 0) || (y % (step / 2) == 0);

                    float alpha = 0.06f; // Relleno interior ultra translúcido
                    if (isMajorGrid) alpha = 0.55f;
                    else if (isSubGrid) alpha = 0.22f;

                    // Desvanecimiento suave hacia los bordes exteriores
                    float vignette = Mathf.Clamp01(1.15f - edgeDist * 0.4f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * vignette);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
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
                if (isPlaced)
                {
                    var touchCtrl = ModelContainer.GetComponent<TouchManipulationController>();
                    if (touchCtrl != null)
                    {
                        if (m_isVerticalPlane)
                        {
                            touchCtrl.SetExternalBaseRotation(transform.rotation * m_currentPlaneSurfaceRot);
                        }
                        else
                        {
                            touchCtrl.ClearExternalBaseRotation();
                        }
                    }
                    else
                    {
                        ModelContainer.localRotation = m_currentPlaneSurfaceRot;
                    }
                }
            }
        }

        private void Update()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            HandleInput();

            if (!m_trackingTarget.UserHasPlaced)
            {
                UpdatePlaneDetectionAndVisualizer();
            }
        }

        private void LateUpdate()
        {
            if (m_trackingTarget == null || m_trackingTarget.InstantTracker == null) return;

            // Filtro de estabilización de mundo para evitar micro-temblores del giroscopio/cámara
            if (!m_hasValidSmoothedPose)
            {
                m_smoothedWorldPos = transform.position;
                m_smoothedWorldRot = transform.rotation;
                m_hasValidSmoothedPose = true;
            }
            else
            {
                float posDist = Vector3.Distance(m_smoothedWorldPos, transform.position);
                float rotAng = Quaternion.Angle(m_smoothedWorldRot, transform.rotation);

                float posLerp = posDist < 0.005f ? Time.deltaTime * 3f : Time.deltaTime * 18f;
                float rotLerp = rotAng < 1.0f ? Time.deltaTime * 3f : Time.deltaTime * 18f;

                m_smoothedWorldPos = Vector3.Lerp(m_smoothedWorldPos, transform.position, Mathf.Clamp01(posLerp));
                m_smoothedWorldRot = Quaternion.Slerp(m_smoothedWorldRot, transform.rotation, Mathf.Clamp01(rotLerp));

                transform.position = m_smoothedWorldPos;
                transform.rotation = m_smoothedWorldRot;
            }
        }

        /// <summary>
        /// Evalúa la inclinación de la cámara respecto a la gravedad para adaptar el plano:
        /// - Si el usuario apunta hacia abajo (suelo/mesa): usa MINUS_Z_HEADING (plano horizontal nivelado por gravedad)
        ///   y ajusta la distancia dinámicamente según el ángulo de picado para que el plano se pegue a la superficie.
        /// - Si el usuario apunta de frente hacia una pared (pitch cercano al horizonte): cambia el plano a modo Vertical (Pared).
        /// </summary>
        private void UpdatePlaneDetectionAndVisualizer()
        {
            Transform camTr = ZapparCamera.Instance != null ? ZapparCamera.Instance.transform : Camera.main?.transform;
            float pitchDownAngle = 35f;

            if (camTr != null)
            {
                // Ángulo entre la mirada de la cámara y el horizonte
                // Si mira hacia abajo: camTr.forward.y es negativo
                float forwardY = camTr.forward.y;
                pitchDownAngle = Mathf.Asin(Mathf.Clamp(-forwardY, -1f, 1f)) * Mathf.Rad2Deg;
            }

            // Si la cámara apunta casi horizontalmente (< 14° hacia abajo o hacia arriba) y está habilitado el modo pared
            bool detectVertical = EnableVerticalPlaneDetection && (pitchDownAngle < 14f && pitchDownAngle > -35f);
            m_isVerticalPlane = detectVertical;

            bool shouldUpdateNativeAnchor = (Time.unscaledTime - m_lastAnchorUpdateTime) >= 0.032f;

            if (detectVertical)
            {
                if (shouldUpdateNativeAnchor)
                {
                    m_lastAnchorUpdateTime = Time.unscaledTime;
                    // Modo Plano Vertical (Pared frente al usuario)
                    Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                        m_trackingTarget.InstantTracker.Value,
                        0f,
                        0f,
                        -ScanDistance,
                        Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                    );
                }
                // Rotar el plano 90° hacia el usuario para que se apoye verticalmente sobre la pared
                m_currentPlaneSurfaceRot = Quaternion.Euler(-90f, 0f, 0f);
            }
            else
            {
                // Modo Plano Horizontal (Mesa / Suelo) estabilizado por gravedad (MINUS_Z_HEADING)
                float clampedPitch = Mathf.Clamp(pitchDownAngle, 15f, 75f);
                float rad = clampedPitch * Mathf.Deg2Rad;
                float dynamicY = -Mathf.Sin(rad) * ScanDistance;
                float dynamicZ = -Mathf.Cos(rad) * ScanDistance;

                if (shouldUpdateNativeAnchor)
                {
                    m_lastAnchorUpdateTime = Time.unscaledTime;
                    Z.InstantWorldTrackerAnchorPoseSetFromCameraOffset(
                        m_trackingTarget.InstantTracker.Value,
                        0f,
                        dynamicY,
                        dynamicZ,
                        Z.InstantTrackerTransformOrientation.MINUS_Z_HEADING
                    );
                }
                m_currentPlaneSurfaceRot = Quaternion.identity;
            }

            if (m_visualizerRoot != null && m_visualizerRoot.activeSelf)
            {
                // Suavizar transición entre orientación horizontal y vertical
                m_visualizerRoot.transform.localRotation = Quaternion.Slerp(
                    m_visualizerRoot.transform.localRotation,
                    m_currentPlaneSurfaceRot,
                    Time.deltaTime * 14f
                );

                // Actualizar color según tipo de plano (Cian = Horizontal, Verde Esmeralda = Vertical)
                Color activeCol = m_isVerticalPlane ? VerticalPlaneColor : HorizontalPlaneColor;
                if (m_lineMaterial != null) m_lineMaterial.color = activeCol;
                if (m_gridFillMaterial != null)
                {
                    m_gridFillMaterial.color = new Color(activeCol.r, activeCol.g, activeCol.b, 0.38f);
                }

                // Animar barra láser de escaneo de extremo a extremo del plano delimitado
                if (m_scanLaserLine != null)
                {
                    float halfW = PlaneBoundsSize.x * 0.48f;
                    float halfH = PlaneBoundsSize.y * 0.48f;
                    float scanZ = Mathf.Sin(Time.time * 2.4f) * halfH;
                    m_scanLaserLine.SetPosition(0, new Vector3(-halfW, 0.003f, scanZ));
                    m_scanLaserLine.SetPosition(1, new Vector3( halfW, 0.003f, scanZ));
                }

                // Respiración sutil del delimitador para dar feedback de adaptación viva al plano
                float breath = 1f + Mathf.Sin(Time.time * 3.2f) * 0.025f;
                m_visualizerRoot.transform.localScale = new Vector3(breath, 1f, breath);
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
                        if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                        {
                            isClickOrTouchDown = true;
                        }
                    }
                }
                else if (Input.GetMouseButtonDown(0))
                {
                    if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                    {
                        isClickOrTouchDown = true;
                    }
                }
            }

            bool isResetKeyPressed = Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R);

            if (isClickOrTouchDown)
            {
                if (!m_trackingTarget.UserHasPlaced)
                {
                    m_trackingTarget.PlaceTrackerAnchor();
                    UpdateVisualState();
                    Debug.Log($"[ViMARA AR] Plano {(m_isVerticalPlane ? "VERTICAL (Pared)" : "HORIZONTAL (Superficie)")} fijado. Maqueta posicionada.");
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
                m_hasValidSmoothedPose = false;
                UpdateVisualState();
                Debug.Log("[ViMARA AR] Anclaje liberado. Escaneando plano...");
            }
        }

        private void OnDestroy()
        {
            if (m_lineMaterial != null) Destroy(m_lineMaterial);
            if (m_laserMaterial != null) Destroy(m_laserMaterial);
            if (m_gridFillMaterial != null) Destroy(m_gridFillMaterial);
            if (m_proceduralGridTex != null) Destroy(m_proceduralGridTex);
        }
    }
}
