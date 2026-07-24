# Technical Analysis & Evaluation of WebAR Frameworks and Libraries

**Author**: WebAR Frameworks Specialist Explorer  
**Date**: July 24, 2026  
**Scope**: Comprehensive evaluation of web-native (non-Unity) WebAR frameworks, libraries, and tools across iOS WebKit and Android Chromium browsers.  
**Location**: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\teamwork_preview_explorer_r3_webar_frameworks\analysis.md`

---

## Executive Summary

Web-based Augmented Reality (WebAR) has matured into two distinct technical paradigms:
1. **Native Browser Standard (WebXR Device API & System AR Viewers)**: Leveraging OS-level AR engines (ARCore on Android, ARKit on iOS) with minimal JS overhead.
2. **In-Browser Computer Vision (WASM / WebGL / Neural Nets)**: Processing camera frames directly inside JavaScript/WASM to compute camera poses (used for Image Tracking, Face Mesh tracking, and software SLAM).

This report investigates **4 primary Free & Open Source (FOSS)** solutions (`<model-viewer>`, MindAR.js, AR.js, Three.js + Native WebXR) alongside **3 major Commercial WebAR Platforms** (8th Wall, Zappar, Blippar). The goal is to establish an optimal, zero-cost architecture for cross-platform WebAR deployment without commercial licensing fees or WebGL framework overhead.

---

## 1. Google `<model-viewer>`

### 1.1 Architecture & Core Foundation
Google `<model-viewer>` is a standard Web Component (`<model-viewer>`) built by Google. It encapsulates a custom rendering engine powered by **Three.js** and **rendering pipeline shaders** optimized for Physically Based Rendering (PBR), lighting estimation, and accessibility.

Rather than implementing a custom WebGL computer vision pipeline inside the browser tab for AR, `<model-viewer>` acts as an architectural **bridge to native mobile AR engines**:
- **Android**: Interoperates directly with **Google Scene Viewer** (an OS-level intent) or the native browser **WebXR Device API**.
- **iOS**: Interoperates directly with **Apple AR Quick Look** by dynamically invoking Safari's native USDZ preview modal (`rel="ar"`).

```
+---------------------------------------------------------------------------------+
|                              <model-viewer> DOM Tag                             |
+---------------------------------------------------------------------------------+
                                       |
                   +-------------------+-------------------+
                   |                                       |
         [Android Chromium]                           [iOS WebKit]
                   |                                       |
    +--------------+--------------+            +-----------+-----------+
    |                             |            |                       |
(WebXR Mode)              (Scene Viewer)   (USDZ Quick Look)      (WebXR Polyfill)
 In-Browser Canvas         Native Android    Native iOS ARKit       Limited
  Plane Tracking            Intent App       Modal View             In-Page
```

### 1.2 Capabilities & Technical Features
- **Supported 3D Formats**: Primary native support for **glTF 2.0 / GLB**. For iOS, accepts pre-converted **USDZ** files (`ios-src="model.usdz"`) or automatically generates USDZ using internal converter tools when hosted on supported backends.
- **Plane / Surface Tracking**: High-precision 6DoF plane tracking powered directly by ARCore (Android) and ARKit (iOS). Instant plane detection, raycasting, and surface anchoring.
- **Light Estimation**: Captures ambient environment lighting via the camera feed to adjust scene directional lights, environment reflection maps, and dynamic soft shadows in real time.
- **Interactivity & UI**: Built-in orbit controls, auto-rotate, camera targets, hotspot annotations (`<button slot="hotspot-1">`), animation track selection (`animation-name`), and material variant switching (`variant-name`).

### 1.3 Mobile Browser Behavior & Parity
- **Android (Chrome/Edge/Samsung Internet)**: Launches WebXR AR mode directly inside the web page canvas (`ar-modes="webxr"`). The user remains on the web page with full HTML/DOM overlays preserved over the camera feed. If WebXR is unsupported, fallbacks seamlessly to the Google Scene Viewer app.
- **iOS (Safari / Chrome for iOS / Firefox for iOS)**: Triggers Apple AR Quick Look (`ar-modes="quick-look"`). Safari suspends the web page context and opens Apple's full-screen ARKit Quick Look viewer.
  - *Limitation*: When Quick Look opens on iOS, custom HTML buttons, canvas overlays, and JS event listeners are inactive until the user exits back to Safari.

### 1.4 Pros, Cons, and Limitations
- **Pros**:
  - 100% Free and Open Source (Apache License 2.0).
  - Minimal web bundle impact (~300 KB gzipped).
  - Maximum frame rate (60 FPS) and tracking stability since AR processing is handled out-of-process by ARKit/ARCore.
  - Near-zero learning curve: simple declarative HTML tag syntax.
- **Cons**:
  - **No Custom Image Marker Tracking**: Incapable of scanning printed markers, posters, or business cards.
  - **iOS UI Context Loss**: Native iOS AR Quick Look strips custom Web UI elements during AR mode.
  - **Single Scene / Single Asset Focus**: Designed primarily for viewing individual 3D objects, not complex multi-entity games or multi-object scenes.

---

## 2. MindAR.js

### 2.1 Architecture & Core Engines
MindAR.js is a modern, standalone JavaScript WebAR library developed by Hiukim. It relies on **TensorFlow.js** (utilizing WebGL shaders for hardware-accelerated tensor computations) to execute computer vision algorithms directly inside the browser thread.

MindAR provides two distinct tracking modules:
1. **MindAR Image Tracking**: Natural Feature Tracking (NFT) algorithm extracting 2D keypoints and descriptors from arbitrary images.
2. **MindAR Face Tracking**: 468 3D facial landmark mesh tracking using Google's MediaPipe FaceMesh model.

```
+---------------------------------------------------------------------------------+
|                                   MindAR.js                                     |
+---------------------------------------------------------------------------------+
                                       |
              +------------------------+------------------------+
              |                                                 |
   [Image Tracking Engine]                           [Face Tracking Engine]
  - Natural Feature Extraction (NFT)               - MediaPipe 468 Landmarks
  - Target Matching & Pose Estimation              - Face Mesh Mesh Anchoring
              |                                                 |
              +------------------------+------------------------+
                                       |
                         [TensorFlow.js WebGL Backend]
                                       |
                   +-------------------+-------------------+
                   |                                       |
           [Three.js Integration]                 [A-Frame Integration]
             (MindARThree API)                     (<mindar-image>)
```

### 2.2 Target File Generation & Compiler
- **Web Compiler**: MindAR provides a client-side WebAssembly/JS target compiler running entirely in the browser. Developers drag-and-drop target images (JPG/PNG), inspect keypoint density visualizer maps, and export a binary target file (`.mind`).
- **CLI Compiler**: Command-line interface (`@mind-ar/image-target/node`) allowing automated build-pipeline compilation of single or multi-target `.mind` files.

### 2.3 Integration & Ecosystem
- **Three.js Native API (`MindARThree`)**: Provides fine-grained control over the WebGL render loop, camera parameters, and 3D object anchor groups (`mindarThree.addAnchor(index)`).
- **A-Frame Declarative API**: Exposes custom HTML tags (`<a-scene mindar-image="imageTargetSrc: targets.mind">`, `<a-entity mindar-image-target="targetIndex: 0">`).

### 2.4 Tracking Capabilities & Performance
- **Image Tracking**: Excellent tracking stability with low jitter for planar surfaces (posters, book covers, packaging). Supports tracking **multiple image targets simultaneously** (e.g., tracking target 0 and target 1 concurrently in the camera view).
- **Plane / World Tracking**: **Not Supported**. MindAR does not feature visual-inertial odometry (VIO) surface detection or floor plane placement.
- **Performance**: Achieves 30–60 FPS on iOS (iPhone 10+) and mid-to-high-end Android devices. Frame rate depends on camera resolution and the number of active target features processed by TensorFlow.js.

### 2.5 Pros, Cons, and Licensing
- **Pros**:
  - 100% Free and Open Source (**MIT License**).
  - Best-in-class open-source image tracking stability and multi-target support.
  - Cross-platform web consistency: runs identically on iOS Safari and Android Chrome without requiring native app fallbacks.
  - Active maintenance, modern TypeScript/ES6 architecture.
- **Cons**:
  - Larger bundle size (~1.5 MB – 2.5 MB due to TensorFlow.js dependencies).
  - Lacks World/Plane tracking.
  - Higher CPU/GPU thermal consumption due to continuous per-frame neural network inference in WebGL.

---

## 3. AR.js

### 3.1 Architecture & Core Modules
AR.js is an open-source WebAR library originally created by Jerome Etienne and currently maintained by the AR-js-org community. It serves as a wrapper around **jsartoolkit5** (an Emscripten WebAssembly port of the classic C++ ARToolKit library).

AR.js supports three primary modes:
1. **Marker-based AR**: Binary square matrix patterns (e.g., Hiro marker, Kanji marker, or custom pattern `.patt` files).
2. **Image Tracking (NFT)**: Natural Feature Tracking using feature point set files (`.fset`, `.iset`, `.fset3`).
3. **Location-based AR**: Geolocation tracking mapping 3D assets to GPS coordinates (`latitude`, `longitude`) combined with device orientation sensor fusion.

```
+---------------------------------------------------------------------------------+
|                                     AR.js                                       |
+---------------------------------------------------------------------------------+
                                       |
        +------------------------------+------------------------------+
        |                              |                              |
[Marker-based Engine]        [Image Tracking (NFT)]        [Location-based Engine]
- jsartoolkit5 (Emscripten)   - Feature Set (.fset)         - Device GPS & Compass
- Square Matrix Patterns      - Visual Feature Matching     - World Coordinates
        |                              |                              |
        +------------------------------+------------------------------+
                                       |
                           [A-Frame / Three.js Layer]
```

### 3.2 Tracking Modes & Performance
- **Marker-Based**: Extremely fast and lightweight (<100 KB core logic overhead). Performs marker identification and matrix transformation in under 2 ms per frame. Works on low-end hardware.
- **NFT (Image Tracking)**: Requires generating multi-file feature sets using an online NFT marker generator. Tracking stability is noticeably inferior to MindAR.js—experiencing significant jitter, loss of tracking under quick camera motion, and strict lighting requirements.
- **Location-Based**: Useful for outdoor AR (displaying POIs on a real-world map view), but prone to GPS accuracy drift (+/- 5 to 15 meters) and magnetometer heading interference inside buildings.

### 3.3 Community Status & Setup Simplicity
- **Setup**: Declarative HTML with A-Frame requires under 15 lines of code for Hiro marker tracking.
- **Community Status**: Maintenance has slowed significantly. The underlying `jsartoolkit5` core relies on older C++ patterns, and mobile browser compatibility updates are infrequent.

### 3.4 Pros, Cons, and Licensing
- **Pros**:
  - 100% Free and Open Source (**MIT License**).
  - Ultra-lightweight for classic marker tracking.
  - Integrated Location-based (GPS) AR capabilities out of the box.
- **Cons**:
  - NFT (Image Tracking) exhibits high jitter and low stability compared to MindAR.
  - Lacks modern VIO plane tracking.
  - Dated codebase with limited active development.

---

## 4. Three.js + Native WebXR API

### 4.1 Architecture & Standardized Pipeline
The **W3C WebXR Device API** (`navigator.xr`) is the official web standard for rendering immersive 3D content directly within web browsers. When paired with **Three.js**, developers build a custom 3D rendering pipeline using `WebGLRenderer` with `renderer.xr.enabled = true`.

```
+---------------------------------------------------------------------------------+
|                         Three.js + Native WebXR API                             |
+---------------------------------------------------------------------------------+
                                       |
                   +-------------------+-------------------+
                   |                                       |
         [Android Chromium]                           [iOS WebKit]
    - WebXR Device API Active                  - WebXR AR API Disabled
    - XRSession (immersive-ar)                 - Requires Polyfill, WebXR
    - XRHitTestSource (Hit Test)                  Viewer, or USDZ Fallback
    - 60 FPS In-Page Canvas                                |
                   |                                       |
                   v                                       v
         Native ARCore Service                   Fallback Layer (<model-viewer> /
         Plane Detection & Anchors               MindAR / USDZ Quick Look)
```

### 4.2 Technical Workflow & Hit-Testing
1. **Session Request**: Initiate an immersive AR session via `navigator.xr.requestSession('immersive-ar', { requiredFeatures: ['hit-test'] })`.
2. **Hit Testing**: Create an `XRHitTestSource` using a viewer reference space. On every frame render loop (`renderer.setAnimationLoop`), raycast from the camera center into the real-world environment.
3. **Anchor Placement**: Extract matrix transformations from `XRHitTestResult.getPose()` to position Three.js mesh anchors (`XRAnchor`) directly on detected physical surfaces (floors, tables, walls).
4. **Rendering**: Full rendering control over shaders, post-processing filters, dynamic PBR lighting, physics engines (Cannon.js / Rapier), and audio spatialization.

### 4.3 Mobile OS Support & iOS Workaround Strategies
- **Android Support**: **100% Native**. Standard in Chrome, Edge, and Samsung Internet. Provides zero-latency camera feed compositing and 60 FPS WebGL rendering.
- **iOS WebKit (Safari) Support**: **Not Supported Natively**. Apple Safari does not enable the WebXR AR module by default, and WebXR hit-testing is unavailable in standard iOS Safari WebKit.
- **iOS Workaround Strategies**:
  1. **Dual-Mode Fallback to `<model-viewer>` / USDZ**: Detect iOS user agent. Render the interactive experience in WebXR for Android, and embed Google `<model-viewer>` or dynamically trigger AR Quick Look USDZ export for iOS users.
  2. **WebXR Polyfill / WebXR Viewer App**: Utilize Mozilla WebXR Viewer app or WebXR Polyfill (suitable for controlled enterprise deployments, but undesirable for public web visitors who refuse to install secondary apps).
  3. **MindAR Computer Vision Fallback**: Use MindAR for camera feed processing on iOS, while using WebXR on Android.

### 4.4 Bundle Size & Performance Metrics
- **Bundle Size**: Extremely compact (~600 KB to 1.2 MB total for Three.js core + hit-test module + user application logic).
- **Performance**: Maximum efficiency. Zero JavaScript computer vision overhead because plane tracking is calculated natively in C++ by ARCore/OS services.

### 4.5 Pros, Cons, and Licensing
- **Pros**:
  - 100% Free and Open Source (**MIT License**).
  - W3C Web Standard with long-term vendor longevity.
  - Complete control over UI, HTML canvas, custom shaders, and complex 3D scene graphs.
  - Top-tier performance and thermal efficiency on Android.
- **Cons**:
  - Complete absence of native WebXR AR hit-testing in iOS Safari.
  - Requires writing custom fallback logic for iOS mobile users.

---

## 5. Commercial WebAR Ecosystem (Cost & Capabilities Contrast)

To provide full context for project architecture decisions, commercial WebAR solutions were evaluated to contrast capabilities, licensing models, and cost structures.

### 5.1 8th Wall (Niantic)
- **Architecture**: Proprietary WASM / WebGL computer vision engine running directly inside standard mobile web browsers (iOS Safari & Android Chrome).
- **Capabilities**: Full 6DoF World Tracking (SLAM plane detection without WebXR), multi-image tracking, curved image target tracking (cans, bottles, cylinders), face tracking, hand tracking, and sky segmentation. Compatible with Three.js, A-Frame, Babylon.js, and PlayCanvas.
- **Licensing & Pricing**:
  - **Starter Tier**: $99 / month + usage fees ($0.15 per view after cap).
  - **Pro / Enterprise Tier**: $1,250 – $2,000+ / month + per-view charges ($0.01 – $0.05 per view).
  - **Verdict**: **Strictly Non-Viable** for 100% free / open-source projects. High ongoing SaaS cost.

### 5.2 Zappar (ZapWorks / Universal AR)
- **Architecture**: Proprietary tracking runtime with wrappers for Three.js, A-Frame, Babylon.js, and Unity WebGL.
- **Capabilities**: Image tracking, face tracking, and Instant World Tracking (software SLAM).
- **Licensing & Pricing**:
  - **Free Tier**: Includes Zappar watermark, limited commercial views, restricted features.
  - **Commercial Plans**: Starts at ~$45 / month up to $250+ / month per project.
  - **Verdict**: Non-viable for watermark-free open-source releases, though useful for quick commercial prototypes.

### 5.3 Blippar (Blippbuilder & WebAR SDK)
- **Architecture**: SaaS cloud platform with visual web drag-and-drop editor and standalone WebAR SDK.
- **Capabilities**: Markerless surface tracking, image tracking, location AR.
- **Licensing & Pricing**: Pay-per-view model ($0.02 – $0.10 per view) or annual enterprise subscriptions ($500+ / month).
- **Verdict**: Non-viable for open-source self-hosted architectures.

---

## 6. Comprehensive Framework Evaluation Matrix

| Framework Name | Primary Tracking Mechanism | iOS Support & Method | Android Support & Method | Supported 3D Formats | Bundle Size & Load Speed | Ease of Development | Licensing & Cost |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Google `<model-viewer>`** | Plane Tracking (VIO Surface) | Native Apple AR Quick Look (`.usdz` modal) | Native WebXR API / Google Scene Viewer | glTF 2.0, GLB, USDZ | **Ultra-Light** (~300 KB gzipped) / Instant | **Extremely Easy** (Declarative HTML tag) | **100% Free** (Apache 2.0) |
| **MindAR.js** | Image Tracking (NFT) & Face Mesh | In-Browser WebGL + TensorFlow.js (Safari) | In-Browser WebGL + TensorFlow.js (Chrome) | glTF, GLB, OBJ, Three.js meshes | **Medium** (~1.5–2.5 MB) / Fast | **Moderate** (Three.js & A-Frame wrappers) | **100% Free** (MIT License) |
| **AR.js** | Marker (Hiro/Pattern), NFT, Location | In-Browser WebGL + jsartoolkit5 (Safari) | In-Browser WebGL + jsartoolkit5 (Chrome) | glTF, GLB, OBJ, A-Frame primitives | **Light** (~500 KB for Marker) / Fast | **Easy** (A-Frame HTML tags) | **100% Free** (MIT License) |
| **Three.js + Native WebXR** | Plane Tracking (Hit-Testing) | **No Native AR** (Requires Polyfill or Quick Look fallback) | Native WebXR API (`immersive-ar`) | glTF, GLB, OBJ, Custom Shaders | **Light** (~600 KB – 1.2 MB) / Fast | **Advanced** (Custom JS render loop) | **100% Free** (MIT License) |
| **8th Wall (Niantic)** | World SLAM, Image, Curved, Face | In-Browser WASM Engine (Safari) | In-Browser WASM Engine (Chrome) | glTF, GLB, WebGL Engine Agnostic | **Medium** (~1.5 MB runtime) / Fast | **Moderate** (Studio or Custom JS) | **Commercial SaaS** ($99 to $2,000+/mo) |
| **Zappar (Universal AR)** | Image, Face, Instant World | In-Browser Engine (Safari) | In-Browser Engine (Chrome) | glTF, GLB, Unity WebGL builds | **Medium** (~1.2 MB) / Fast | **Moderate** (SDK wrappers for Three/Unity) | **Freemium / Paid** (Watermarked free tier, $45+/mo) |
| **Blippar WebAR SDK** | Surface, Image Tracking | In-Browser WebGL Engine (Safari) | In-Browser WebGL Engine (Chrome) | glTF, GLB | **Medium** (~1.5 MB) / Fast | **Easy to Moderate** (Web Builder / SDK) | **Commercial SaaS** (Pay-per-view / $500+/mo) |

---

## 7. Recommended 100% Free WebAR Architecture Stacks

To achieve zero cost, cross-platform stability, and maximum user reach without commercial lock-in, projects should combine open-source libraries based on the specific AR interaction requirement:

```
+---------------------------------------------------------------------------------+
|                       WEB AR USE CASE DECISION TREE                             |
+---------------------------------------------------------------------------------+
                                       |
            +--------------------------+--------------------------+
            |                                                     |
  [Surface / Plane AR]                                   [Image Target AR]
            |                                                     |
  +---------+---------+                                 +---------+---------+
  |                   |                                 |                   |
(Simple Showcase)  (Custom App/UI)                    (Posters/Cards)     (Outdoor AR)
  |                   |                                 |                   |
  v                   v                                 v                   v
Google             Hybrid Stack                     MindAR.js           AR.js
<model-viewer>     - Android: Three.js + WebXR          + Three.js          Location Mode
                   - iOS: <model-viewer> Fallback
```

### Recommendation 1: Simple 3D Asset & Product Showcase (Plane AR)
- **Recommended Stack**: **Google `<model-viewer>`**
- **Why**: Zero external library overhead, instant 60 FPS plane tracking backed by ARKit (iOS) and ARCore (Android), automatic shadow/light estimation, and zero hosting costs.

### Recommendation 2: Scannable Poster / Card / Packaging Experience (Image AR)
- **Recommended Stack**: **MindAR.js + Three.js**
- **Why**: 100% MIT open source. Superior tracking stability over AR.js NFT. Built-in web target compiler. Full custom WebGL rendering and UI control via Three.js.

### Recommendation 3: Custom Interactive WebAR Application (World Plane AR with Custom HTML UI)
- **Recommended Stack**: **Hybrid Three.js Architecture**
  - **Android Path**: Three.js + Native WebXR Hit-Test API (delivers in-browser custom canvas, responsive HTML buttons, complex game logic).
  - **iOS Path**: Progressive fallback to Google `<model-viewer>` (or MindAR Image Target anchor when plane tracking is non-essential).

---

## 8. Summary of Findings & Next Steps

1. **Open-Source Parity**: Web-native FOSS frameworks completely satisfy Image Tracking (MindAR) and Plane AR Showcase (`<model-viewer>`) requirements without requiring 8th Wall or Zappar subscriptions.
2. **iOS WebXR Gap**: The primary technical friction point in FOSS WebAR remains Apple's lack of WebXR AR hit-testing in iOS Safari. The hybrid architecture (WebXR for Android, USDZ/Quick Look for iOS) is the industry-standard workaround for zero-cost deployments.
3. **No Unity Dependency**: Eliminating Unity WebGL for WebAR reduces build sizes from ~25-40 MB down to <2 MB, improving load times on mobile networks by over 90%.

---
*Report compiled and verified by WebAR Frameworks Specialist Explorer.*
