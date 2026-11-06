using UnityEngine;
using UnityEngine.EventSystems;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de manipulación táctil y de escritorio para maquetas 3D en WebAR.
    /// Soporta:
    /// - 1 Dedo (arrastre horizontal): Rotación 360° en eje Y.
    /// - 2 Dedos (pellizco / pinch): Escalado uniforme con bloqueo de rotación accidental al soltar un dedo.
    /// - Mouse (Desktop): Clic izquierdo + arrastre para rotar, rueda del ratón para zoom.
    /// </summary>
    public class TouchManipulationController : MonoBehaviour
    {
        [Header("Sensibilidad de Rotación")]
        public float RotationSpeed = 0.4f;
        public float MouseRotationSpeed = 3.5f;

        [Header("Sensibilidad de Escalado")]
        public float PinchZoomSpeed = 0.003f;
        public float ScrollZoomSpeed = 0.15f;
        public float MinScale = 0.05f;
        public float MaxScale = 3.0f;

        [Header("Suavizado (Inercia)")]
        public float SmoothDamping = 12f;

        private float m_targetRotationY;
        private float m_currentYawOffset;
        private Vector3 m_targetScale;
        private Quaternion m_externalBaseRotation = Quaternion.identity;
        private bool m_useExternalBaseRotation;

        private Vector2 m_lastTouchPos;
        private Vector3 m_lastMousePos;
        private float m_lastPinchDistance;
        private bool m_isInteracting;
        private bool m_isPinching;
        private bool m_wasPinching;
        private bool m_isMouseInteracting;

        private void Awake()
        {
            Input.simulateMouseWithTouches = false;
        }

        private void Start()
        {
            m_targetRotationY = 0f;
            m_currentYawOffset = 0f;
            m_targetScale = transform.localScale;
        }

        public void SetExternalBaseRotation(Quaternion baseWorldRot)
        {
            m_useExternalBaseRotation = true;
            m_externalBaseRotation = baseWorldRot;
        }

        public void ClearExternalBaseRotation()
        {
            m_useExternalBaseRotation = false;
        }

        private void Update()
        {
            if (Input.touchCount > 0)
            {
                m_isMouseInteracting = false;
                HandleTouchInput();
            }
            else
            {
                m_isInteracting = false;
                m_isPinching = false;
                m_wasPinching = false;
                HandleMouseInput();
            }

            ApplySmoothTransform();
        }

        private void HandleTouchInput()
        {
            // Modo Zoom exclusivo con 2 o más dedos (prohibido rotar durante pellizco)
            if (Input.touchCount >= 2)
            {
                m_wasPinching = true;
                m_isInteracting = false;

                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                if (!m_isPinching || touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    if (IsPointerOverUI(touch0.fingerId) || IsPointerOverUI(touch1.fingerId)) return;
                    m_lastPinchDistance = currentDistance;
                    m_isPinching = true;
                }
                else if (m_isPinching && (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved))
                {
                    float deltaDistance = currentDistance - m_lastPinchDistance;
                    ApplyScaleFactor(1f + (deltaDistance * PinchZoomSpeed));
                    m_lastPinchDistance = currentDistance;
                }
                else if (touch0.phase == TouchPhase.Ended || touch0.phase == TouchPhase.Canceled ||
                         touch1.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Canceled)
                {
                    m_isPinching = false;
                }
                return;
            }

            // Si queda 1 dedo tras un pellizco de 2 dedos, bloquear rotación hasta soltar ambos dedos
            if (Input.touchCount == 1)
            {
                m_isPinching = false;
                Touch touch = Input.GetTouch(0);

                if (m_wasPinching)
                {
                    m_lastTouchPos = touch.position;
                    m_isInteracting = false;
                    return;
                }

                if (touch.phase == TouchPhase.Began)
                {
                    if (IsPointerOverUI(touch.fingerId)) return;
                    m_lastTouchPos = touch.position;
                    m_isInteracting = true;
                }
                else if (touch.phase == TouchPhase.Moved && m_isInteracting)
                {
                    float deltaX = touch.position.x - m_lastTouchPos.x;
                    m_targetRotationY -= deltaX * RotationSpeed;
                    m_lastTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    m_isInteracting = false;
                }
            }
        }

        private void HandleMouseInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsMouseOverUI()) return;
                m_lastMousePos = Input.mousePosition;
                m_isMouseInteracting = true;
            }
            else if (Input.GetMouseButton(0) && m_isMouseInteracting)
            {
                float deltaX = Input.mousePosition.x - m_lastMousePos.x;
                if (Mathf.Abs(deltaX) > 0.1f)
                {
                    m_targetRotationY -= deltaX * MouseRotationSpeed * Time.deltaTime * 60f;
                }
                m_lastMousePos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_isMouseInteracting = false;
            }

            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                if (IsMouseOverUI()) return;
                ApplyScaleFactor(1f + (scroll * ScrollZoomSpeed));
            }
        }

        private void ApplyScaleFactor(float scaleFactor)
        {
            m_targetScale = new Vector3(
                Mathf.Clamp(m_targetScale.x * scaleFactor, MinScale, MaxScale),
                Mathf.Clamp(m_targetScale.y * scaleFactor, MinScale, MaxScale),
                Mathf.Clamp(m_targetScale.z * scaleFactor, MinScale, MaxScale)
            );
        }

        private static bool IsPointerOverUI(int fingerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        private static bool IsMouseOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void ApplySmoothTransform()
        {
            float factor = Time.deltaTime * SmoothDamping;
            m_currentYawOffset = Mathf.Lerp(m_currentYawOffset, m_targetRotationY, factor);

            if (m_useExternalBaseRotation)
            {
                transform.rotation = m_externalBaseRotation * Quaternion.Euler(0f, m_currentYawOffset, 0f);
            }
            else
            {
                Quaternion targetRotation = Quaternion.Euler(0f, m_currentYawOffset, 0f);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, factor);
            }

            transform.localScale = Vector3.Lerp(transform.localScale, m_targetScale, factor);
        }

        public void ResetTransformation(Vector3 defaultScale)
        {
            m_targetRotationY = 0f;
            m_currentYawOffset = 0f;
            m_targetScale = defaultScale;
            if (!m_useExternalBaseRotation)
            {
                transform.localRotation = Quaternion.identity;
            }
            transform.localScale = defaultScale;
        }
    }
}
