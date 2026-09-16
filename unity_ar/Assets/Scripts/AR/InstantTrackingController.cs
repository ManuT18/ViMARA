using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador para Zappar Instant Tracking.
    /// Permite anclar y re-anclar objetos tanto en dispositivos móviles (Touch) como en PC/Laptop (Mouse Click / Teclado).
    /// Incorpora suavizado (Damping/Lerp) para estabilizar el ancla y evitar temblores bruscos.
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        [Tooltip("Si está activo, tocar la pantalla o hacer clic fijará o liberará el anclaje (Toggle).")]
        public bool AllowTouchToggle = true;

        [Tooltip("Tecla para resetear y reposicionar el anclaje.")]
        public KeyCode ResetKey = KeyCode.Space;

        [Header("Estabilidad y Filtrado (Damping)")]
        [Tooltip("Activa la interpolación suave para evitar que el objeto salte bruscamente.")]
        public bool EnableSmoothing = true;

        [Tooltip("Velocidad de interpolación de la posición. Valores más bajos = más suave pero más lento.")]
        [Range(1f, 30f)]
        public float PositionLerpSpeed = 12f;

        [Tooltip("Velocidad de interpolación de la rotación.")]
        [Range(1f, 30f)]
        public float RotationLerpSpeed = 12f;

        // Estado suavizado interno
        private Vector3 m_smoothedPosition;
        private Quaternion m_smoothedRotation;
        
        // Flag para ignorar saltos grandes al colocar por primera vez
        private bool m_justPlacedOrReset = true;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
        }

        private void Start()
        {
            if (m_trackingTarget != null)
            {
                m_smoothedPosition = transform.localPosition;
                m_smoothedRotation = transform.localRotation;
            }
        }

        // Usamos LateUpdate para asegurarnos de que corremos DESPUÉS de ZapparInstantTrackingTarget.Update()
        private void LateUpdate()
        {
            if (m_trackingTarget == null) return;

            HandleInput();

            if (EnableSmoothing)
            {
                ApplySmoothing();
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
                    m_justPlacedOrReset = true; // Forzamos snap al colocar
                    Debug.Log("[ViMARA AR] Anclaje FIJADO en el espacio.");
                }
                else
                {
                    m_trackingTarget.ResetTrackerAnchor();
                    m_justPlacedOrReset = true; // Forzamos snap al soltar
                    Debug.Log("[ViMARA AR] Anclaje LIBERADO. El objeto vuelve a acompañar a la cámara.");
                }
            }
            else if (isResetKeyPressed && m_trackingTarget.UserHasPlaced)
            {
                m_trackingTarget.ResetTrackerAnchor();
                m_justPlacedOrReset = true;
                Debug.Log("[ViMARA AR] Anclaje reiniciado vía teclado.");
            }
        }

        private void ApplySmoothing()
        {
            // Zappar acaba de actualizar transform.localPos/Rot en su Update()
            Vector3 rawPosition = transform.localPosition;
            Quaternion rawRotation = transform.localRotation;

            if (m_justPlacedOrReset)
            {
                // Si acabamos de anclar o soltar, hacemos "snap" inmediato sin interpolar para evitar un viaje largo
                m_smoothedPosition = rawPosition;
                m_smoothedRotation = rawRotation;
                m_justPlacedOrReset = false;
            }
            else
            {
                // Interpolamos suavemente desde nuestra última posición suavizada hacia la nueva pose cruda
                m_smoothedPosition = Vector3.Lerp(m_smoothedPosition, rawPosition, Time.deltaTime * PositionLerpSpeed);
                m_smoothedRotation = Quaternion.Slerp(m_smoothedRotation, rawRotation, Time.deltaTime * RotationLerpSpeed);
            }

            // Aplicamos la pose suavizada al transform, que es lo que verá la cámara
            transform.localPosition = m_smoothedPosition;
            transform.localRotation = m_smoothedRotation;
        }
    }
}
