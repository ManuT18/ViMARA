# Handoff Report: WebAR Frameworks Investigation

**Agent Archetype**: WebAR Frameworks Specialist Explorer  
**Working Directory**: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks`  
**Target Recipient**: Orchestrator Agent (`f3b4c5b5-48f8-4931-9e36-fe926b2405bb`)  
**Date**: July 24, 2026  

---

## 1. Observation

- **Analyzed Frameworks**:
  1. Google `<model-viewer>` v3.x+ (Open Source, Apache 2.0).
  2. MindAR.js v1.2+ (Open Source, MIT).
  3. AR.js v3.4+ (Open Source, MIT).
  4. Three.js r160+ with Native W3C WebXR Device API (Open Source, MIT).
  5. Commercial Ecosystem: 8th Wall (Niantic), Zappar (ZapWorks / Universal AR), Blippar.
- **Artifacts Created**:
  - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\ORIGINAL_REQUEST.md`
  - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\BRIEFING.md`
  - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\progress.md`
  - `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\analysis.md`
- **Key Technical Facts**:
  - Google `<model-viewer>` wraps Three.js internally for in-browser WebGL, and uses native OS AR engines: **WebXR / Scene Viewer** on Android Chrome and **Apple AR Quick Look (`.usdz`)** on iOS Safari. Payload is ~300 KB gzipped.
  - MindAR.js utilizes **TensorFlow.js WebGL backend** for Natural Feature Tracking (NFT) image target tracking and 468 3D facial landmark mesh tracking. Target files (`.mind`) are precompiled via web/CLI target compiler tools.
  - AR.js relies on **jsartoolkit5** (Emscripten C++ port) for square matrix marker tracking and NFT. NFT image tracking exhibits higher jitter and lower stability compared to MindAR.js.
  - Three.js + Native WebXR API provides pure 60 FPS in-page WebGL rendering with surface hit-testing on Android Chromium (`immersive-ar`), but **iOS Safari lacks native WebXR hit-testing support**, necessitating fallback strategies.
  - Commercial engines (8th Wall $99–$2,000+/mo, Zappar $45+/mo, Blippar per-view SaaS) provide cross-platform SLAM in JS/WASM, but their pricing invalidates them for zero-cost / 100% open-source projects.

---

## 2. Logic Chain

1. **Premise 1**: The user requires a comprehensive evaluation of non-Unity, web-native WebAR frameworks for mobile browsers (iOS/Android), emphasizing 100% free and open-source solutions vs commercial alternatives.
2. **Premise 2**: Unity WebGL builds incur high bundle size overhead (25–40 MB) and memory constraints on mobile browsers, making native JS/WebGL WebAR libraries (<2 MB payload) superior for fast web loading.
3. **Step 1 (Plane Tracking Evaluation)**:
   - For plane surface tracking, `<model-viewer>` provides zero-cost, native ARKit/ARCore performance without writing custom WebGL rendering boilerplate.
   - For custom interactive plane tracking with HTML UI overlays, Three.js + Native WebXR API is ideal on Android, while requiring a `<model-viewer>` / USDZ Quick Look fallback on iOS Safari.
4. **Step 2 (Image Marker Tracking Evaluation)**:
   - For image target tracking (cards, posters, covers), MindAR.js (MIT) outperforms AR.js in tracking stability, multi-marker support, and target compilation toolchain via TensorFlow.js.
5. **Step 3 (Commercial Comparison)**:
   - While 8th Wall provides in-browser SLAM on iOS without WebXR, its subscription cost ($99 to $2,000+/mo) makes it unsuitable for 100% free open-source builds.
6. **Conclusion**: Free open-source combinations (`<model-viewer>` for plane AR showcase, MindAR.js for image target tracking, Three.js + WebXR with iOS fallback for custom plane tracking) fully meet cross-platform WebAR requirements with zero commercial licensing fees.

---

## 3. Caveats

- **iOS WebXR Evolution**: Apple's Safari WebGL/WebXR implementation is subject to change in future iOS releases. Currently, iOS Safari requires fallback to USDZ AR Quick Look or WebGL computer vision (MindAR).
- **Device Hardware Variability**: Performance on mid-tier/budget Android devices running MindAR (TensorFlow.js) may drop below 30 FPS depending on camera resolution and thermal throttling.
- **No Native 6DoF World SLAM in FOSS JS**: There is currently no 100% open-source pure JavaScript SLAM engine matching 8th Wall's proprietary surface tracking on iOS Safari without WebXR or native ARKit/QuickLook invocation.

---

## 4. Conclusion

1. **Recommended Stack for Plane Tracking Showcase**: **Google `<model-viewer>`** (100% Free Apache 2.0, ~300 KB bundle, uses WebXR on Android & AR Quick Look on iOS).
2. **Recommended Stack for Image Target AR**: **MindAR.js + Three.js** (100% Free MIT, superior stability over AR.js, precompiled `.mind` targets).
3. **Recommended Stack for Custom Interactive Plane AR**: **Hybrid Three.js Architecture** (Three.js WebXR API for Android Chrome, `<model-viewer>` / USDZ fallback for iOS Safari).
4. **Commercial Avoidance**: 8th Wall, Zappar, and Blippar can be completely avoided for non-enterprise zero-budget applications.

---

## 5. Verification Method

To independently verify the findings in this report:

1. **Inspect Analysis Report**:
   - Open `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\analysis.md`.
   - Verify that Sections 1 through 8 cover `<model-viewer>`, MindAR.js, AR.js, Three.js + WebXR, Commercial engines, the Master Decision Matrix, and Recommended Stacks.
2. **Verify Matrix Completeness**:
   - Check Section 6 table in `analysis.md` for evaluation parameters: Framework Name, Primary Tracking Mechanism, iOS Support & Method, Android Support & Method, Supported 3D Formats, Bundle Size & Load Speed, Ease of Development, Licensing & Cost.
3. **Cross-Reference Browser Standards**:
   - Verify Android WebXR `immersive-ar` hit-testing spec against W3C WebXR Device API specifications.
   - Verify iOS Safari AR Quick Look `.usdz` requirement via Apple Developer documentation.
