using UnityEngine;

namespace ViMARA.UI
{
    /// <summary>
    /// Script para hacer que la cámara de la vista previa orbite alrededor del modelo 3D.
    /// </summary>
    public class PreviewRotator : MonoBehaviour
    {
        [Header("Configuración de la cámara de la vista previa")]
        [Tooltip("Velocidad a la que la cámara orbita (grados por segundo).")]
        public float rotationSpeed = 30f;

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
