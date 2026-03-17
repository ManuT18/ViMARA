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

    // Botones
    private Button btnModePlane;
    private Button btnModeMarker;
    private Button btnBackToMenu;

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

        // Búsqueda de Botones
        btnModePlane = root.Q<Button>("Btn_SelectPlane");
        btnModeMarker = root.Q<Button>("Btn_SelectMarker");
        btnBackToMenu = root.Q<Button>("Btn_Back");

        // Suscripción a eventos
        if (mainMenuContainer != null) mainMenuContainer.RegisterCallback<PointerDownEvent>(OnEnterAppClicked);
        if (btnModePlane != null) btnModePlane.clicked += OnSelectPlaneClicked;
        if (btnModeMarker != null) btnModeMarker.clicked += OnSelectMarkerClicked;
        if (btnBackToMenu != null) btnBackToMenu.clicked += OnBackToMenuClicked;

        // Mostrar pantalla inicial por defecto
        ShowView(mainMenuContainer);
    }

    private void OnDisable()
    {
        // Cancelar suscripción a eventos para evitar memory leaks
        if (mainMenuContainer != null) mainMenuContainer.UnregisterCallback<PointerDownEvent>(OnEnterAppClicked);
        if (btnModePlane != null) btnModePlane.clicked -= OnSelectPlaneClicked;
        if (btnModeMarker != null) btnModeMarker.clicked -= OnSelectMarkerClicked;
        if (btnBackToMenu != null) btnBackToMenu.clicked -= OnBackToMenuClicked;
    }

    /// <summary>
    /// Oculta todas las vistas y muestra la solicitada
    /// </summary>
    private void ShowView(VisualElement viewToShow)
    {
        if (mainMenuContainer != null) mainMenuContainer.style.display = DisplayStyle.None;
        if (modeSelectionContainer != null) modeSelectionContainer.style.display = DisplayStyle.None;
        if (modelImportContainer != null) modelImportContainer.style.display = DisplayStyle.None;

        if (viewToShow != null)
        {
            viewToShow.style.display = DisplayStyle.Flex;
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

    private void OnBackToMenuClicked()
    {
        ShowView(mainMenuContainer);
    }
}
