# R1 Export Analysis: 3D Modeling & CAD/BIM Software Export Capabilities

**Author:** `explorer_3d_r1`  
**Project:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Date:** July 24, 2026  
**Status:** Completed Investigation  

---

## Executive Summary & Comparison Matrix

This investigation provides a comprehensive evaluation of 3D export formats across six major CAD, BIM, and 3D modeling software packages: **SketchUp Free (Web)**, **SketchUp Pro (Desktop)**, **AutoCAD**, **Revit**, **Blender**, and **Rhino (Rhinoceros 3D)**. 

Special focus is given to **glTF/GLB** (the web and AR standard required for ViMARA's WebAR pipeline), **STL**, **OBJ**, **FBX**, **DAE**, **DWG/DXF**, **USDZ**, and **IFC**, alongside plugin dependencies and license/version constraints.

### 3D Export Compatibility Matrix

| Software / Platform | glTF / GLB | STL | OBJ (+MTL) | FBX | DAE (Collada) | DWG / DXF | USDZ | IFC (BIM) | Native Extension API |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: |
| **SketchUp Free (Web)** | ❌ (No) | ✅ (Native) | ❌ (No) | ❌ (No) | ❌ (No) | ❌ (No) | ❌ (No) | ❌ (No) | ❌ Locked (No Plugins) |
| **SketchUp Pro (Desktop)**| 🔌 (Plugin) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native v2023+) | ✅ (Native) | ✅ Full Ruby API (`.rbz`) |
| **AutoCAD (Desktop)** | 🔌 (Plugin) | ✅ (Native) | ✅ (Native v2023+) | ⚠️ (v2011-2018 native; deprecated v2019+) | 🔌 (Plugin) | ✅ (Native) | ❌ (No) | 🔌 (Plugin/Architecture) | ✅ AutoLISP / ObjectARX (Full only) |
| **Revit (BIM Desktop)** | 🔌 (Plugin) | 🔌 (Plugin/Add-in) | ✅ (Native v2023+) | ✅ (Native) | 🔌 (Plugin) | ✅ (Native) | ❌ (No) | ✅ (Native openBIM) | ✅ C# .NET API (Full only) |
| **Blender (3D Suite)** | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | 🔌 (Plugin) | ✅ (Native v3.0+) | 🔌 (BlenderBIM) | ✅ Full Python API |
| **Rhino 7 (CAD/NURBS)** | 🔌 (Plugin) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | 🔌 (Plugin) | 🔌 (VisualARQ) | ✅ C# / Python / C++ |
| **Rhino 8 (CAD/NURBS)** | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | ✅ (Native) | 🔌 (VisualARQ) | ✅ C# / Python / C++ |

*Legend: ✅ Native Out-of-the-Box | 🔌 Plugin/Extension Required | ⚠️ Deprecated/Version Dependent | ❌ Not Supported*

---

## Exhaustive Software Export Profiles

---

### 1. SketchUp Free (Web)

#### Overview
SketchUp Free is Trimble’s browser-based version of SketchUp running on WebGL within standard web browsers. It provides core 3D modeling tools for personal and hobbyist use without software installation.

#### Native 3D Export Formats (Out-of-the-Box)
- **STL** (*Stereolithography*): Native 3D geometry export out-of-the-box (primarily for 3D printing).
- **SKP** (*SketchUp Project File*): Saved directly to Trimble Connect cloud storage or downloaded locally as a `.skp` file.
- **PNG** (*2D Raster Image*): Native 2D viewport rendering export.
- **3D Warehouse Downloads**: Models published to or fetched from 3D Warehouse can be downloaded in `.skp` or `.dae` (Collada) formats depending on asset availability.

#### Plugin-Required Export Formats & Web Restrictions
- **No Ruby Extension Support**: SketchUp Free runs inside a browser sandbox and **does NOT support Ruby extensions or plugins** (`.rbz` files cannot be installed).
- **glTF / GLB Export**: **NOT AVAILABLE** in SketchUp Free (Web). Because third-party extensions cannot be installed, users cannot use plugins like Centaur glTF Exporter or SimLab glTF Exporter.
- **Unavailable 3D Formats**: DAE, OBJ, FBX, DWG, DXF, 3DS, VRML, IFC, USDZ are all unavailable for export in the free web tier.

#### License & Version Restrictions
- **Free Web Tier**: Restricted strictly to STL, SKP, and 2D PNG.
- **Paid Web Tiers (SketchUp Go / Shop)**: Unlocks web exports for DWG, DXF, OBJ, FBX, DAE, 3DS, and STL, but **still lacks full Ruby extension support**.
- **Impact on ViMARA**: Architecture students using SketchUp Free **cannot export directly to .glb**. They must either download `.skp` / `.stl` and convert via a desktop converter (e.g. Blender) or upgrade to SketchUp Pro Desktop with a glTF exporter extension.

---

### 2. SketchUp Pro (Desktop)

#### Overview
SketchUp Pro is the desktop 3D modeling application for Windows and macOS widely used by architects, interior designers, and urban planners. It features full Ruby API extensibility via the Extension Warehouse.

#### Native 3D Export Formats (Out-of-the-Box)
- **DAE** (*Collada 1.4/1.5*): Native open 3D interchange format with material and texture preservation.
- **KMZ** (*Keyhole Markup Language Zipped*): Native 3D format for Google Earth geo-location visualization.
- **3DS** (*3D Studio Legacy*): Native mesh and material export for legacy Autodesk software.
- **DWG** (*AutoCAD Drawing 3D*): Native 3D wireframe, surface, and solid CAD entity export.
- **DXF** (*Drawing Interchange Format 3D*): Open CAD interchange format for 3D geometry.
- **FBX** (*Autodesk Filmbox*): Native 3D format with mesh, hierarchy, materials, and camera support.
- **OBJ** (*Wavefront 3D Object + MTL*): Native 3D mesh with material definitions and UV maps.
- **VRML** (*WRL - Virtual Reality Modeling Language*): Legacy 3D web format (v2.0 / VRML97).
- **XSI** (*Softimage XSI*): Legacy 3D animation interchange format.
- **STL** (*Stereolithography*): Native 3D export since SketchUp 2018 (previously required an extension).
- **IFC** (*Industry Foundation Classes 2x3 & IFC4*): Native openBIM building entity export with classification tags.
- **USDZ / USDA** (*Universal Scene Description*): Added natively in SketchUp 2023 / 2024 for Apple AR Quick Look integration.
- **DEM** (*Digital Elevation Model*): Terrain elevation surface data export.

#### Plugin-Required Export Formats
- **glTF 2.0 / GLB**: **Requires Plugin**. SketchUp Pro does not natively export glTF/GLB out of the box in stock installations.
  - **Centaur glTF Exporter (Khronos Group)**: Open-source Ruby plugin (`.rbz`) available on GitHub and Extension Warehouse. Exports `.gltf` and `.glb` with PBR material mappings.
  - **SimLab glTF Exporter for SketchUp**: Commercial extension offering advanced GLB compression, PBR texture mapping, LOD generation, and animation baking.
  - **Universal Importer / Exporter**: Community extensions enabling glTF, STEP, and IGES export.
- **STEP / IGES / CAD Formats**: Require commercial extensions (e.g., SimLab STEP Exporter for SketchUp).

#### License & Version Restrictions
- **Subscription Required**: Requires an active SketchUp Pro or Studio subscription.
- **Extension API**: Desktop Pro fully supports `.rbz` extensions via Ruby API (`Sketchup::Extension`).
- **SketchUp Make 2017 (Legacy Free Desktop)**: Limited to DAE, KMZ, and STL (via plugin). Pro formats (DWG, DXF, FBX, OBJ, 3DS, IFC) were locked behind a 30-day trial.

---

### 3. AutoCAD (Autodesk)

#### Overview
AutoCAD is Autodesk's industry-standard 2D/3D Computer-Aided Design (CAD) drafting software used across architecture, engineering, and construction (AEC).

#### Native 3D Export Formats (Out-of-the-Box)
- **DWG** (*Native AutoCAD Drawing 3D*): Primary native format storing 3D ACIS solids, surfaces, and meshes.
- **DXF** (*Drawing Exchange Format 3D*): Open text/binary CAD specification for 3D geometry exchange.
- **SAT** (*ACIS 3D Solid Geometry* via `ACISOUT` / `EXPORT`): Standard boundary representation (B-Rep) solid geometry format.
- **STL** (*Stereolithography* via `STLOUT`): Native 3D mesh export for rapid prototyping and 3D printing.
- **OBJ** (*Wavefront 3D* via `OBJEXPORT`): **Natively added in AutoCAD 2023 & 2024+** for direct mesh export with materials.
- **IGES / IGS** (*Initial Graphics Exchange Specification* via `IGESEXPORT`): Standard format for 3D surface NURBS models.
- **STEP / STP** (*ISO 10303 Product Data Standard* via `STEPEXPORT`): Added in recent AutoCAD releases for solid model exchange.
- **3DDWF / DWFx** (*Design Web Format 3D*): Autodesk native lightweight 3D viewing format.

#### Plugin-Required Export Formats & Deprecated Commands
- **glTF 2.0 / GLB**: **Requires Plugin**. AutoCAD has NO native glTF/GLB export capability.
  - **SimLab glTF Exporter for AutoCAD**: Commercial plugin adding direct GLB/glTF export with texture and material retention.
  - **ProtoTech glTF Exporter for AutoCAD**: Third-party plugin supporting GLB generation from 3D solids and surfaces.
- **FBX (Autodesk Filmbox)**: **Version Dependent / Deprecated**.
  - AutoCAD 2011 through AutoCAD 2018 featured native `FBXEXPORT`.
  - **Autodesk removed `FBXEXPORT` in AutoCAD 2019+** due to outdated FBX SDK dependencies. Exporting to FBX in AutoCAD 2019+ requires third-party utilities or exporting to DWG/SAT first, then importing into 3ds Max/Navisworks.
- **3DS (3D Studio)**: `3DSOUT` was removed in AutoCAD 2007+; requires an official Autodesk download utility plugin for legacy versions.
- **DAE (Collada)**: Requires third-party plugins or conversion via FBX/3ds Max.

#### License & Version Restrictions
- **AutoCAD Full vs AutoCAD LT**:
  - **AutoCAD LT (Lite)**: Restricted strictly to 2D drafting. It **does NOT support 3D solid modeling, 3D export commands (`STLOUT`, `OBJEXPORT`, `ACISOUT`), or LISP/ARX API plugins**.
- **Commercial License**: Full AutoCAD desktop commercial license required for 3D modeling and export.

---

### 4. Revit (Autodesk BIM)

#### Overview
Autodesk Revit is the leading Building Information Modeling (BIM) software platform for architectural design, structural engineering, MEP engineering, and construction management.

#### Native 3D Export Formats (Out-of-the-Box)
- **RVT** (*Native Revit Project*): Saved directly.
- **IFC** (*Industry Foundation Classes 2x3, IFC4, IFC4.3*): Native openBIM standard exporter for building geometry, spatial hierarchies, and parametric BIM data.
- **DWG** (*AutoCAD 3D Solids / Mesh*): Native export of 3D geometry as ACIS solids, polymesh, or polyface mesh.
- **DXF** (*Drawing Exchange Format 3D*): Native 3D geometry export.
- **FBX** (*Autodesk Filmbox 3D*): Native 3D export containing full element geometry, materials, textures, scene lighting, and camera positions.
- **DGN** (*Bentley MicroStation 3D*): Native export for V8 DGN files.
- **DWF / DWFx** (*Design Web Format 3D*): Native 3D visual and metadata export.
- **SAT** (*ACIS 3D Solid Geometry*): Native B-Rep geometry export.
- **OBJ** (*Wavefront 3D*): **Natively added in Revit 2023 / 2024+**; older versions required FBX conversion or plugins.
- **NWC** (*Navisworks Cache*): Native export when Navisworks Exporter utility is installed.

#### Plugin-Required Export Formats
- **glTF 2.0 / GLB**: **Requires Plugin**. Revit does NOT include a native glTF/GLB exporter out of the box.
  - **SimLab glTF Exporter for Revit**: Widely used commercial plugin exporting GLB/glTF with PBR materials, custom LOD control, and BIM parameter preservation.
  - **SunBurn / Virtual-Surreal glTF Exporter**: Open-source / commercial add-in for Revit.
  - **ProtoTech glTF Exporter for Revit**: Commercial plugin for GLB/glTF generation.
  - **Autodesk Platform Services (APS / Forge)**: Cloud API pipeline converting RVT files to SVF/glTF asynchronously.
- **STL** (*Stereolithography*): Requires the free official **Autodesk Revit STL Exporter** add-in (available on Autodesk App Store / built into recent 3D printing menus).
- **DAE (Collada)**: Requires plugins (e.g., Lumion LiveSync plugin for Revit, SimLab Collada Exporter, or FBX-to-DAE conversion).

#### License & Version Restrictions
- **Revit Full vs Revit LT**:
  - **Revit LT**: Lacks API support for third-party C#/.NET add-ins. **Third-party glTF and STL plugins CANNOT be installed on Revit LT**.
- **Commercial / Educational License**: Full Revit installation required for extension development and full export API.

---

### 5. Blender (3D Creation Suite)

#### Overview
Blender is a free, open-source 3D creation suite supporting modeling, rigging, animation, simulation, rendering, compositing, motion tracking, and video editing.

#### Native 3D Export Formats (Out-of-the-Box)
- **glTF 2.0 / GLB / glTF Embedded** (`.glb`, `.gltf`): **Fully Native Out-of-the-Box**. Built-in exporter (`io_scene_gltf2`) co-developed by the Khronos Group and Blender Foundation. Supports PBR Principled BSDF materials, skeletal animation, morph targets, mesh instances, and Draco geometry compression.
- **OBJ / MTL** (*Wavefront 3D Object*): Fully native; rewritten in C++ in Blender 3.x+ for high performance.
- **FBX** (*Autodesk Filmbox*): Fully native Python exporter supporting mesh, armatures, animations, materials, and custom transforms.
- **STL** (*Stereolithography*): Fully native C++/Python exporter for 3D printing.
- **DAE** (*Collada 1.4/1.5*): Fully native C++ Collada module built into Blender core.
- **PLY** (*Stanford Triangle Format*): Fully native mesh export with vertex color support.
- **USD / USDA / USDC / USDZ** (*Universal Scene Description*): Natively built-in since Blender 3.0+ / 3.5+, supporting direct `.usdz` export for Apple iOS AR Quick Look.
- **Blend** (*Native Blender File*): Saved directly.
- **ABC** (*Alembic Animation Geometry*): Built-in open format for complex animation caches.
- **X3D / VRML2**: Built-in standard web 3D format exporter.
- **BVH** (*Biovision Hierarchy*): Built-in motion capture skeleton exporter.

#### Plugin-Required Export Formats
- Blender natively covers almost all standard 3D web, game, and graphics formats. Third-party add-ons are only required for specialized proprietary CAD formats:
  - **STEP / IGES**: Requires community add-ons (e.g. *CAD Sketcher* or *IO STEP*).
  - **DWG**: Requires community add-ons utilizing ODA/Teigha file converters.
  - **IFC**: Supported via the open-source **BlenderBIM** add-on (turns Blender into a full openBIM authoring tool).

#### License & Version Restrictions
- **GNU General Public License (GPL v3)**: 100% free and open-source software for commercial, educational, and personal use.
- **Cross-Platform**: Complete feature parity across Windows, macOS, and Linux without licensing constraints.
- **Ideal Role for ViMARA**: Blender serves as the premier open-source conversion bridge for ViMARA (e.g., converting SketchUp `.skp`, `.stl`, or Revit `.fbx` models into web-optimized `.glb` files).

---

### 6. Rhino (Rhinoceros 3D)

#### Overview
Rhino (by Robert McNeel & Associates) is a 3D computer graphics and CAD software application based on the NURBS (Non-Uniform Rational B-Splines) mathematical model, widely used in industrial design, architecture, and computational design (Grasshopper).

#### Native 3D Export Formats (Out-of-the-Box)
- **3DM** (*Native Rhino Geometry*): Direct native save.
- **OBJ / MTL** (*Wavefront 3D Object*): Native exporter with advanced mesh density controls and material mapping.
- **FBX** (*Autodesk Filmbox*): Native mesh, material, and hierarchy export.
- **DAE** (*Collada*): Native 3D open format export.
- **STL** (*Stereolithography*): Native binary and ASCII STL export with mesh tolerance adjustments.
- **DWG / DXF** (*AutoCAD 2D/3D*): Native export supporting both 2D curves and 3D NURBS/mesh entities.
- **glTF / GLB**: **NATIVELY BUILT INTO RHINO 8!** Rhino 8 includes direct out-of-the-box export to `.glb` and `.gltf` with PBR material mapping.
- **USD / USDZ**: **Natively built into Rhino 8** for Apple AR integration.
- **SKP** (*SketchUp File*): Rhino **natively exports to SketchUp `.skp` format** out of the box.
- **3DS** (*3D Studio*): Native legacy mesh export.
- **IGES / IGS**: Native CAD NURBS surface exchange format.
- **STEP / STP**: Native B-Rep CAD solid exchange format.
- **SAT** (*ACIS Solid*): Native 3D CAD solid export.
- **PLY**, **VRML (WRL)**, **Parasolid (`.x_t`)**, **RAW**, **POV**, **SLDPRT** (*SolidWorks Parts*): Native exports.

#### Plugin-Required Export Formats
- **glTF / GLB in Rhino 7 & Earlier**: Required installing the official McNeel plugin `glTF-BinExporter` / `ImportExportGlTF` via the built-in package manager command `PackageManager` or Food4Rhino.
- **IFC (BIM)**: Requires **VisualARQ** (architectural BIM plugin for Rhino) or Grasshopper BIM workflow tools.

#### License & Version Restrictions
- **Commercial / Educational License**: Requires paid license or 90-day fully functional trial.
- **Rhino 7 vs Rhino 8**: Rhino 8 is a major upgrade adding native glTF/GLB and USDZ export out-of-the-box without extra plugins.

---

## Synthesis & ViMARA Pipeline Recommendations

### Primary Format Recommendation: glTF / GLB
For ViMARA's WebAR architecture (`<model-viewer>` and `Three.js` / `MindAR.js`), **GLB (`.glb`)** is the single required 3D format due to:
1. **Compact Binary Packaging**: Self-contained single file containing geometry, PBR textures, materials, and animations.
2. **Web Native Standard**: Designed by Khronos Group specifically for rapid WebGL transmission and mobile AR parsing.
3. **Native AR Support**: Directly rendered by `<model-viewer>` (Android Scene Viewer and iOS AR Quick Look fallback).

---

### Software Export Paths for ViMARA Users

```
+---------------------------------------------------------------------------------------------------+
|                               ViMARA 3D EXPORT WORKFLOW PATHS                                     |
+---------------------------------------------------------------------------------------------------+

 1. SKETCHUP FREE (WEB)
    [SketchUp Free (Web)] ---> Exports STL / SKP ---> [Blender / Converter] ---> Exports GLB ---> [ViMARA WebAR]
    (No direct GLB export; plugins locked)

 2. SKETCHUP PRO (DESKTOP)
    [SketchUp Pro Desktop] --+---> With Extension (Centaur / SimLab) ---------> Direct GLB  ---> [ViMARA WebAR]
                           +---> Native DAE / OBJ / FBX -> [Blender] ---------> Exports GLB  ---> [ViMARA WebAR]

 3. BLENDER
    [Blender 3.x / 4.x] ------------------------------------------------------> Native GLB  ---> [ViMARA WebAR]

 4. RHINO 8
    [Rhino 8 Desktop] --------------------------------------------------------> Native GLB  ---> [ViMARA WebAR]

 5. AUTOCAD / REVIT / RHINO 7
    [AutoCAD / Revit / Rhino 7] +---> With Extension (SimLab / ProtoTech) ------> Direct GLB  ---> [ViMARA WebAR]
                                +---> Native FBX / OBJ -> [Blender] -----------> Exports GLB  ---> [ViMARA WebAR]
```

### Summary of Actionable Strategies for ViMARA:
1. **For SketchUp Free Users**: Instruct students to download their model as `.stl` (or `.skp`) and process it through an automated server-side or web assembly converter (or free desktop Blender) to generate `.glb`.
2. **For SketchUp Pro Desktop Users**: Recommend installing the free open-source **Centaur glTF Exporter** (`.rbz`) plugin for direct 1-click `.glb` export.
3. **For Revit & AutoCAD Users**: Recommend exporting to native `FBX` or `OBJ` (Revit 2023+ / AutoCAD 2023+) and using Blender for automated batch GLB conversion, OR installing commercial plugins (SimLab/ProtoTech) if direct export is required.
4. **For Blender & Rhino 8 Users**: Provide direct `.glb` export guides, as both software packages support native `.glb` export out-of-the-box.

---

## Verification & Sources
- **SketchUp Official Documentation**: Extension Warehouse, SketchUp Web vs Desktop Export Specifications (2024).
- **Autodesk Knowledge Network**: AutoCAD 2023/2024 Export Commands (`OBJEXPORT`, `STLOUT`, `FBXEXPORT` deprecation notes), Revit 2023/2024 Native Exporters.
- **Blender Manual**: Blender 3.6 / 4.x `io_scene_gltf2` documentation (Khronos Group glTF 2.0 specification).
- **Rhino Documentation**: McNeel Rhino 8 Release Notes (Native glTF/GLB and USDZ export support).
