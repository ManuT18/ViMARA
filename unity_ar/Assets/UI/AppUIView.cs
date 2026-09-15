using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ViMARA.UI
{
    // Vista: Sólo maneja la parte visual y avisa qué botón se tocó
    public class AppUIView : MonoBehaviour
    {
        [Header("Configuración UI")]
        [SerializeField] private UIDocument appUIDocument;

        [Header("Vista Previa del Modelo")]
        [SerializeField] private RenderTexture modelPreviewTexture; 
        [SerializeField] private Camera previewCamera;

        // <!-- =========================================== -->
        // <!-- Variables de Elementos Visuales             -->
        // <!-- =========================================== -->
        
        //Elemento visual que contiene la textura para la preview
        private VisualElement previewBoxContainer;

        // Elementos raíz y vistas principales (VisualElements)
        private VisualElement root;
        private VisualElement mainMenuContainer;
        private VisualElement modeSelectionContainer;
        private VisualElement modelImportContainer;
        private VisualElement infoPopupContainer;

        // Botones
        private Button btnModePlane;
        private Button btnModeMarker;
        private Button btnGlobalBack;
        private Button btnExitApp;

        // Botones de Info
        private Button btnSelectionInfo;

        // Componentes de Importación de Modelo
        private Button btnOpenFileBrowser;
        private Label lblFileStatus;
        private Button btnStartAR;
        private string selectedFilePath = null;

        // <!-- =========================================== -->
        // <!-- Eventos Públicos (Actions)                  -->
        // <!-- =========================================== -->
        
        // Eventos públicos (Actions) que escuchará el Presentador. Éstas son las alarmas.
        public event Action OnEnterAppClicked;
        public event Action OnSelectPlaneClicked;
        public event Action OnSelectMarkerClicked;
        public event Action OnSelectionInfoClicked;
        public event Action OnCloseInfoClicked;
        public event Action OnExitAppClicked;
        public event Action OnOpenFileBrowserClicked;
        public event Action OnStartARClicked;
        public event Action OnGlobalBackClicked;
        public event Action OnFileSelectedSuccess;

        // <!-- =========================================== -->
        // <!-- Métodos de Inicialización y Búsqueda        -->
        // <!-- =========================================== -->
        
        private void OnEnable()
        {
            root = appUIDocument.rootVisualElement;

            // Búsqueda de contenedores
            mainMenuContainer = root.Q<VisualElement>("MainMenu");
            modeSelectionContainer = root.Q<VisualElement>("ModeSelection");
            modelImportContainer = root.Q<VisualElement>("ModelImport");
            infoPopupContainer = root.Q<VisualElement>("InfoPopup");
            previewBoxContainer = root.Q<VisualElement>("PreviewBox");

            // Búsqueda de Botones
            btnModePlane = root.Q<Button>("Btn_SelectPlane");
            btnModeMarker = root.Q<Button>("Btn_SelectMarker");
            btnGlobalBack = root.Q<Button>("Btn_GlobalBack");
            btnSelectionInfo = root.Q<Button>("Btn_SelectionInfo");
            btnExitApp = root.Q<Button>("Btn_ExitApp");

            // Búsqueda en Pantalla de Carga
            btnOpenFileBrowser = root.Q<Button>("Btn_OpenFileBrowser");
            lblFileStatus = root.Q<Label>("Lbl_FileStatus");
            btnStartAR = root.Q<Button>("Btn_StartAR");
        
            // Suscripción de clics de UI Toolkit hacia nuestros Eventos Action (Alarmas)
            if (mainMenuContainer != null) mainMenuContainer.RegisterCallback<PointerDownEvent>(evt => OnEnterAppClicked?.Invoke());
            
            if (btnModePlane != null) btnModePlane.clicked += () => OnSelectPlaneClicked?.Invoke();
            if (btnModeMarker != null) btnModeMarker.clicked += () => OnSelectMarkerClicked?.Invoke();
            if (btnGlobalBack != null) btnGlobalBack.clicked += () => OnGlobalBackClicked?.Invoke();
            if (btnSelectionInfo != null) btnSelectionInfo.clicked += () => OnSelectionInfoClicked?.Invoke();
            if (btnExitApp != null) btnExitApp.clicked += () => OnExitAppClicked?.Invoke();
            
            if (infoPopupContainer != null) infoPopupContainer.RegisterCallback<PointerDownEvent>(evt => OnCloseInfoClicked?.Invoke());
            
            if (btnOpenFileBrowser != null) btnOpenFileBrowser.clicked += () => OnOpenFileBrowserClicked?.Invoke();
            
            if (btnStartAR != null) 
            {
                btnStartAR.SetEnabled(false); // Inicia deshabilitado por seguridad
                btnStartAR.clicked += () => OnStartARClicked?.Invoke();            
            }            

        }

        // --- MÉTODOS PÚBLICOS PARA QUE EL PRESENTADOR MANDE ÓRDENES ---
        
        public void UpdateFileStatusLabel(string fileName)
        {
            if (lblFileStatus == null) return;
            // A petición, ya no mostramos el texto cuando hay un archivo, lo ocultamos para dar paso al Preview.
            lblFileStatus.style.display = DisplayStyle.None;
        }

        public void SetStartARButtonState(bool isEnabled)
        {
            if (btnStartAR == null) return;
            btnStartAR.SetEnabled(isEnabled);
            btnStartAR.style.opacity = isEnabled ? 1f : 0.5f;

            if (isEnabled)
            {
                // Quitamos la clase de botón gris secundario para que vuelva a su azul original
                btnStartAR.RemoveFromClassList("secondary-button");
            }
            else
            {
                btnStartAR.AddToClassList("secondary-button");
            }
        }

        public void ShowModelPreviewStream()
        {
            if (previewBoxContainer != null && modelPreviewTexture != null)
            {
                previewBoxContainer.style.backgroundImage = new StyleBackground(Background.FromRenderTexture(modelPreviewTexture));
                previewBoxContainer.style.display = DisplayStyle.Flex;
            }
        }

        // --- FUNCIONES PÚBLICAS PARA CAMBIO DE PANTALLA ---
        
        public void ShowMainMenu() { ShowView(mainMenuContainer); }
        public void ShowModeSelection() { ShowView(modeSelectionContainer); }
        public void ShowModelImport() { ShowView(modelImportContainer); }

        public void ShowInfoPopup() 
        { 
            if (infoPopupContainer != null) infoPopupContainer.style.display = DisplayStyle.Flex; 
            btnSelectionInfo.style.display = DisplayStyle.None;
            btnGlobalBack.style.display = DisplayStyle.None;
            btnExitApp.style.display = DisplayStyle.None;
        }

        public void HideInfoPopup() 
        { 
            if (infoPopupContainer != null) infoPopupContainer.style.display = DisplayStyle.None; 
            btnSelectionInfo.style.display = DisplayStyle.Flex;
            btnGlobalBack.style.display = DisplayStyle.Flex;
            btnExitApp.style.display = DisplayStyle.Flex;
        }

        // Oculta todas las vistas y muestra la solicitada (privado para la vista física)
        private void ShowView(VisualElement viewToShow)
        {
            if (mainMenuContainer != null) mainMenuContainer.style.display = DisplayStyle.None;
            if (modeSelectionContainer != null) modeSelectionContainer.style.display = DisplayStyle.None;
            if (modelImportContainer != null) modelImportContainer.style.display = DisplayStyle.None;

            if (viewToShow != null)
            {
                viewToShow.style.display = DisplayStyle.Flex;
            }

            // Visibilidad de flechas globales
            if (btnGlobalBack != null)
            {
                btnGlobalBack.style.display = (viewToShow == modelImportContainer) ? DisplayStyle.Flex : DisplayStyle.None;
                if (viewToShow == modelImportContainer) btnGlobalBack.BringToFront();
            }

            // Visibilidad de info y salir
            if (btnSelectionInfo != null && btnExitApp != null)
            {
                bool isModeSelection = (viewToShow == modeSelectionContainer);
                btnSelectionInfo.style.display = isModeSelection ? DisplayStyle.Flex : DisplayStyle.None;
                btnExitApp.style.display = isModeSelection ? DisplayStyle.Flex : DisplayStyle.None;
                
                if (isModeSelection)
                {
                    btnSelectionInfo.BringToFront();
                    btnExitApp.BringToFront();
                }
            }
        }
    }

}
