# Project: ViMARA 3D File Format Standards Analysis

## Mission
Investigate 3D modeling software export capabilities (SketchUp, AutoCAD, Revit, Blender, Rhino) and conduct technical comparison of 3D formats (.glb/.gltf, .stl, .obj, .fbx, .dae, etc.) to define a curated list of 2-4 official 3D file formats for the ViMARA WebAR platform. Produce `3D_File_Format_Standards_ViMARA.md` in the project root.

## Milestones
| # | Name | Scope | Dependencies | Status |
|---|------|-------|-------------|--------|
| 1 | R1 Export Analysis | Export options in SketchUp Free Web & Pro Desktop + AutoCAD, Revit, Blender, Rhino. Identify native formats vs plugins required. | None | DONE |
| 2 | R2 Technical Comparison | Technical analysis of formats (.glb/.gltf, .stl, .obj, .fbx, .dae, etc.) for WebAR: Phase 1 (untextured geometry size/speed), Phase 2 (PBR & complex materials), WebAR support (Three.js & Google <model-viewer>). | None | DONE |
| 3 | R3 Format Selection & Report Drafting | Recommend 2-4 official formats for ViMARA with complete technical justification balancing user workflow compatibility vs WebAR optimization. Draft `3D_File_Format_Standards_ViMARA.md` in project root. | M1, M2 | DONE |
| 4 | Verification & Audit | Independent review and forensic audit of `3D_File_Format_Standards_ViMARA.md` against acceptance criteria. | M3 | DONE (CLEAN) |

## Deliverable Specification
- Output path: `c:\Users\manut\Documentos\UNIVERSIDAD\BENTRE25\ViMARA\3D_File_Format_Standards_ViMARA.md` (GENERATED & VERIFIED)
- Verified Contents:
  1. Software Export Analysis (SketchUp Free Web, SketchUp Pro Desktop, AutoCAD, Revit, Blender, Rhino 7 & 8) - native formats vs plugins needed and licensing constraints.
  2. Format Technical Comparison (.glb/.gltf, .stl, .obj/.mtl, .fbx, .dae, .usdz):
     - Phase 1: Base geometry file size, vertex/face overhead ($6V$ vs $V$), mobile browser parsing speed (ArrayBuffer zero-copy vs text regex), Draco & `KHR_mesh_quantization`.
     - Phase 2: PBR textures (Cook-Torrance BRDF GGX vs legacy Phong), material definitions, GPU texture supercompression (`KHR_texture_basisu` KTX2 VRAM savings from 16.77MB to 2.1MB), scene graph, animations.
     - WebAR Compatibility: Three.js loaders (`GLTFLoader`, `OBJLoader`, `STLLoader`, `FBXLoader`, `ColladaLoader`), Google `<model-viewer>` native support, iOS Quick Look (`.usdz`) / Android Scene Viewer (`.glb`) protocols.
  3. Conversion & Pipeline Architecture: $0-cost client-side WebAssembly conversion pipeline (`ViMARAModelPipeline` JS class with `GLTFExporter`, `DRACOLoader`, `USDZExporter`, architectural clay shader, and automatic unit scaling).
  4. Final Curated Format Selection (Tier 1: `.glb`/`.gltf`, Tier 2: `.obj`/`.stl`, Tier 3: `.usdz`) with complete technical justification matrix and 6 step-by-step export guides.
