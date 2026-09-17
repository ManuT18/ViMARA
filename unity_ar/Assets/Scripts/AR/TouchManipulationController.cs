using UnityEngine;
using UnityEngine.EventSystems;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador de manipulación táctil intuitivo para maquetas en WebAR.
    /// Soporta:
    /// - 1 Dedo (arrastre horizontal): Rotación 360° en eje Y.
    /// - 2 Dedos (pellizco / pinch): Escalado / Zoom suave con límites ergonómicos.
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
        private Vector3 m_targetScale;
        private Vector2 m_lastTouchPos;
        private float m_lastPinchDistance;
        private bool m_isInteracting = false;
        private bool m_isMouseInteracting = false;

        private void Start()
        {
            m_targetRotationY = transform.localEulerAngles.y;
            m_targetScale = transform.localScale;
        }

        private void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
            ApplySmoothTransform();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;
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
            else if (Input.touchCount >= 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began || !m_isInteracting)
                {
                    if (EventSystem.current != null && (EventSystem.current.IsPointerOverGameObject(touch0.fingerId) || EventSystem.current.IsPointerOverGameObject(touch1.fingerId))) return;
                    m_lastPinchDistance = currentDistance;
                    m_isInteracting = true;
                }
                else if ((touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved) && m_isInteracting)
                {
                    float deltaDistance = currentDistance - m_lastPinchDistance;
                    float scaleFactor = 1f + (deltaDistance * PinchZoomSpeed);

                    float newScaleX = Mathf.Clamp(m_targetScale.x * scaleFactor, MinScale, MaxScale);
                    float newScaleY = Mathf.Clamp(m_targetScale.y * scaleFactor, MinScale, MaxScale);
                    float newScaleZ = Mathf.Clamp(m_targetScale.z * scaleFactor, MinScale, MaxScale);

                    m_targetScale = new Vector3(newScaleX, newScaleY, newScaleZ);
                    m_lastPinchDistance = currentDistance;
                }
                else if (touch0.phase == TouchPhase.Ended || touch1.phase == TouchPhase.Ended)
                {
                    m_isInteracting = false;
                }
            }
        }

        private void HandleMouseInput()
        {
            // Rotación con Clic Izquierdo del Mouse
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                m_lastTouchPos = Input.mousePosition;
                m_isMouseInteracting = true;
            }
            else if (Input.GetMouseButton(0) && m_isMouseInteracting)
            {
                float deltaX = Input.mousePosition.x - m_lastTouchPos.x;
                if (Mathf.Abs(deltaX) > 0.1f)
                {
                    m_targetRotationY -= deltaX * MouseRotationSpeed * Time.deltaTime * 60f;
                }
                m_lastTouchPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                m_isMouseInteracting = false;
            }

            // Escalado con Rueda del Mouse
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
                float scaleFactor = 1f + (scroll * ScrollZoomSpeed);
                float newScaleX = Mathf.Clamp(m_targetScale.x * scaleFactor, MinScale, MaxScale);
                float newScaleY = Mathf.Clamp(m_targetScale.y * scaleFactor, MinScale, MaxScale);
                float newScaleZ = Mathf.Clamp(m_targetScale.z * scaleFactor, MinScale, MaxScale);
                m_targetScale = new Vector3(newScaleX, newScaleY, newScaleZ);
            }
        }

        private void ApplySmoothTransform()
        {
            // Interpolación suave de rotación
            Quaternion targetRotation = Quaternion.Euler(transform.localEulerAngles.x, m_targetRotationY, transform.localEulerAngles.z);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * SmoothDamping);

            // Interpolación suave de escala
            transform.localScale = Vector3.Lerp(transform.localScale, m_targetScale, Time.deltaTime * SmoothDamping);
        }

        public void ResetTransformation(Vector3 defaultScale)
        {
            m_targetRotationY = 0f;
            m_targetScale = defaultScale;
            transform.localRotation = Quaternion.identity;
            transform.localScale = defaultScale;
        }
    }
}
