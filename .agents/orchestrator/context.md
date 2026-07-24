# Domain Technical Context: ViMARA AR/WebAR Research

## Core Objectives
1. Compare AR Foundation vs Vuforia (2023-2024 versions) for native mobile apps in Unity:
   - Setup complexity
   - Plane tracking (ARKit/ARCore abstraction vs Smart Terrain/VisLAM)
   - Image tracking marker stability & target database handling
   - Dynamic 3D model importing at runtime (glTF/GLB, Addressables, TriLib, etc.)
   - Licensing & cost breakdown (Zero-cost mandate: Vuforia watermark/pricing vs AR Foundation free model)

2. Evaluate WebAR Viability:
   - Mobile browser support (iOS Safari vs Android Chrome)
   - WebXR API adoption & limitations
   - Unity WebGL AR exports (Unity WebXR plugin, Needle Engine, Zappar WebGL)
   - Performance, bundle size, initial load times, memory constraints on mobile

3. Evaluate Non-Unity WebAR Frameworks:
   - `<model-viewer>` (Google)
   - MindAR.js (Open source Image/Face tracking)
   - AR.js (Marker, NFT, Location)
   - Three.js + WebXR API
   - Commercial alternatives (8th Wall, Zappar, Blippar) for cost comparison
   - Detailed Pros/Cons/Cost matrix highlighting 100% free/open-source options

4. Formulate Final Architectural Recommendation respecting 100% free / zero-cost constraint.
