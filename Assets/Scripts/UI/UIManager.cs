using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument appUIDocument;

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

    private void OnEnable()
    {
        // Validación de que existe el UIDocument en la escena
        if (appUIDocument == null)
            appUIDocument = GetComponent<UIDocument>();

        if (appUIDocument == null)
        {
            Debug.LogError("No se encontró el UIDocument. Asigna uno o agrega este componente al GameObject del UIDocument.");
            return;
        }

        root = appUIDocument.rootVisualElement;

        // Búsqueda de contenedores
        mainMenuContainer = root.Q<VisualElement>("MainMenu");
        modeSelectionContainer = root.Q<VisualElement>("ModeSelection");
        modelImportContainer = root.Q<VisualElement>("ModelImport");
        infoPopupContainer = root.Q<VisualElement>("InfoPopup");

        // Búsqueda de Botones
        btnModePlane = root.Q<Button>("Btn_SelectPlane");
        btnModeMarker = root.Q<Button>("Btn_SelectMarker");
        btnGlobalBack = root.Q<Button>("Btn_GlobalBack");
        btnSelectionInfo = root.Q<Button>("Btn_SelectionInfo");
        btnExitApp = root.Q<Button>("Btn_ExitApp");

        // Suscripción a eventos
        if (mainMenuContainer != null) mainMenuContainer.RegisterCallback<PointerDownEvent>(OnEnterAppClicked);
        if (btnModePlane != null) btnModePlane.clicked += OnSelectPlaneClicked;
        if (btnModeMarker != null) btnModeMarker.clicked += OnSelectMarkerClicked;
        if (btnGlobalBack != null) btnGlobalBack.clicked += HandleBackAction;
        if (btnSelectionInfo != null) btnSelectionInfo.clicked += OnSelectionInfoClicked;
        if (btnExitApp != null) btnExitApp.clicked += OnExitAppClicked;
        if (infoPopupContainer != null) infoPopupContainer.RegisterCallback<PointerDownEvent>(OnCloseInfoClicked);

        // Mostrar pantalla inicial por defecto
        ShowView(mainMenuContainer);
    }

    private void OnDisable()
    {
        // Cancelar suscripción a eventos para evitar memory leaks
        if (mainMenuContainer != null) mainMenuContainer.UnregisterCallback<PointerDownEvent>(OnEnterAppClicked);
        if (btnModePlane != null) btnModePlane.clicked -= OnSelectPlaneClicked;
        if (btnModeMarker != null) btnModeMarker.clicked -= OnSelectMarkerClicked;
        if (btnGlobalBack != null) btnGlobalBack.clicked -= HandleBackAction;
        if (btnSelectionInfo != null) btnSelectionInfo.clicked -= OnSelectionInfoClicked;
        if (btnExitApp != null) btnExitApp.clicked -= OnExitAppClicked;
        if (infoPopupContainer != null) infoPopupContainer.UnregisterCallback<PointerDownEvent>(OnCloseInfoClicked);
    }

    // Oculta todas las vistas y muestra la solicitada
    private void ShowView(VisualElement viewToShow)
    {
        if (mainMenuContainer != null) mainMenuContainer.style.display = DisplayStyle.None;
        if (modeSelectionContainer != null) modeSelectionContainer.style.display = DisplayStyle.None;
        if (modelImportContainer != null) modelImportContainer.style.display = DisplayStyle.None;

        if (viewToShow != null)
        {
            viewToShow.style.display = DisplayStyle.Flex;
        }

        // Controlar la visibilidad de los botones flotantes de navegación
        if (btnGlobalBack != null)
        {
            if (viewToShow == modelImportContainer)
            {
                btnGlobalBack.style.display = DisplayStyle.Flex;
                btnGlobalBack.BringToFront();
            }
            else
            {
                btnGlobalBack.style.display = DisplayStyle.None;
            }
        }

        if (btnSelectionInfo != null && btnExitApp != null)
        {
            if (viewToShow == modeSelectionContainer)
            {
                btnSelectionInfo.style.display = DisplayStyle.Flex;
                btnSelectionInfo.BringToFront();
                
                btnExitApp.style.display = DisplayStyle.Flex;
                btnExitApp.BringToFront();
            }
            else
            {
                btnSelectionInfo.style.display = DisplayStyle.None;
                btnExitApp.style.display = DisplayStyle.None;
            }
        }
    }

    private void HandleBackAction()
    {
        // 1. Si hay un pop-up abierto, lo prioritario es cerrarlo
        if (infoPopupContainer != null && infoPopupContainer.style.display == DisplayStyle.Flex)
        {
            infoPopupContainer.style.display = DisplayStyle.None;
            return;
        }

        // 2. Si estamos en Model Import, volvemos a la selección de modos
        if (modelImportContainer != null && modelImportContainer.style.display == DisplayStyle.Flex)
        {
            ShowView(modeSelectionContainer);
            return;
        }
    }

    // --- Métodos de Acción ---

    private void OnEnterAppClicked(PointerDownEvent evt)
    {
        Debug.Log("Entrando a ViMARA...");
        ShowView(modeSelectionContainer);
    }

    private void OnSelectPlaneClicked()
    {
        Debug.Log("Modo Plano AR Seleccionado.");
        // TODO: Cargar escena de AR Plana en el futuro
        ShowView(modelImportContainer);
    }

    private void OnSelectMarkerClicked()
    {
        Debug.Log("Modo Marcador AR Seleccionado.");
        // TODO: Cargar escena de Marcador en el futuro
        ShowView(modelImportContainer);
    }

    private void OnSelectionInfoClicked()
    {
        if (infoPopupContainer != null)
        {
            infoPopupContainer.style.display = DisplayStyle.Flex;
        }
    }

    private void OnCloseInfoClicked(PointerDownEvent evt)
    {
        if (infoPopupContainer != null)
        {
            infoPopupContainer.style.display = DisplayStyle.None;
        }
    }

    private void OnExitAppClicked()
    {
        Debug.Log("Saliendo de ViMARA...");
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
