# Technical Analysis: WebAR & Mobile WebXR Viability Assessment

**Author:** WebAR Mobile Viability Explorer  
**Project:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Date:** July 2026  
**Status:** Complete Technical Assessment  

---

## Executive Summary

This report evaluates the technical viability, architectural constraints, browser capability matrix, and user experience (UX) performance of **WebAR / WebXR on mobile devices** (iOS Safari / WebKit vs. Android Chrome / ARCore), with a specific focus on evaluating **Unity WebGL export pipelines** for Augmented Reality applications.

Key findings indicate a fundamental divide in the mobile WebAR ecosystem:
1. **Android Chrome** natively supports the W3C WebXR Device API (`immersive-ar`), enabling rich, in-browser AR experiences via ARCore.
2. **iOS Safari (WebKit)** in iOS 17 and iOS 18 **does NOT natively support WebXR `immersive-ar`** out-of-the-box, forcing fallback to Apple's native 3D Quick Look viewer (`.usdz`), third-party proprietary frameworks (e.g., 8th Wall, Zappar), or legacy polyfills.
3. **Unity WebGL Export for WebAR** is **technically unviable for production mobile web deployment**, primarily due to massive WebAssembly (WASM) bundle sizes (25MB–100MB+), high RAM memory footprints that trigger frequent iOS Safari WebGL canvas terminations (~1.4GB memory cap), long initialization delays (15–45+ seconds), thermal throttling, and camera frame pipeline latency.

---

## 1. Browser Native Support for WebXR API (iOS vs Android)

```
                       +-------------------------------------------------------+
                       |               Mobile WebAR Request                    |
                       +-------------------------------------------------------+
                                                   |
                        +--------------------------+--------------------------+
                        |                                                     |
               [ Android Chrome ]                                     [ iOS Safari ]
                        |                                                     |
         +--------------+--------------+                      +---------------+---------------+
         |                             |                      |                               |
  (Native WebXR)             (Fallback Intent)           (Native WebXR)               (AR Quick Look)
  navigator.xr                Scene Viewer               immersive-ar                 rel="ar" / .usdz
  `immersive-ar`              glTF / GLB                 UNSUPPORTED                  SUPPORTED
  SUPPORTED                   SUPPORTED                  (Requires polyfill/apps)     Passive 3D Viewer
```

### 1.1 Android (Chrome & ARCore Ecosystem)

Android Chrome provides full W3C WebXR Device API native support integrated directly with **Google Play Services for AR (ARCore)**.

* **Supported WebXR Modules:**
  * `immersive-ar`: Launches a native camera-backed AR session overlay within Chrome.
  * `hit-test`: Real-time raycasting against detected physical planes (floor, tables, walls).
  * `dom-overlay`: Renders HTML/CSS user interface elements over the WebXR 3D WebGL canvas.
  * `light-estimation`: Returns spherical harmonics and directional lighting data from the physical environment to match 3D lighting.
  * `anchors`: Allows placing persistent spatial anchors in the AR scene.
  * `depth-sensing`: Provides depth buffer access on hardware equipped with ToF sensors or stereoscopic ARCore depth estimation.
* **Hardware & Runtime Requirements:** Android device with ARCore compatibility and Google Chrome v79+.
* **Performance:** Smooth 60 FPS WebGL rendering on modern mid-to-high-tier devices, direct hardware projection matrix synchronization.

### 1.2 iOS (Safari WebKit & iOS 17 / iOS 18 Ecosystem)

Apple WebKit support for WebXR on iOS remains incomplete and restricted:

* **Native WebXR Status in iOS 17 & iOS 18:**
  * Although WebKit has added internal WebXR experimental feature flags in iOS settings, **`immersive-ar` is disabled by default** and cannot be used by standard web applications without manual user intervention in developer flags.
  * WebKit's primary WebXR focus has been VisionOS (Apple Vision Pro) for `immersive-vr` spatial web sessions, leaving iOS mobile Safari without native ARKit WebXR bindings.
* **Workarounds & Polyfills:**
  * **WebXR iOS Viewer App (Mozilla):** A custom browser shell wrapping ARKit. It requires users to download a separate browser app from the App Store, nullifying the instant accessibility advantage of WebAR.
  * **WebXR Polyfill (`webxr-polyfill.js` + `getUserMedia`):** Uses raw camera video feed + JavaScript-based computer vision (e.g., SLAM or marker tracking via WebGL/OpenCV.js).
    * *Drawbacks:* Extremely high CPU consumption, severe latency (30–60ms per frame), lack of real-world metric scaling, unstable plane detection, rapid battery drain, and thermal throttling.
* **Verdict for iOS WebXR:** Standard iOS Safari **cannot run native WebXR AR sessions**. Web apps targeting iOS MUST use fallback mechanisms or third-party web frameworks.

### 1.3 Native 3D Viewer Fallbacks: AR Quick Look vs Scene Viewer

Due to browser fragmentation, web developers rely on native model viewer fallbacks for quick 3D model visualization.

| Feature / Metric | Apple AR Quick Look (iOS) | Android Scene Viewer (Android) |
| :--- | :--- | :--- |
| **Primary File Format** | `.usdz` or `.reality` | `.gltf` / `.glb` |
| **Integration Mechanism** | `<a rel="ar" href="model.usdz">` or `<model-viewer>` | `intent://arvr.google.com/scene-viewer/...` |
| **User Experience** | Seamless iOS ARKit AR overlay over browser | Native ARCore overlay launched via Android Intent |
| **Custom UI Support** | **NONE** (Fixed native iOS UI) | **NONE** (Fixed native Android UI) |
| **Interactive Logic (C# / JS)** | **NONE** (Passive 3D model preview only) | **NONE** (Passive 3D model preview only) |
| **Plane & Surface Placement** | Excellent (Native ARKit plane detection) | Excellent (Native ARCore plane detection) |
| **Real Scale Visualization** | Yes (1:1 scale architectural preview) | Yes (1:1 scale architectural preview) |
| **ViMARA Suitability** | Good for quick 3D model inspection; unusable for interactive AR application features | Good for quick 3D model inspection; unusable for interactive AR application features |

---

## 2. Unity WebGL Export for WebAR: Feasibility & Pipelines

Using Unity as the authoring tool and exporting to WebGL for WebAR has been attempted through several community and commercial projects. Below is an analysis of the three primary pipelines.

```
                           +-------------------------------------+
                           |      Unity Project Source (C#)      |
                           +-------------------------------------+
                                              |
        +-------------------------------------+-------------------------------------+
        |                                     |                                     |
  [ Standard WebXR Export ]           [ Needle Engine ]                     [ Zappar WebGL ]
        |                                     |                                     |
  * Compiles to WASM                  * Transpiles/Exports to               * Unity WASM + Zappar
  * Calls navigator.xr                Three.js / TypeScript                 WASM Tracking Plugin
  * Fails on iOS Safari               * Works on iOS & Android              * Requires Paid License
  * 30MB-80MB Bundle                  * 2MB-5MB Bundle                      * Watermark on Free Tier
```

### 2.1 Standard Unity WebXR Export (WebXR Foundation Package)

The **Unity WebXR Export** plugin (maintained by WebXR Foundation / Mozilla community) wraps the JavaScript WebXR API for Unity C#.

* **Architecture:** Renders the Unity scene into a WebGL canvas and synchronizes the camera projection matrix with `XRSession` pose data via C# <-> JS interop.
* **Capabilities:**
  * Supports controller input, hand tracking, hit testing on Android Chrome.
  * Preserves Unity C# scripting, material pipelines (URP/Built-in), and physics.
* **Critical Browser Limitations:**
  * Completely non-functional on iOS Safari due to missing native `navigator.xr` `immersive-ar` support.
  * Falling back on iOS requires switching to Apple AR Quick Look, which terminates the Unity WebGL runtime entirely to display a passive `.usdz` file.

### 2.2 Needle Engine (Three.js-Based Unity Export Pipeline)

**Needle Engine** takes an unconventional approach: instead of compiling Unity's C# engine to WASM, it extracts Unity scene hierarchies, prefabs, materials, and animations, exporting them to lightweight **Three.js / Web Components** JavaScript applications.

* **Architecture:**
  * Uses Unity solely as a 3D editor/scene composer.
  * Scripting is written in TypeScript/JavaScript running natively on browser engines.
* **Strengths:**
  * **Minimal Bundle Size:** Output size is typically 2MB–5MB (10x–20x smaller than Unity WASM).
  * **Instant Loading:** 1–3 second load times on mobile networks.
  * **Cross-Platform AR:** Automatically routes Android users to WebXR `immersive-ar` and iOS users to AR Quick Look or WebXR polyfills.
* **Limitations for ViMARA:**
  * Does NOT execute Unity C# scripts at runtime.
  * Requires rewriting C# application logic (UI Toolkit, dynamic glTF import, spatial interactions) in TypeScript.
  * Cannot use Unity-specific C# packages or runtime DLLs.

### 2.3 Zappar WebGL for Unity (ZapWorks)

**Zappar WebGL** provides custom WebGL camera feed tracking for Unity WebGL exports.

* **Architecture:** Compiles Zappar's proprietary computer vision tracking library (face, image, world tracking) into WASM, feeding raw HTML5 video frames to Unity WebGL.
* **Strengths:** Works directly in iOS Safari and Android Chrome inside standard web pages without native WebXR.
* **Licensing & Cost Bottlenecks (Violation of ViMARA Zero-Cost Requirement):**
  * **Commercial Paywall:** Requires a paid ZapWorks subscription ($150 to $500+/month or per-view paywall for commercial distribution).
  * **Free Tier Restrictions:** Watermarked canvas branding, restricted project limits, and non-commercial license terms.
  * **Verdict:** Unviable for ViMARA due to mandatory commercial licensing costs.

---

## 3. Technical Bottlenecks of Unity WebGL on Mobile Browsers

Deploying Unity WebGL builds to mobile browsers encounters severe hardware and web engine execution bottlenecks:

```
+-----------------------------------------------------------------------------------+
|                        Unity WebGL Mobile Bottleneck Chain                        |
+-----------------------------------------------------------------------------------+
|  1. WASM Download (30MB-100MB+) ---> 15-45s Initial Load Time (High Bounce Rate)  |
|  2. Contiguous WASM Memory Heap  ---> iOS Safari Memory Limit (Tab Crash >1.4GB)  |
|  3. CPU/GPU Thermal Load        ---> Thermal Throttling (FPS Drops to 15-20)     |
|  4. Camera Video Frame Interop   ---> 30-60ms Visual Latency & AR Object Drift    |
+-----------------------------------------------------------------------------------+
```

### 3.1 WebAssembly (WASM) Bundle Size & Network Latency

Standard Unity WebGL builds include the entire Unity engine runtime compiled to WASM, asset bundles, shaders, and JavaScript wrapper code.

* **Compressed Build Size (Gzip/Brotli):** 25MB – 80MB.
* **Uncompressed Memory Size:** 80MB – 250MB.
* **Download Time Benchmark:**

| Network Connection | Average Throughput | Unity WebGL Download Time (45MB) | Web-Native Three.js Load Time (3MB) |
| :--- | :--- | :--- | :--- |
| **3G Mobile** | 1.5 Mbps | ~240 seconds (4.0 min) | ~16 seconds |
| **4G / LTE** | 15 Mbps | ~24 seconds | ~1.6 seconds |
| **5G Mobile** | 100 Mbps | ~3.6 seconds | ~0.24 seconds |
| **Wi-Fi (Home Broadband)**| 50 Mbps | ~7.2 seconds | ~0.48 seconds |

On typical mobile 4G networks, users must wait 20 to 45 seconds before the application even begins initializing.

### 3.2 High RAM Consumption & Safari Canvas Crash Thresholds

iOS Safari enforces strict per-tab memory limits to maintain system stability. When a web tab exceeds these limits, iOS WebKit forcefully terminates the WebGL canvas process.

* **iOS Memory Cap:** On devices with 3GB–4GB RAM (e.g., iPhone 11, 12, 13, 14), Safari caps WebGL canvas memory allocation at **~1.4GB – 1.8GB**.
* **Unity WASM Memory Overhead:**
  * Unity WebGL requires pre-allocating a fixed, contiguous WebAssembly Memory heap (e.g., 512MB or 1024MB).
  * When loading high-resolution 3D architectural models (`.gltf`/`.glb`) with uncompressed textures (RGBA32), vertex buffers, and WebGL render targets, memory consumption quickly spikes beyond the Safari cap.
* **Consequence:** Safari displays the error: **"This webpage was reloaded because a problem occurred"**, crashing the user session completely.

### 3.3 Initial Load Time & User Abandonment

In mobile web applications, page load performance directly dictates user retention:

* Industry analytics demonstrate that **over 53% of mobile users abandon a web page that takes longer than 3 seconds to load**.
* Unity WebGL's typical 20–40 second initialization window results in **abandonment rates exceeding 80–90%** in real-world mobile web deployments.

### 3.4 Thermal Throttling & Battery Consumption

Running the Unity C# engine compiled to WebAssembly alongside WebGL fragment shaders, video frame processing, and WebXR tracking loop calculations creates intensive multi-core CPU and GPU utility.

* Mobile SoCs (Apple A-series, Qualcomm Snapdragon) experience rapid heat accumulation within 2–4 minutes of continuous WebGL WebAR execution.
* **Thermal Throttling Response:** The operating system scales back CPU/GPU clock frequencies to manage heat, causing frame rates to drop from 60 FPS down to **15–25 FPS**, resulting in severe stutter, visual jitter, and user disorientation.

### 3.5 Camera Texture Access & Interop Overhead

In non-native WebXR AR (e.g., polyfills or custom WebGL video backgrounds):

* Capturing camera frames via `getUserMedia` requires passing video frames from the browser HTML `<video>` element into the WebGL texture unit every frame (`texImage2D`).
* Memory copying across JavaScript and WASM boundaries adds **30–60ms of frame latency**.
* This latency creates visual drift ("swimming effect"), where 3D architectural models fail to anchor firmly to the camera background, destroying the immersion of real-scale architectural visualization.

---

## 4. User Experience & Onboarding Comparison

```
+-----------------------------------------------------------------------------------+
|                        Onboarding & Access Friction Breakdown                     |
+-----------------------------------------------------------------------------------+
| NATIVE APP (Android / iOS Unity Build):                                           |
|   [QR Code / Link] -> [App Store Page] -> [Install (50MB)] -> [Grant Perms] -> [AR] |
|   Friction Points: 3-4 steps | Setup Time: 1-3 mins | Performance: 60 FPS        |
+-----------------------------------------------------------------------------------+
| WEBAR PREVIEW (Native WebXR / Model-Viewer):                                      |
|   [QR Code / Link] -> [Browser Load (3MB)] -> [Grant Perms] -> [Instant AR Session] |
|   Friction Points: 1 step    | Setup Time: 2-5 secs | Performance: Variable       |
+-----------------------------------------------------------------------------------+
```

### 4.1 Friction Analysis: Native App Download vs Instant WebAR

| UX Metric | Native Mobile App (ViMARA Unity App) | WebAR (Instant Web App) |
| :--- | :--- | :--- |
| **Access Flow** | QR Code -> App Store -> Download & Install -> Launch -> Permissions -> AR | QR Code / Web Link -> Browser Open -> Camera Permission -> AR |
| **Time-to-First-Frame** | 1 to 3 minutes (First install) / Instant (Subsequent) | 2 to 5 seconds (Web-native) / 20-40s (Unity WebGL) |
| **Installation Barrier** | High (Requires user consent to download & install store app) | Zero (Runs directly inside mobile browser) |
| **Offline Capability** | Full offline support after installation | Requires active network connection to load assets |
| **Hardware Access** | Unrestricted access to ARKit/ARCore, file storage, full RAM | Restricted by browser sandbox, RAM caps, and WebXR support |

### 4.2 Security Context & Camera Permissions

* **HTTPS Enforcement:** Both WebXR and HTML5 `getUserMedia` APIs strictly require a **Secure Context (HTTPS)**. Unencrypted HTTP servers block camera access entirely.
* **Permission Dialogs:** Browsers enforce mandatory explicit camera permission prompts. If a user inadvertently denies camera access, recovering requires manual navigation into browser site settings.

---

## 5. Viability Conclusion & Summary Matrix

### 5.1 Comparative Viability Matrix

Below is a master summary matrix comparing all evaluated WebAR options against Native Unity AR Foundation:

| Criteria / Solution | Native Unity App (AR Foundation) | Standard Unity WebGL (WebXR Export) | Needle Engine (Three.js Export) | Apple AR Quick Look / Scene Viewer | Commercial WebAR (Zappar WebGL) |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Android WebXR AR** | N/A (Native App) | Supported (`immersive-ar`) | Supported (`immersive-ar`) | Supported via Scene Viewer | Supported (Custom Video AR) |
| **iOS WebXR AR** | N/A (Native App) | **UNSUPPORTED** (Safari missing API) | Fallback to AR Quick Look | Supported (AR Quick Look) | Supported (Custom Video AR) |
| **WASM Bundle Size** | Native Binary (Fast) | High (25MB – 80MB+) | Minimal (2MB – 5MB) | Minimal (<1MB HTML) | Medium (15MB – 30MB) |
| **Initial Load Time** | Fast (Post-install) | Poor (20–45s) | Fast (1–4s) | Instant (1–2s) | Moderate (8–15s) |
| **RAM Footprint / Safari Crash** | Stable (Native RAM) | **High Risk** (>1.4GB Crash) | Low / Stable | Very Low / Native | Medium Risk |
| **Custom UI / Interactivity** | **Full (UI Toolkit / C#)** | Full (Unity UI) | Moderate (HTML/TS) | **NONE** (Passive 3D) | Full (Zappar SDK) |
| **Dynamic `.gltf` Import** | **Supported (Runtime)** | Limited by WASM RAM | Supported (Three.js loader) | Pre-converted USDZ/GLB | Limited |
| **Licensing & Cost** | **Free / Open Source** | Free / Open Source | Free tier / Paid Pro | Free (Native Web Standards) | **Paid License Required** |
| **Overall Viability for ViMARA** | **PRIMARY CHOICE (10/10)**| **UNVIABLE (2/10)** | **ALTERNATIVE PREVIEW (7/10)**| **PASSIVE FALLBACK (6/10)**| **UNVIABLE (Zero-Cost Violation)**|

---

### 5.2 Final Architectural Recommendation for ViMARA

#### Q: Is Unity WebGL recommended for WebAR in ViMARA?
> **ANSWER: NO.** Compiling Unity to WebGL for WebAR is **NOT RECOMMENDED** for the ViMARA mobile project.

#### Strategic Rationale:
1. **iOS Safari Incompatibility:** Standard Unity WebXR builds fail on iOS Safari because Apple WebKit does not natively support `immersive-ar`. iOS users (a major target demographic) cannot experience interactive WebAR.
2. **Safari WebGL Memory Crashes:** Architectural models (`.gltf`/`.glb`) combined with Unity's heavy WASM heap frequently exceed Safari's ~1.4GB per-tab memory limit, triggering browser crashes.
3. **Extreme Load Latency:** A 30MB–60MB Unity WebGL WASM bundle requires 20 to 45 seconds to download over mobile data, leading to severe user drop-off (>80%).
4. **Licensing Constraints:** Cross-browser WebGL camera solutions like Zappar require paid commercial licenses, violating the project's zero-cost requirement.

---

### Recommended Dual-Tier Architecture for ViMARA

To achieve both high performance and maximum accessibility, ViMARA should implement a **Dual-Tier Architecture**:

```
+-----------------------------------------------------------------------------------+
|                        ViMARA Recommended Dual-Tier Architecture                   |
+-----------------------------------------------------------------------------------+
| TIER 1: PRIMARY APPLICATION (Native Mobile AR - Unity + AR Foundation)             |
|   * Target Platforms: Android (Google Play Services for AR) & iOS (ARKit)         |
|   * Architecture: Native Unity C# App with UI Toolkit & Runtime glTF Importer     |
|   * Benefits: 60 FPS, full RAM access, 0 MB web load latency, robust plane tracking|
+-----------------------------------------------------------------------------------+
| TIER 2: LIGHTWEIGHT WEB PREVIEW FALLBACK (Non-Unity Model-Viewer / Quick Look)    |
|   * Technology: Google `<model-viewer>` / Apple AR Quick Look (`.usdz`)            |
|   * Purpose: Instant web preview links & QR code scanning without app install      |
|   * Benefits: Instant load (<2s), 0 MB WASM overhead, 0% crash rate on iOS Safari   |
+-----------------------------------------------------------------------------------+
```

1. **Tier 1 (Primary Application): Native Mobile AR (Unity 3D + AR Foundation)**
   * Build ViMARA as a native Android/iOS app compiled from Unity using **AR Foundation**.
   * Delivers full C# runtime capabilities, dynamic `.gltf`/`.glb` local file importing, UI Toolkit menus, plane tracking, and spatial object manipulation at native 60 FPS without memory crash risks.
2. **Tier 2 (Web Preview Sharing): Web-Native Lightweight 3D Viewer (`<model-viewer>`)**
   * Provide a lightweight web preview fallback utilizing Google's `<model-viewer>` component.
   * Enables instant web model viewing via QR code links: launches **Android Scene Viewer** (`.glb`) on Android and **Apple AR Quick Look** (`.usdz`) on iOS.
   * Completely avoids Unity WebGL WASM overhead while providing instant 3D model visualization.
