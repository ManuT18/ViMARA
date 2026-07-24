# Handoff Report: R3 Curated 3D Format Selection & Architecture

**Author**: explorer_3d_r3  
**Date**: 2026-07-24  
**Working Directory**: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_3d_r3`  
**Target Recipient**: Parent / Orchestrator (`9b9adbef-3691-450d-bad3-c5ce7acf37ef`)

---

## 1. Observation

Direct observations from workspace files and project environment:

1. **Project Scope and Zero-Cost Constraint (`AIContext.md`, Lines 4–15, 18–22)**:
   - "ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es un proyecto universitario (beca BENTRE25). El objetivo es visualizar maquetas de arquitectura, exportadas desde software como SketchUp..."
   - "Se priorizará .glb (estándar óptimo para Web y AR) y se evaluará el soporte para .stl (común en geometría sin textura)."
   - "Ruta Recomendada (WebAR): Frontend: HTML, CSS, JavaScript... Motor AR: `<model-viewer>` para Plane Tracking nativo... y MindAR.js + Three.js para Image Tracking."

2. **WebAR Framework Landscape (`.agents/teamwork_preview_explorer_r3_webar_frameworks/analysis.md`, Lines 20–55)**:
   - Google `<model-viewer>` native support is glTF 2.0 / GLB on Android WebXR and Apple USDZ on iOS AR Quick Look (`ios-src="model.usdz"`).
   - Three.js rendering pipeline relies on `GLTFLoader`, `OBJLoader`, `STLLoader`, and `GLTFExporter` for web 3D model parsing and export.

3. **CAD Software Ecosystem**:
   - Architectural software tools used by students: SketchUp (Pro / Free), AutoCAD, Revit, Rhino 7/8, Blender.
   - Native export capabilities vary: Blender and Rhino 8 support native `.glb` export; SketchUp Pro requires plugins or export to `.obj`/`.stl`; AutoCAD exports 3D solids to `.stl`/`.obj`; Revit requires plugins or `.obj`/`.fbx` bridges.

4. **Web Performance & Memory Constraints**:
   - Mobile Safari on iOS imposes a page memory threshold (~1.4 GB total tab memory) causing browser reloads if WASM/WebGL memory footprint spikes.
   - Text-based ASCII formats like `.obj` (e.g. 50 MB) parse slowly in JavaScript CPU threads, whereas binary `.glb` with Draco compression reduces transmission size by 70–90% and uploads directly to GPU vertex buffer objects (VBOs).

---

## 2. Logic Chain

1. **From Observation 1 & 2**: ViMARA targets mobile web browsers (Android Chrome and iOS Safari) with zero licensing costs ($0 requirement). Since `<model-viewer>` and Three.js natively accept `.glb`/`.gltf` as their core standard, `.glb` must be designated as the **Tier 1 Primary Native WebAR Format**.
2. **From Observation 3**: Many architecture students use basic software (e.g., SketchUp Free, base AutoCAD) without native `.glb` exporters. Forcing all students to purchase plugins or use external converters creates adoption friction. Therefore, ViMARA must support **Tier 2 Secondary Import Formats (`.obj` + `.mtl` and `.stl`)**, covering legacy CAD export and physical 3D printing maquetas ("maqueta blanca").
3. **From Observation 1 & 2**: iOS Safari native AR Quick Look requires Apple USDZ. To avoid requiring students to export USDZ on Windows, ViMARA must incorporate an **on-the-fly client-side `USDZExporter` (Tier 3 iOS Fallback)**.
4. **From Observation 1 & 4**: To respect the zero-cost constraint and prevent server hosting bills while safeguarding student privacy, the conversion of `.obj`/`.stl` to `.glb` must occur **client-side in the browser** using Three.js loaders (`OBJLoader`, `STLLoader`), `GLTFExporter`, and WebAssembly Draco compression (`draco_encoder.wasm`).
5. **From Observation 4**: Heavy geometry (stairs, structural frames) in architectural maquetas can be compressed via Draco (14-bit position quantization) to achieve 70–90% reduction in file size with 1mm physical accuracy, staying well within mobile RAM and bandwidth limits.

---

## 3. Caveats

1. **Extreme Mesh Polycount Limits**: While client-side WebAssembly conversion handles models up to ~50 MB (~1M polygons), extremely unoptimized CAD files (>2M polygons, unmerged vertices) may freeze the mobile browser main thread during parsing. For these rare cases, a server-side microservice fallback is outlined in `analysis_r3.md`, but not implemented in client-side code.
2. **SketchUp Texture Path References**: When exporting `.obj` + `.mtl` from SketchUp, texture image paths inside `.mtl` must be uploaded alongside the `.obj` file via multi-file selection (`<input type="file" multiple>`) or a zip file picker to embed textures into the converted `.glb`.
3. **BIM Semantics Excluded**: `.ifc` format was evaluated and intentionally excluded from the core tier due to heavy CSG boolean geometry processing overhead, but recommended for future server-side BIM expansion.

---

## 4. Conclusion

ViMARA should officially adopt the following curated 3D format tier structure and conversion architecture:

- **Tier 1 (Primary Native WebAR Standard)**: `.glb` / `.gltf` (glTF 2.0 Binary container with Draco compression).
- **Tier 2 (Secondary Import Formats)**: `.obj` (+ `.mtl`) for universal CAD geometry and `.stl` for 3D printed / volumetric white maquetas ("maqueta blanca").
- **Tier 3 (iOS AR Fallback Container)**: `.usdz` dynamically generated on-the-fly via Three.js `USDZExporter`.
- **Conversion Architecture**: 100% Client-side in-browser conversion using Three.js loaders + `GLTFExporter` + WebAssembly Draco encoder (`draco_encoder.wasm`), ensuring $0 infrastructure cost, offline capability, instant local preview, and complete user privacy.

---

## 5. Verification Method

To independently verify the findings and architectural designs in this report:

1. **Inspect Report Document**:
   - File: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\.agents\explorer_3d_r3\analysis_r3.md`
   - Verify all 5 core sections: Executive Summary, Tier Structure, CAD Export Compatibility, Pipeline Architecture (with code snippets and ASCII diagrams), and Technical Justification Matrix.
2. **Validate Code Snippets & Library Imports**:
   - Confirm Three.js loader modules (`GLTFLoader`, `OBJLoader`, `STLLoader`, `GLTFExporter`, `USDZExporter`, `DRACOLoader`) align with standard npm package `three@^0.160.0`.
3. **Invalidation Conditions**:
   - The client-side zero-cost recommendation would be invalidated if mobile browsers completely disallow WebAssembly execution or if iOS Safari removes USDZ Quick Look support (both highly improbable given web standards).

---
