using UnityEngine;
using Zappar;

namespace ViMARA.AR
{
    /// <summary>
    /// Controlador para Zappar Instant Tracking.
    /// Permite anclar y re-anclar objetos tanto en dispositivos móviles (Touch) como en PC/Laptop (Mouse Click / Teclado).
    /// </summary>
    [RequireComponent(typeof(ZapparInstantTrackingTarget))]
    public class InstantTrackingController : MonoBehaviour
    {
        private ZapparInstantTrackingTarget m_trackingTarget;

        [Header("Configuración de Interacción")]
        [Tooltip("Si está activo, un clic con el mouse fijará o liberará el anclaje.")]
        public bool AllowMouseClick = true;

        [Tooltip("Tecla para resetear y reposicionar el anclaje.")]
        public KeyCode ResetKey = KeyCode.Space;

        private void Awake()
        {
            m_trackingTarget = GetComponent<ZapparInstantTrackingTarget>();
        }

        private void Update()
        {
            if (m_trackingTarget == null) return;

            // 1. Detección de clic de mouse (para pruebas en Desktop / Laptop)
            if (AllowMouseClick && Input.GetMouseButtonDown(0))
            {
                if (!m_trackingTarget.UserHasPlaced)
                {
                    m_trackingTarget.PlaceTrackerAnchor();
                    Debug.Log("[ViMARA AR] Anclaje fijado en el espacio mediante clic de mouse.");
                }
            }

            // 2. Tecla para resetear / reposicionar la maqueta
            if (Input.GetKeyDown(ResetKey) || Input.GetKeyDown(KeyCode.R))
            {
                m_trackingTarget.ResetTrackerAnchor();
                Debug.Log("[ViMARA AR] Anclaje reiniciado. El objeto vuelve a acompañar a la cámara.");
            }
        }
    }
}
