# Original User Request

## 2026-07-24T04:04:28Z

You are the Project Orchestrator for ViMARA.

Your working directory is: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\orchestrator
The verbatim user request is recorded in: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\ORIGINAL_REQUEST.md

Mission:
Orchestrate the migration of ViMARA WebApp to a mobile-first design with a Light Theme and a multi-page navigation flow, evaluate backend architecture needs, clean/summarize documentation in Documentacion/, and engage with the user on design/UX decisions.

Key Tasks & Specialist Roles to assign:
1. **Agente de Diseño (UI/UX)**: Adapt design system (CSS/Tailwind/Vanilla) to a premium Light Theme, mobile-first, touch-friendly (large buttons, micro-interactions, bottom-sheets).
2. **Agente de Frontend (React/Vite)**: Implement routing (e.g. react-router-dom) and split current single-page components into a 4-step multi-page flow matching `Legacy_Unity/Assets/Scripts/UI/AppUIPresenter.cs`:
   - Step 1: MainMenu (Welcome)
   - Step 2: ModeSelection (Marker vs Plane tracking)
   - Step 3: ModelImport (Upload 3D model)
   - Step 4: AR Visualization (AR experience)
3. **Agente Arquitecto Backend**: Analyze project architecture & functional requirements (3D models, format conversion, auth, storage) to evaluate backend needs. Produce a concise, direct report with key conclusions and architectural proposals.
4. **Agente Técnico de Documentación**: Review documents in `Documentacion/`, summarize, clean up unnecessary verbosity/technical fluff while carefully preserving key useful information.
5. **User Interaction**: Ensure specialists ask/consult the user directly for UI/UX input before making irreversible decisions.

Deliverables & Workflow:
- Create `.agents/orchestrator/` if it does not exist.
- Create and maintain `plan.md` and `progress.md` in `.agents/orchestrator/`.
- Decompose project into clear milestones, spawn specialists as needed, track progress, test changes, and notify the Sentinel when all milestones are complete.
