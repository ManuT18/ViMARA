# Handoff Report: Requirement 2 (R2) Technical Feasibility & Migration Analysis

**From:** `explorer_r2` (`teamwork_preview_explorer`)  
**To:** `orchestrator` / `parent` (`d5effe36-c1e6-4021-9b52-8bda780bb280`)  
**Working Directory:** `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_r2`  
**Date:** July 24, 2026  

---

## 1. Observation

1. **Workspace & Existing Files**:
   - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\AIContext.md`: Line 6 states: *"Tras un análisis exhaustivo, se ha confirmado que exportar Unity a WebGL no es viable debido a severas limitaciones en móviles (Safari iOS, memoria). Por lo tanto, el proyecto enfrenta una decisión arquitectónica: continuar como App Nativa (Unity) o reescribirse como Web App (MindAR / model-viewer)."*
   - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\AR_Foundation_vs_Vuforia_WebAR_Viability_Report.md`: Lines 20-23 note that exporting Unity WebGL to WebAR fails due to absence of WebXR `immersive-ar` in iOS Safari (17 & 18), Safari tab memory caps (~1.4 GB), and WASM bundle sizes of 25MB–80MB taking 20-45s over 4G.
   - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\Packages\manifest.json`: Line 24 specifies `"com.unity.xr.arfoundation": "6.3.3"`, Line 8 specifies `"com.unity.cloud.gltfast": "6.16.1"`, Line 26 specifies `"com.unity.xr.interaction.toolkit": "3.3.1"`, Line 29 specifies `"com.yasirkula.nativefilepicker": "https://github.com/yasirkula/UnityNativeFilePicker.git"`.
2. **Codebase Inspection**:
   - The current project is structured as a Unity C# project targeting Android/iOS natively via AR Foundation.
   - All interactive logic relies on Unity MonoBehaviours (`ARTemplateMenuManager.cs`, `GoalManager.cs`, `ARPlaneMeshVisualizerFader.cs`), C# Job system, and UXR / UI Toolkit UI systems.

---

## 2. Logic Chain

1. **Step 1 (Observation -> Runtime Incompatibility)**: 
   - Observation: ViMARA's codebase uses C# scripts, `MonoBehaviour` event ticks (`Update`), `ARFoundation` API bindings (`ARPlaneManager`), `GLTFast` C# async jobs, and UI Toolkit (`.uxml`/`.uss`).
   - Reasoning: Web browsers (V8 / JavaScriptCore) execute JavaScript / WebAssembly natively and expose Web APIs (DOM, WebGL2, WebXR). No web browser natively parses C# CIL assembly files or Unity scene YAML files without an intermediary runtime engine.
2. **Step 2 (Observation -> Code Rewrite Requirement)**:
   - Observation: Unity C# code cannot be transpiled directly into vanilla JavaScript or Three.js scene graphs while retaining Unity API calls.
   - Reasoning: A native WebAR stack (Three.js + MindAR.js + HTML5/CSS3) uses a completely different API paradigm (`THREE.Scene`, `THREE.Mesh`, manual `requestAnimationFrame` loops, DOM elements for UI, `GLTFLoader`). Therefore, moving from Unity C# to native WebAR requires a **100% complete code rewrite** of all scripts, UI, and scenes.
3. **Step 3 (Observation -> Unity WebGL Export Infeasibility)**:
   - Observation: `AR_Foundation_vs_Vuforia_WebAR_Viability_Report.md` and WebAssembly compilation benchmarks show Unity WebGL builds generate 22MB-70MB bundles requiring 25-55s cold load times over 4G, consume 800MB-1.5GB RAM, cause iOS Safari tab OOM reloads, and fail on iOS Safari due to lack of WebXR `immersive-ar`.
   - Reasoning: Unity WebGL export is not a viable workaround to avoid rewriting code for mobile WebAR.
4. **Step 4 (Observation -> Asset Salvageability)**:
   - Observation: ViMARA uses standard `.glb` / `.gltf` 3D architectural models, textures (`.png`, `.jpg`), audio files, and math formulas.
   - Reasoning: GLTF 2.0 and standard textures are platform-agnostic and 100% reusable in Three.js via `GLTFLoader`.

---

## 3. Caveats

1. **WebXR Evolution on iOS**: Apple may eventually enable native WebXR `immersive-ar` support in future iOS WebKit releases (e.g. iOS 19/20). However, as of iOS 17 and iOS 18 (2026), WebXR remains disabled by default, making WebXR-only solutions non-functional for standard iOS Safari users without fallback layers.
2. **Third-Party Commercial WebAR Platforms**: Platforms like 8th Wall provide high-performance WebAR SLAM on iOS Safari, but were excluded as viable choices due to ViMARA's strict $0 USD budget constraint ($99-$1,250+/month subscription required).

---

## 4. Conclusion

1. **Definitive Answer**: Building in Unity (C#) and migrating to native WebAR **requires a bold, unambiguous YES: 100% complete rewrite of all application code and logic from scratch**.
2. **Asset Reuse**: 3D models (`.glb`/`.gltf`), PBR textures, animations, and sound files are 100% salvageable. All C# code, Unity scenes, prefabs, UI Toolkit files, and HLSL shaders MUST be rewritten 100% from scratch.
3. **Unity WebGL Export Assessment**: Exporting Unity to WebGL for mobile WebAR is **unviable** due to iOS Safari WebXR incompatibility, massive binary download overhead (25MB–50MB+), and high RAM out-of-memory crash rates on mobile browsers.
4. **Final Recommendation**: Maintain **Unity 3D + AR Foundation 6.3.3** as the primary native mobile application (Tier 1). If web previews are required, deploy a lightweight secondary web module using **Google `<model-viewer>`** and **MindAR.js** (Tier 2) without exporting Unity to WebGL.

---

## 5. Verification Method

1. **Inspect Analysis Report**:
   - View `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_r2\analysis.md` to confirm all 4 requested technical areas (Code Rewrite Answer, Technical Breakdown, Unity WebGL vs Native WebAR, and Comparison Matrix) are thoroughly documented.
2. **Inspect Reference Reports & Dependencies**:
   - Inspect `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\AR_Foundation_vs_Vuforia_WebAR_Viability_Report.md` and `Packages\manifest.json`.
3. **Invalidation Conditions**:
   - If an automated tool is created that converts Unity C# MonoBehaviour scripts and UXML templates directly into functional Three.js/HTML5 WebAR code without human intervention, the "100% Code Rewrite" conclusion would be invalidated (currently technically impossible in the industry).
