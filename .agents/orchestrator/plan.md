# Orchestration Plan: ViMARA Mobile-First WebApp Migration

## Phase 1: Setup & Initialization
- [x] Update state & memory files (`ORIGINAL_REQUEST.md`, `BRIEFING.md`, `PROJECT.md`, `plan.md`, `progress.md`)
- [ ] Start heartbeat timer

## Phase 2: Execution Track A (Documentation & Backend Architecture)
- [ ] **Worker 1 (`worker_backend_arch`)**: Analyze ViMARA architecture, 3D file pipeline, auth, storage, hosting. Produce `Documentacion/Backend_Architecture_Report.md`.
- [ ] **Worker 2 (`worker_doc_cleanup`)**: Review and summarize documents in `Documentacion/`, removing excessive verbosity while keeping key technical insights intact. Update `AIContext.md` and `CONTEXTO_IA.md`.

## Phase 3: Execution Track B (Mobile-First UI/UX Light Theme & 4-Step Navigation)
- [ ] **Worker 3 (`worker_ui_ux`)**: Design and implement mobile-first Light Theme design system, CSS variables, touch targets, bottom sheets, mobile header/footer, and micro-interactions.
- [ ] **Worker 4 (`worker_frontend_nav`)**: Install `react-router-dom`, refactor single page into 4-step multi-page flow matching `AppUIPresenter.cs` (MainMenu -> ModeSelection -> ModelImport -> ARVisualization).

## Phase 4: Review, Testing & Verification
- [ ] **Reviewer (`teamwork_preview_reviewer`)**: Test build (`npm run build`), lint (`npm run lint`), and verify multi-step navigation flow and mobile design.
- [ ] **Forensic Auditor (`teamwork_preview_auditor`)**: Perform integrity audit on code and documentation deliverables.

## Phase 5: Sentinel Notification & User Handoff
- [ ] Synthesize all results, update `PROJECT.md` and `progress.md`.
- [ ] Report final completion to parent/sentinel and summarize results for user.
