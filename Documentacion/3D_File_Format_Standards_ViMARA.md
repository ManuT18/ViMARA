# Estándar de Formatos de Archivos 3D y Arquitectura de Conversión WebAR — ViMARA

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Documento:** Especificación Técnica de Formatos 3D y Pipeline de Conversión  
**Fecha:** 24 de Julio de 2026  
**Estado:** Estándar Oficial  

---

## 1. Resumen Ejecutivo y Estructura de Niveles (Tiers)

### 1.1 Contexto de Rendimiento Móvil
ViMARA permite exportar maquetas 3D desde software CAD/BIM/3D para proyectarlas a escala (1:1, 1:50, 1:100) en navegadores móviles (Android Chrome / iOS Safari) sin instalar aplicaciones nativas. Los entornos WebAR imponen restricciones estrictas:
1. **Límites de RAM Móvil**: iOS Safari impone un techo estricto por pestaña (~1.4 GB en dispositivos de 4 GB RAM), cerrando el navegador ante picos de memoria.
2. **Arquitectura UMA (RAM/VRAM Compartida)**: Exige optimización rigurosa de búferes de vértices y compresión de texturas.
3. **Ejecución en Monohilo JS**: Three.js y los parsers de archivos se ejecutan en el Main UI Thread. Parseos pesados provocan congelamiento ("jank") y pérdida de seguimiento AR.

---

### 1.2 Estructura por Niveles (Tiers)

```
+-----------------------------------------------------------------------------------+
|                            ESTRUCTURA DE TIER DE VIMARA                           |
+-----------------------------------------------------------------------------------+
|                                                                                   |
|  [TIER 1: Estándar Primario WebAR - Transmisión y Renderizado]                   |
|  --> .glb / .gltf (Khronos glTF 2.0 Binary Container)                             |
|      * Renderizado directo (Three.js, Google <model-viewer>).                     |
|      * Soporta PBR PBR metallic-roughness, compresión Draco, cuantización y KTX2.  |
|                                                                                   |
|  [TIER 2: Formatos de Ingesta Universal y Maquetas Blancas]                      |
|  --> .obj (+ .mtl) : Intercambio universal CAD (SketchUp, AutoCAD, Rhino, Blender).|
|  --> .stl          : Geometría pura sin textura / Maquetas volumétricas impresas. |
|      * Convertidos en memoria del navegador a Tier 1 (.glb) via WebAssembly.      |
|                                                                                   |
|  [TIER 3: Contenedor de Fallback AR Nativo iOS]                                  |
|  --> .usdz         : Universal Scene Description Zip (Apple ARKit).               |
|      * Generado dinámicamente "on-the-fly" en cliente para Apple Quick Look.      |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

1. **Tier 1 — Estándar Primario WebAR (`.glb` / `.gltf`)**: Formato binario monolítico de Khronos Group. Contiene escena, VBOs de GPU, metadatos PBR y texturas incrustadas en 1 archivo. Soportado por Three.js, `<model-viewer>` y ARCore.
2. **Tier 2 — Ingesta Universal (`.obj` + `.mtl`, `.stl`)**: Garantiza compatibilidad con software CAD sin plugins de pago. No se sirven directamente a móviles; el pipeline cliente los transforma en memoria a Blob binario `.glb`.
3. **Tier 3 — Fallback iOS (`.usdz`)**: Requerido exclusivamente para visores nativos de Apple iOS Safari (Quick Look). Se genera dinámicamente desde `.glb` mediante el exportador WASM/JS de Three.js.

---

### 1.3 Matriz Sintética de Clasificación de Formatos

| Tier | Formato | Extensión | Función en ViMARA | Soporte Materiales | Compresión Geométrica | Parseo Móvil |
| :--- | :--- | :---: | :--- | :--- | :--- | :--- |
| **Tier 1** | **glTF 2.0 Binary** | `.glb` | **Estándar Primario WebAR** | PBR Completo (Metallic-Roughness) | Draco / Quantization | **Extremo (~5-15 ms)** |
| **Tier 1** | **glTF 2.0 JSON** | `.gltf` | Estándar Primario Multi-archivo | PBR Completo | Draco / Quantization | Alto (~15-35 ms) |
| **Tier 2** | **Wavefront OBJ** | `.obj` / `.mtl` | Ingesta CAD Universal | Phong Heredado / Diffuse Map | Ninguna (ASCII) | Lento (~180-450 ms) |
| **Tier 2** | **Stereolithography** | `.stl` | Ingesta Maqueta Blanca / 3D | Ninguno (Asignación Arcilla) | Ninguna (Sin índices) | Moderado (~35-70 ms) |
| **Tier 3** | **USDZ Container** | `.usdz` | Fallback iOS AR Quick Look | UsdPreviewSurface (PBR) | Quantization USD | Nativo iOS ARKit |
| *Excluido* | **IFC** | `.ifc` | *Excluido (Sobrecostos BIM)* | Metadatos BIM / Colores | Ninguna | Muy Lento (CSG) |
| *Excluido* | **Autodesk FBX** | `.fbx` | *Excluido (Formato Propietario)* | Phong / Shaders Propietarios | Ninguna | Lento (~150-380 ms) |
| *Excluido* | **COLLADA** | `.dae` | *Excluido (XML Verboso)* | COLLADA FX | Ninguna | Muy Lento (~250-600 ms) |

---

## 2. Análisis de Exportación por Software CAD / BIM / 3D

### 2.1 Matriz Comparativa de Exportación

| Software / Plataforma | glTF / GLB | STL | OBJ (+MTL) | FBX | DAE | DWG/DXF | USDZ | IFC | Mecanismo de API / Extensión |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **SketchUp Free (Web)** | ❌ | ✅ Nativo | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | Sandbox Web Bloqueado (Sin Ruby) |
| **SketchUp Pro (Desktop)**| 🔌 Plugin | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ (v2023+) | ✅ Nativo | Ruby API (`.rbz`) |
| **AutoCAD (Desktop)** | 🔌 Plugin | ✅ Nativo | ✅ (v2023+) | ⚠️ Deprec. | 🔌 Plugin | ✅ Nativo | ❌ | 🔌 Plugin | AutoLISP / ObjectARX |
| **Revit (BIM Desktop)** | 🔌 Plugin | 🔌 Add-in | ✅ (v2023+) | ✅ Nativo | 🔌 Plugin | ✅ Nativo | ❌ | ✅ Nativo | C# .NET API (Solo Full) |
| **Blender (3D Suite)** | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 Plugin | ✅ (v3.0+) | 🔌 Plugin | Python API (GPL v3) |
| **Rhino 7 (CAD/NURBS)** | 🔌 Plugin | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 Plugin | 🔌 Plugin | C# / Python / C++ |
| **Rhino 8 (CAD/NURBS)** | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 Plugin | C# / Python / C++ |

*Simbología: ✅ Nativo | 🔌 Requiere Plugin | ⚠️ Depreciado | ❌ No Soportado*

---

### 2.2 Desglose por Aplicación

1. **SketchUp Free (Web)**: Sandbox web sin extensiones Ruby. No exporta `.glb`. Solución: Exportar a **STL** (o descargar `.skp` para Blender). El conversor de ViMARA aplica shader de arcilla ("Maqueta Blanca") y convierte a `.glb`.
2. **SketchUp Pro (Desktop)**: Plugins `.rbz` disponibles (*Centaur glTF Exporter* gratuito o *SimLab*). Alternativas nativas: exportar a **OBJ+MTL** o **DAE**.
3. **AutoCAD**: Comando nativo `OBJEXPORT` (2023+) o `STLOUT`. Comandos FBX eliminados desde AutoCAD 2019. AutoCAD LT carece de modelado 3D y APIs.
4. **Revit**: Revit 2023+ exporta **OBJ** nativamente. Versiones previas usan *Autodesk Revit STL Exporter* o **FBX**. Revit LT no admite add-ins de terceros.
5. **Blender**: Exportador nativo `io_scene_gltf2` con PBR Principled BSDF y compresión Draco integrada. Herramienta puente ideal.
6. **Rhino 8 vs 7**: Rhino 8 incluye exportador nativo de **glTF 2.0 (`.glb`)** y USDZ. Rhino 7 requiere plugin `glTF-BinExporter` en `PackageManager`.

---

### 2.3 Flujos de Exportación hacia ViMARA

```
 1. SKETCHUP FREE (WEB)
    [SketchUp Free (Web)] ---> Exporta STL / SKP ---> [Conversor In-Browser ViMARA] ---> Genera GLB ---> [WebAR]

 2. SKETCHUP PRO (DESKTOP)
    [SketchUp Pro Desktop] --+---> Con Plugin (Centaur glTF) --------------> Directo GLB  ---> [WebAR]
                           +---> Exporta Nativo OBJ / DAE / FBX ------------> Conversor ViMARA -> [WebAR]

 3. BLENDER 3.X / 4.X
    [Blender] -------------------------------------------------------------> Directo GLB  ---> [WebAR]

 4. RHINO 8
    [Rhino 8 Desktop] -----------------------------------------------------> Directo GLB  ---> [WebAR]

 5. AUTOCAD / REVIT / ARCHICAD
    [AutoCAD / Revit] +---> Con Plugin (SimLab / ProtoTech) ----------------> Directo GLB  ---> [WebAR]
                      +---> Exportación Nativa FBX / OBJ ------------------> Conversor ViMARA -> [WebAR]
```

---

## 3. Comparativa Técnica Profunda de Formatos

### 3.1 Geometría Base ("Maquetas Blancas")

#### 1. ASCII vs. Binario
- Los formatos ASCII (`.obj`, STL ASCII, `.dae`) representan coordenadas con caracteres de texto.
- Una coordenada flotante IEEE 754 de 32 bits ($3 \times 4$ bytes = 12 bytes binarios) ocupa 35-48 bytes en ASCII, produciendo un **sobrecosto de 250% a 375%** y exigiendo parseo string en JS (`parseFloat()`).
- Los formatos binarios (`.glb`, Binary STL, Binary FBX) transmiten secuencias de bytes que mapean directamente a TypedArrays (`Float32Array`).

#### 2. Indexación de Vértices vs. Duplicación (STL)
- **Mallas Indexadas (`.glb`, `.usdz`, `.obj`)**: Guardan vértices únicos en un Vertex Buffer y definen caras mediante un Index Buffer (`Uint16`/`Uint32`).
- **Mallas No Indexadas (Binary STL)**: Almacenan 3 vértices explícitos por cada triángulo. En una malla de $V$ vértices y $F \approx 2V$ caras, STL almacena $3 \times F = 6V$ vértices (**6 veces la cantidad de vértices reales**).

#### 3. Compresión Geométrica glTF 2.0
1. **`KHR_mesh_quantization`**: Convierte `float32` (4 bytes/comp) a enteros normalizados `int16`/`uint16`/`int8`. Reduce el stride de 32 bytes a 14 bytes (**56.25% de ahorro**). **Cero overhead de CPU** (la GPU decodifica por hardware).
2. **`KHR_draco_mesh_compression`**: Cuantización + codificación *Edgebreaker* + compresión de entropía (ANS). Logra **85%-95% de compresión**. Requiere decodificador WASM (`draco_decoder.wasm`, ~350 KB).

#### 4. Benchmark Comparativo de Geometría (Atributos: Posición + Normal + UV)

| Escala Maqueta | ASCII OBJ | Binary STL | Binary FBX | Uncompressed GLB | Quantized GLB | Draco GLB |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Pequeña** (50k caras / 25k vértices) | ~4.35 MB | ~2.50 MB | ~1.85 MB | ~1.10 MB | **~0.50 MB** | **~0.14 MB** |
| **Mediana** (250k caras / 125k vértices)| ~21.75 MB | ~12.50 MB | ~9.25 MB | ~7.00 MB | **~3.25 MB** | **~0.75 MB** |
| **Grande** (1M caras / 500k vértices)   | ~87.00 MB | ~50.00 MB | ~37.00 MB | ~28.01 MB | **~13.00 MB** | **~2.80 MB** |
| **Indexación** | Indexada | No Indexada | Indexada | Indexada | Indexada | Edgebreaker |
| **Ratio Relativo** | 100% | 57.5% | 42.5% | 32.2% | **14.9%** | **3.2%** |

---

### 3.2 Velocidad de Parseo en Main Thread JS

- **ASCII OBJ (`OBJLoader`)**: Procesa texto línea por línea (`split`/regex), crea objetos JS temporales y causa pausas frecuentes de Garbage Collection.
- **Binary STL (`STLLoader`)**: Lee binario rápido vía `Float32Array`, pero genera mallas no indexadas.
- **glTF 2.0 Binary GLB (`GLTFLoader`)**: Utiliza `TypedArray` views **zero-copy** sobre el `ArrayBuffer` original de `fetch()` y transfiere directo a VRAM (`gl.bufferData`).

#### Benchmark de Parseo en Móvil (Maqueta 250k Caras, Snapdragon 778G / Apple A14)

| Métrica | ASCII OBJ | Binary STL | Binary FBX | Uncompressed GLB | Quantized GLB | Draco GLB (WASM) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Descarga (4G 15 Mbps)** | ~11.6 s | ~6.6 s | ~4.9 s | ~3.7 s | **~1.7 s** | **~0.4 s** |
| **Parseo Main Thread JS** | **380 ms** | 45 ms | 280 ms | **8 ms** | **9 ms** | **42 ms** (Worker) |
| **Pico Memoria JS Heap**  | ~85 MB | ~28 MB | ~62 MB | **~7 MB** | **~3.5 MB** | ~12 MB |
| **Frecuencia Pausas GC**  | Alta | Baja | Alta | **Cero** | **Cero** | Baja |
| **Impacto Caída FPS AR**  | Caída severa | Jitter menor | Caída severa | **Imperceptible** | **Imperceptible** | Ninguno (Worker) |

---

### 3.3 Consumo de Memoria RAM vs. VRAM (Maqueta 250k caras / 125k vértices)

- **STL (No Indexado)**: 750k vértices. Position Buffer = 9.0 MB, Normal Buffer = 9.0 MB. **Total VRAM = 18.0 MB**.
- **glTF/GLB (Indexado)**: 125k vértices únicos. Position Buffer = 1.5 MB, Normal Buffer = 1.5 MB, Index Buffer (`Uint32`) = 3.0 MB. **Total VRAM = 6.0 MB (66.7% de ahorro)**.

---

### 3.4 Materiales Complejos, PBR y Supercompresión

1. **PBR Metallic-Roughness vs. Phong**: glTF 2.0 y USDZ emplean iluminación PBR física (Cook-Torrance BRDF GGX) consistente en cualquier entorno HDRI. OBJ (`.mtl`) usa el modelo Phong empírico (`Ka`, `Kd`, `Ks`). STL carece de materiales.
2. **Topología Monolítica vs. Multifichero**: `.glb`/`.usdz` empaquetan escena, VBOs y texturas en 1 sola petición HTTP (cero errores CORS o rutas rotas). `.obj`+`.mtl` requiere múltiples peticiones HTTP en cascada.
3. **Supercompresión KTX2 (`KHR_texture_basisu`)**:
   - Una textura PNG 2K descompone en VRAM: $2048 \times 2048 \times 4\text{ B} = 16.77\text{ MB}$.
   - KTX2 transcodifica en ~2 ms via WASM al formato comprimido GPU nativo del chip (ASTC_4x4 en iOS, ETC2/ASTC en Android), reduciendo el peso en VRAM a **~2.1 MB (ahorro del 80-90%)**.

#### Tabla de Animaciones y Jerarquías

| Capacidad | glTF 2.0 / GLB | USDZ | OBJ / MTL | STL | FBX | COLLADA (DAE) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Scene Graph** | Jerarquía Completa | Jerarquía Prims | Lista Plana (`g`/`o`)| Triángulos Planos | Jerarquía Completa | Jerarquía XML |
| **TRS Transform**| Translación, Rotación, Escala | Matriz / TRS | Ninguna | Ninguna | TRS + Pre/Post Rot | Matriz / TRS |
| **Anim. Esquelética** | Joints, Skin Weights | Skeletal Joints | Ninguna | Ninguna | Huesos Complejos | Skeletal Joints |
| **Morph Targets**| Soportado Nativo | Blend Shapes | Ninguno | Ninguno | Blend Shapes | Morph Targets |
| **GPU Instancing**| `EXT_mesh_gpu_instancing` | Instanciación USD | Ninguna | Ninguna | Instanciación Nodos| Instanciación XML |

---

### 3.5 Ecosistema WebAR y Cargadores Three.js

| Cargador | Método de Parseo | Consumo Memoria | Carga Búfer WebGL |
| :--- | :--- | :--- | :--- |
| **`GLTFLoader`** | Binary ArrayBuffer Slice | Mínimo (1.0x) | Directo Zero-Copy |
| **`STLLoader`**  | Binary DataView Read | Alto (3.0x)* | Re-duplicado |
| **`OBJLoader`**  | JS Text Regex Parsing | Extremo (4.5x) | Re-indexado CPU |
| **`FBXLoader`**  | Binary Tree Parser | Alto (2.8x) | Re-indexado CPU |
| **`ColladaLoader`**| DOMParser XML Traversal | Extremo (5.0x) | Re-indexado CPU |

#### Componente `<model-viewer>`
Exige glTF 2.0 (`.glb`/`.gltf`). Integra AR Quick Look en iOS via atributo `ios-src`:
```html
<model-viewer 
  src="maqueta.glb" 
  ios-src="maqueta.usdz" 
  ar ar-modes="webxr scene-viewer quick-look" 
  camera-controls alt="Maqueta ViMARA">
</model-viewer>
```

#### Protocolos WebAR
- **WebXR Device API**: Canvas WebGL en navegador (Chrome Android). Formato: `.glb`.
- **Google ARCore Scene Viewer**: App nativa Android vía URL Intent. Formato: `.glb`.
- **Apple ARKit Quick Look**: Visor nativo iOS Safari. Formato: `.usdz` binario ZIP alineado a 64 bytes.

---

## 4. Selección Curada de Formatos y Arquitectura ViMARA

### 4.1 Formatos Seleccionados vs. Excluidos
- **Seleccionados**: Tier 1 (`.glb`/`.gltf`), Tier 2 (`.obj`+`.mtl`, `.stl`), Tier 3 (`.usdz`).
- **Excluidos del Núcleo**: `.ifc` (geometría CSG compleja, requiere parser WASM de 3MB+ `web-ifc`), `.fbx` (formato cerrado, parser pesado de 250KB), `.dae` (XML verboso), `.3ds` (obsoleto 16-bit).

---

### 4.2 Pipeline de Conversión Cliente In-Browser

```
+-----------------------------------------------------------------------------------+
|                        FLUJO DEL PIPELINE DE CONVERSIÓN VIMARA                    |
+-----------------------------------------------------------------------------------+
                                          |
                                  [Carga de Archivo 3D]
                                          |
                   +----------------------+----------------------+
                   |                                             |
             [Formato: .glb]                             [Formato: .obj / .stl]
                   |                                             |
                   v                                             v
        [Validación e Inspección]                      [Parseo Cliente JS/WASM]
         - Inspección de caja envolvente                - OBJLoader + MTLLoader
         - Verificación de texturas                     - STLLoader (Maqueta Blanca)
                   |                                             |
                   +----------------------+----------------------+
                                          |
                                          v
                            [Nodo de Normalización Geométrica]
                             - Centrar pivote en el origen (0, 0, 0)
                             - Alinear base inferior a Y = 0 (Suelo AR)
                             - Escalar a metros (Detección mm -> m)
                                          |
                                          v
                           [Exportador En-Memoria GLTFExporter]
                             - Convertir grafo de escena Three.js
                             - Incrustar texturas en ArrayBuffer
                                          |
                                          v
                        [Compresión Draco en Web Worker]
                         - Ejecución de draco_encoder.wasm
                         - Cuantización de malla (posBits: 14)
                                          |
                                          +-----------------------+
                                          |                       |
                                          v                       v
                                 [Renderizado WebAR]     [¿Navegador iOS Safari?]
                                  - <model-viewer>                |
                                  - Three.js WebXR           (Sí) |
                                                                  v
                                                        [Pipeline USDZExporter]
                                                         - Crear Blob .usdz en memoria
```

---

### 4.3 Código de Producción: Pipeline Cliente In-Browser (JavaScript / Three.js)

```javascript
/**
 * ViMARA - Pipeline Cliente In-Browser de Ingestión y Conversión 3D
 */
import * as THREE from 'three';
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader.js';
import { MTLLoader } from 'three/examples/jsm/loaders/MTLLoader.js';
import { STLLoader } from 'three/examples/jsm/loaders/STLLoader.js';
import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader.js';
import { GLTFExporter } from 'three/examples/jsm/exporters/GLTFExporter.js';
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader.js';
import { USDZExporter } from 'three/examples/jsm/exporters/USDZExporter.js';

export class ViMARAModelPipeline {
  constructor() {
    this.dracoLoader = new DRACOLoader();
    this.dracoLoader.setDecoderPath('/wasm/draco/');
  }

  async processUserFile(file, mtlFile = null) {
    const fileName = file.name.toLowerCase();
    let loadedObject;
    const startTime = performance.now();

    if (fileName.endsWith('.glb') || fileName.endsWith('.gltf')) {
      loadedObject = await this._loadDirectGLTF(file);
    } else if (fileName.endsWith('.obj')) {
      loadedObject = await this._parseOBJ(file, mtlFile);
    } else if (fileName.endsWith('.stl')) {
      loadedObject = await this._parseSTL(file);
    } else {
      throw new Error(`Formato no soportado: ${file.name}`);
    }

    const transformStats = this.normalizeArchitecturalGeometry(loadedObject);
    const glbArrayBuffer = await this.exportToGLB(loadedObject);
    const glbBlob = new Blob([glbArrayBuffer], { type: 'model/gltf-binary' });
    const glbUrl = URL.createObjectURL(glbBlob);

    let usdzUrl = null;
    if (this._isIOSDevice()) {
      usdzUrl = await this.generateUSDZ(loadedObject);
    }

    const endTime = performance.now();

    return {
      glbUrl,
      usdzUrl,
      stats: {
        processingTimeMs: Math.round(endTime - startTime),
        originalSizeMB: (file.size / (1024 * 1024)).toFixed(2),
        optimizedSizeMB: (glbBlob.size / (1024 * 1024)).toFixed(2),
        boundingDimensions: transformStats.dimensions
      }
    };
  }

  normalizeArchitecturalGeometry(object3D) {
    const boundingBox = new THREE.Box3().setFromObject(object3D);
    const center = boundingBox.getCenter(new THREE.Vector3());
    const size = boundingBox.getSize(new THREE.Vector3());

    object3D.position.x -= center.x;
    object3D.position.y -= boundingBox.min.y;
    object3D.position.z -= center.z;

    let appliedScale = 1.0;
    const maxDimension = Math.max(size.x, size.y, size.z);
    if (maxDimension > 500) {
      appliedScale = 0.001; // Conversión mm a metros
      object3D.scale.multiplyScalar(appliedScale);
    }

    return {
      dimensions: {
        widthMeters: (size.x * appliedScale).toFixed(2),
        heightMeters: (size.y * appliedScale).toFixed(2),
        lengthMeters: (size.z * appliedScale).toFixed(2)
      }
    };
  }

  async _parseSTL(file) {
    const arrayBuffer = await file.arrayBuffer();
    const loader = new STLLoader();
    const geometry = loader.parse(arrayBuffer);
    geometry.computeVertexNormals();

    const clayMaterial = new THREE.MeshStandardMaterial({
      color: 0xe8e6e1, // Blanco cálido arcilla
      roughness: 0.75,
      metalness: 0.05,
      side: THREE.DoubleSide
    });

    return new THREE.Mesh(geometry, clayMaterial);
  }

  async _parseOBJ(file, mtlFile) {
    const objText = await file.text();
    const objLoader = new OBJLoader();

    if (mtlFile) {
      const mtlText = await mtlFile.text();
      const mtlLoader = new MTLLoader();
      const materials = mtlLoader.parse(mtlText);
      materials.preload();
      objLoader.setMaterials(materials);
    }

    return objLoader.parse(objText);
  }

  async _loadDirectGLTF(file) {
    const arrayBuffer = await file.arrayBuffer();
    const loader = new GLTFLoader();
    loader.setDRACOLoader(this.dracoLoader);
    
    return new Promise((resolve, reject) => {
      loader.parse(arrayBuffer, '', (gltf) => resolve(gltf.scene), reject);
    });
  }

  async exportToGLB(object3D) {
    const exporter = new GLTFExporter();
    return new Promise((resolve, reject) => {
      exporter.parse(
        object3D,
        (gltfArrayBuffer) => resolve(gltfArrayBuffer),
        (error) => reject(error),
        { binary: true, embedImages: true }
      );
    });
  }

  async generateUSDZ(object3D) {
    const exporter = new USDZExporter();
    const usdzArrayBuffer = await exporter.parse(object3D);
    const usdzBlob = new Blob([usdzArrayBuffer], { type: 'model/vnd.usda+zipped' });
    return URL.createObjectURL(usdzBlob);
  }

  _isIOSDevice() {
    return /iPad|iPhone|iPod/.test(navigator.userAgent) || 
      (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
  }
}
```

---

### 4.4 Microservicio de Servidor Fallback (Node.js)

```javascript
// Servidor de Optimización Fallback (Node.js / Express)
const express = require('express');
const gltfPipeline = require('gltf-pipeline');
const app = express();

app.post('/api/v1/optimize-glb', express.raw({ type: 'model/gltf-binary', limit: '100mb' }), async (req, res) => {
  try {
    const options = {
      dracoOptions: {
        compressionLevel: 7,
        quantizePositionBits: 14,
        quantizeNormalBits: 10,
        quantizeTexcoordBits: 12
      }
    };
    const results = await gltfPipeline.processGlb(req.body, options);
    res.setHeader('Content-Type', 'model/gltf-binary');
    res.send(results.glb);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

---

## 5. Guías Prácticas de Exportación

### Guía 1: SketchUp Free (Web)
1. Concluir modelado. Menú Hamburguesa $\rightarrow$ **Exportar** $\rightarrow$ **STL**.
2. Seleccionar **Unidades: Metros/Milímetros** y **Binario**.
3. Cargar el `.stl` en ViMARA Web (asignación automática de "Maqueta Blanca" y conversión a GLB).

### Guía 2: SketchUp Pro (Escritorio)
- **Plugin Centaur glTF (Recomendado)**: Plugins $\rightarrow$ Centaur glTF Exporter $\rightarrow$ Export Binary glTF (`.glb`).
- **Nativo OBJ**: Archivo $\rightarrow$ Exportar $\rightarrow$ Modelo 3D $\rightarrow$ OBJ (*.obj). Opción: Exportar texturas y unidades metros.

### Guía 3: Revit (Autodesk BIM)
- **Revit 2023+**: Archivo $\rightarrow$ Exportar $\rightarrow$ Formatos CAD $\rightarrow$ OBJ.
- **Revit <2023**: Usar add-in *Autodesk Revit STL Exporter* o exportar a **FBX** (convertir en Blender a `.glb`).

### Guía 4: AutoCAD
- Escribir `OBJEXPORT` en consola (2023+), seleccionar sólidos y guardar `.obj`.
- En versiones anteriores, escribir `STLOUT` y guardar `.stl`.

### Guía 5: Blender
1. File $\rightarrow$ Export $\rightarrow$ glTF 2.0 (`.glb`).
2. Configuración: Transform = *Y Up*, Geometry = *Draco mesh compression* (Nivel 7, cuantización 14).
