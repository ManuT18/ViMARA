using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ViMARA.UI
{
    public class AppUIPresenter : MonoBehaviour
    {
        [SerializeField] private AppUIView view;
        private AppUIModel model;

        private void Awake()
        {
            // Instanciamos el modelo de datos.
            model = new AppUIModel();
        }

        private void OnEnable()
        {
            if (view == null) return;

            // Suscribimos el Presentador a las alarmas de la Vista.
            view.OnEnterAppClicked += HandleEnterApp;
            view.OnOpenFileBrowserClicked += HandleFileBrowserRequest;
            view.OnStartARClicked += HandleStartARRequest;
            view.OnSelectPlaneClicked += HandleSelectPlane;
            view.OnSelectMarkerClicked += HandleSelectMarker;
            view.OnGlobalBackClicked += HandleGlobalBack;
            view.OnSelectionInfoClicked += HandleSelectionInfo;
            view.OnCloseInfoClicked += HandleCloseInfo;
            view.OnExitAppClicked += HandleExitApp;            
        }

        private void OnDisable()
        {
            if (view == null) return;

            // Siempre desuscribir para evitar fugas de memoria
            view.OnEnterAppClicked -= HandleEnterApp;
            view.OnOpenFileBrowserClicked -= HandleFileBrowserRequest;
            view.OnStartARClicked -= HandleStartARRequest;
            view.OnSelectPlaneClicked -= HandleSelectPlane;
            view.OnSelectMarkerClicked -= HandleSelectMarker;
            view.OnGlobalBackClicked -= HandleGlobalBack;
            view.OnSelectionInfoClicked -= HandleSelectionInfo;
            view.OnCloseInfoClicked -= HandleCloseInfo;
            view.OnExitAppClicked -= HandleExitApp;            
        }

        // --- MANEJADORES DE LA LÓGICA DE NEGOCIO ---

        private void HandleFileBrowserRequest()
        {
            Debug.Log("[Presenter] La vista pide abrir el explorador de archivos.");

#if UNITY_EDITOR
            string path = EditorUtility.OpenFilePanel("Selecciona el modelo 3D", "", "gltf,glb,obj,stl");
            if (!string.IsNullOrEmpty(path))
            {
                ProcessFileSelection(path);
            }
#else
            Debug.LogWarning("Importador celular requiere NativeFilePicker.");
            // NativeFilePicker.PickFile(...)
#endif
        }

        private void ProcessFileSelection(string path)
        {
            // 1. Guardar en el Modelo
            model.SetFilePath(path);

            // 2. Le pide a la Vista que se actualice con el nombre corto
            var fileName = System.IO.Path.GetFileName(path);
            view.UpdateFileStatusLabel(fileName);
            
            // 3. Le da luz verde a la vista para habilitar el botón "INICIAR AR" 
            view.SetStartARButtonState(true);
            
            // 4. Prende el "streaming" de la vista previa de la cámara a la UI
            view.ShowModelPreviewStream();
            
            // TODO (Fase 4): ¡Aquí es donde instanciarás en el mundo 3D usando GLTFast!
        }

        private void HandleStartARRequest()
        {
            if (string.IsNullOrEmpty(model.SelectedFilePath)) return;

            Debug.Log($"[Presenter] Cambiando a Escena de Realidad Aumentada con el modelo: {model.SelectedFilePath}");
            // Lógica para cambiar de escena y pasar la ruta.
        }
        
        private void HandleEnterApp()
        {
            Debug.Log("[Presenter] Entrando a la App.");
            view.ShowModeSelection();
        }

        private void HandleSelectPlane()
        {
            Debug.Log("[Presenter] Cambiando a modo Plano.");
            // En tu lógica vieja íbamos a la pestaña de Importar Modelo
            view.ShowModelImport();
        }

        private void HandleSelectMarker()
        {
            Debug.Log("[Presenter] Cambiando a modo Marcador.");
            view.ShowModelImport();
        }

        private void HandleGlobalBack()
        {
            // TODO A FUTURO: Para hacer el botón de 'Atrás' inteligente como lo tenías antes,
            // lo ideal en MVP es que el 'model' tenga una variable de estado (Ej: model.PantallaActual)
            // y según esa variable sepamos a dónde retroceder (al Menú o a la Selección de modo).
            // Por ahora, para simplificar y no bloquearte, volvemos al modo de selección.
            
            Debug.Log("[Presenter] Volviendo atrás.");
            view.ShowModeSelection();
        }

        private void HandleSelectionInfo()
        {
            Debug.Log("[Presenter] Mostrando información de selección.");
            view.ShowInfoPopup();
        }

        private void HandleCloseInfo()
        {
            Debug.Log("[Presenter] Cerrando información.");
            view.HideInfoPopup();
        }

        private void HandleExitApp()
        {
            Debug.Log("[Presenter] Saliendo de la aplicación.");
            Application.Quit();
        }
    }
}
