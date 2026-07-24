## 2026-07-24T07:05:58Z
You are the React Frontend Specialist (teamwork_preview_worker) for ViMARA.
Your working directory is: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\worker_frontend_nav
Target Workspace: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA

Task:
Implement routing in the React/Vite project (install `react-router-dom` via powershell npm install react-router-dom) and refactor the single-page application into a 4-step multi-page navigation flow matching Legacy_Unity/Assets/Scripts/UI/AppUIPresenter.cs:

Flow Steps:
1. Step 1: MainMenu ('/') - Welcome screen, logo/branding, hero tagline, "Entrar a la App" action button.
2. Step 2: ModeSelection ('/mode-selection') - Select AR tracking mode: "Seguimiento por Marcador" vs "Seguimiento por Plano". Includes info popup modal ("Información de Selección") and back button to MainMenu.
3. Step 3: ModelImport ('/model-import') - Upload custom 3D model (FileUploader) or pick sample model (SampleSelector). Displays selected mode tag, format recommendations (.glb, .gltf, .stl, .obj), "Iniciar AR" button (enabled when a model is selected), and back button to ModeSelection.
4. Step 4: ARVisualization ('/ar-view') - 3D viewer & WebAR experience with <model-viewer>, full screen view, controls, model info badge, back button to ModelImport, and exit button.

Ensure state management keeps track of selected tracking mode and selected 3D model URL/file across route transitions.
Verify build with `npm run build` and `npm run lint`.
