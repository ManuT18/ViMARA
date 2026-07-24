# Project: ViMARA Mobile-First WebApp Migration & Architecture

## Mission
Migrate the ViMARA WebApp to a mobile-first design with a Light Theme and a multi-page 4-step navigation flow matching Legacy Unity (`AppUIPresenter.cs`), evaluate backend architecture needs, clean/summarize documentation in `Documentacion/`, and consult user on design/UX decisions.

## Architecture & Code Layout
- Root Directory: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA`
- Frontend Framework: React 19 + Vite 8 + CSS / Modern Design Tokens
- Routing: `react-router-dom`
- 3D / WebAR Engine: `@google/model-viewer` + Three.js
- Documentation Directory: `Documentacion/`

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | M1 Backend Architecture Report | Evaluate 3D models, Wasm format conversion, storage, auth, hosting needs. Produce `Documentacion/Backend_Architecture_Report.md`. | None | DONE |
| 2 | M2 Documentation Cleanup | Review, summarize, and clean up technical fluff across `Documentacion/` while preserving key data. | None | DONE |
| 3 | M3 UI/UX Light Theme & Mobile-First | Mobile-first Light Theme, CSS tokens, touch targets (≥48px), bottom sheets, micro-interactions. | None | DONE |
| 4 | M4 React 4-Step Navigation Flow | Restructure React app into 4-step router flow: MainMenu -> ModeSelection -> ModelImport -> ARVisualization matching `AppUIPresenter.cs`. | M3 | DONE |
| 5 | M5 Verification & Forensic Audit | Validate build (`npm run build`), lint (`npm run lint`), test UI flow, and run Forensic Integrity Audit. | M1, M2, M4 | DONE (CLEAN) |

## Interface Contracts & Navigation Steps
1. `/` or `/main-menu`: **Step 1 MainMenu** - Welcome, branding, enter app trigger.
2. `/mode-selection`: **Step 2 ModeSelection** - Marker vs Plane tracking selection, info modal.
3. `/model-import`: **Step 3 ModelImport** - Upload 3D file (`.glb`, `.gltf`, `.stl`, `.obj`), select sample model, preview info.
4. `/ar-view`: **Step 4 ARVisualization** - WebAR view with `<model-viewer>`, full screen toggle, AR trigger button, back button.
