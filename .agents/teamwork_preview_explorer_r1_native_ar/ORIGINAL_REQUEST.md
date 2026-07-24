## 2026-07-24T04:52:44Z
<USER_REQUEST>
You are a Native AR Specialist Explorer.
Your working directory is: c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r1_native_ar

OBJECTIVE:
Conduct a rigorous technical and commercial comparison between Unity AR Foundation (versions 5.x/6.x, 2023-2024) and Vuforia Engine (version 10.x+, 2023-2024) for native mobile applications (iOS/Android).

KEY RESEARCH REQUIREMENTS:
1. Setup Complexity & Developer Experience:
   - Integration into Unity via UPM / Packages.
   - Native platform dependencies (ARKit/ARCore setup in AR Foundation vs Vuforia Engine Target Manager & License setup).
   - Build configuration complexity for iOS (Xcode, ARKit privacy keys) and Android (ARCore requirements, Gradle configuration).
2. Plane Tracking Capabilities & Reliability:
   - Underlying abstraction mechanisms (AR Foundation wrapping ARKit/ARCore plane detection vs Vuforia Smart Terrain / VisLAM).
   - Surface detection speed, plane classification (horizontal, vertical), boundary estimation accuracy, occlusion handling, and relocalization under rapid motion or visual drift.
3. Image Tracking (Marcadores) & Marker Stability:
   - Target generation & rating tools (Unity Reference Image Libraries vs Vuforia Target Manager star ratings 1-5).
   - Stability comparison: Jitter reduction, tracking distance, angle tolerance, occlusion resilience, multi-target tracking limits.
   - Runtime dynamic image target creation (creating image targets on-the-fly from downloaded images/URLs).
4. Dynamic Runtime 3D Model Importing:
   - Ecosystem options for importing 3D models (glTF, GLB, FBX, OBJ) dynamically at runtime over network/storage in Unity.
   - Compatibility with Unity tools: GLTFast, UnityGLTF, TriLib 2, Addressables / AssetBundles.
   - Attaching dynamically loaded models to tracked planes or tracked image targets in AR Foundation vs Vuforia.
5. Licensing & Cost Breakdown (CRITICAL ZERO-COST RESTRICTION):
   - Detailed breakdown of Vuforia licensing model (Development watermark in app, Basic/Premium subscription costs, Cloud Recognition fees, limits for free/student/production deployment).
   - AR Foundation cost model (Included with Unity Personal/Pro, zero extra SDK licensing fee, zero production watermarks).
   - Direct assessment against the project mandate for 100% free / zero-cost solution.

INSTRUCTIONS:
- Update progress.md as you work.
- Write your full detailed technical analysis in c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r1_native_ar\analysis.md.
- Write a structured handoff report in c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r1_native_ar\handoff.md.
- Include structured markdown comparison tables for feature comparison and licensing breakdown.
- Send a message to the orchestrator when completed.
</USER_REQUEST>
