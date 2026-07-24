## 2026-07-24T04:52:44Z
<USER_REQUEST>
You are a WebAR Frameworks Specialist Explorer.
Your working directory is: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks

OBJECTIVE:
Investigate and evaluate non-Unity web-native frameworks, libraries, and tools for WebAR across mobile browsers (iOS/Android), highlighting 100% free and open-source solutions vs commercial solutions.

KEY RESEARCH REQUIREMENTS:
1. Google `<model-viewer>`:
   - Architecture: Web component wrapping Three.js & Google Scene Viewer / Apple AR Quick Look.
   - Capabilities: 3D model rendering (glTF/GLB/USDZ), surface placement (plane tracking), basic interaction, light estimation.
   - Mobile browser behavior: Uses WebXR on Android, native AR Quick Look (USDZ) on iOS.
   - Pros, Cons, Limitations (No custom image marker tracking, basic UI/interaction).
   - Licensing & Cost: 100% Free / Open Source (Apache 2.0).
2. MindAR.js:
   - Architecture: Image Tracking (NFT - Natural Feature Tracking) & Face Tracking powered by TensorFlow.js and WebGL.
   - Integration: Native integration with Three.js and A-Frame.
   - Target Compiler: Web-based target compiler / CLI for generating image target tracking files.
   - Tracking stability, multi-marker support, plane tracking support (or lack thereof).
   - Licensing & Cost: 100% Free / Open Source (MIT License).
3. AR.js:
   - Architecture: Marker-based (Hiro, Kanji, custom pattern markers), Image Tracking (NFT), Location-based AR.
   - Integration: A-Frame and Three.js integration.
   - Tracking performance, setup simplicity, active community status.
   - Licensing & Cost: 100% Free / Open Source (MIT License).
4. Three.js + Native WebXR API:
   - Direct WebXR implementation (`ARButton`, WebXR Manager, hit-testing, anchors).
   - Custom 3D rendering pipeline, ultralight web bundle size (<2MB total).
   - Mobile iOS handling (WebXR Polyfill, WebXR Viewer, or AR Quick Look fallback).
   - Licensing & Cost: 100% Free / Open Source (MIT License).
5. Commercial WebAR Ecosystem (Cost & Capabilities Contrast):
   - 8th Wall (Niantic): World Tracking (SLAM), Image Tracking, Curved targets, WebGL engine agnostic. Cost: $99/mo to $2,000+/mo (non-viable for 100% free project).
   - Zappar (ZapWorks / Universal AR): Three.js/A-Frame integration. Cost: Free tier with watermarks/limits, paid subscriptions.
   - Blippar / Blippbuilder / WebAR SDK: Commercial pricing structure.
6. Comprehensive Framework Evaluation & Decision Matrix:
   - Create a master comparison table evaluating: Framework Name, Primary Tracking Mechanism (Plane vs Image vs Marker vs Location), iOS Support & Method, Android Support & Method, Supported 3D Formats (glTF/GLB/USDZ), Bundle Size & Load Speed, Ease of Development, Licensing & Cost.
   - Recommending 100% free combinations (e.g., MindAR + Three.js for Image Tracking, <model-viewer> or WebXR for Plane Tracking).

INSTRUCTIONS:
- Update progress.md as you work.
- Write your full detailed technical analysis in c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\analysis.md.
- Write a structured handoff report in c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\handoff.md.
- Include structured markdown comparative tables.
- Send a message to the orchestrator when completed.
</USER_REQUEST>
