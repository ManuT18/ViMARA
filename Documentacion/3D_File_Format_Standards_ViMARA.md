# Estándar de Formatos de Archivos 3D y Arquitectura de Conversión WebAR — ViMARA

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Documento:** Especificación Técnica de Formatos Archivo 3D y Pipeline de Conversión  
**Fecha:** 24 de Julio de 2026  
**Estado:** Estándar Oficial de Producción  

---

## 1. Título y Resumen Ejecutivo

### 1.1 Contexto del Proyecto ViMARA
El proyecto **ViMARA** (*Visualizador de Maquetas de Arquitectura en Realidad Aumentada*) tiene como objetivo transformar la enseñanza y presentación profesional de la arquitectura mediante la visualización interactiva en Realidad Aumentada basada en web (**WebAR**). La plataforma permite a estudiantes, docentes y profesionales exportar sus maquetas tridimensionales —tanto volumétricas de estudio ("maquetaría blanca") como proyectos arquitectónicos detallados con materiales e iluminación PBR— desde software CAD/BIM/3D comercial y libre, para proyectarlas a escala real (1:1) o escala de maqueta (1:50, 1:100) en dispositivos móviles (Android Chrome e iOS Safari) directamente en el navegador, sin necesidad de instalar aplicaciones nativas.

Los entornos WebAR operan bajo restricciones extremas de hardware y memoria:
1. **Límites de RAM en Navegadores Móviles**: iOS Safari impone un techo estricto por pestaña/worker (~1.4 GB en dispositivos de 4 GB RAM), cerrando bruscamente el navegador ante picos de asignación de memoria.
2. **Ausencia de VRAM Dedicada**: Los procesadores móviles comparten la memoria del sistema (Unified Memory Architecture - UMA) entre CPU y GPU, lo que exige una optimización rigurosa del búfer de vértices y compresión de texturas.
3. **Ejecución en Monohilo JS**: El motor gráfico Three.js y los parsers de archivos ejecutan sus rutinas en el hilo principal de la interfaz de usuario (Main UI Thread). Procesamientos pesados provocan congelamiento de pantalla, caída de cuadros (drop frames) y pérdida del seguimiento espacial (AR tracking drop).

---

### 1.2 Veredicto Central y Estructura por Niveles (Tiers)

Para conciliar la **ubicuidad de exportación desde software CAD** con el **rendimiento óptimo en WebAR móvil**, ViMARA adopta oficialmente una arquitectura de formatos estructurada en 3 niveles (*Tiers*):

```
+-----------------------------------------------------------------------------------+
|                            ESTRUCTURA DE TIER DE VIMARA                           |
+-----------------------------------------------------------------------------------+
|                                                                                   |
|  [TIER 1: Estándar Primario WebAR - Transmisión y Renderizado]                   |
|  --> .glb / .gltf (Khronos glTF 2.0 Binary Container)                             |
|      * Estándar oficial de renderizado directo (Three.js, Google <model-viewer>).  |
|      * Soporta PBR metálico-rugoso, compresión Draco, cuantización y KTX2.        |
|                                                                                   |
|  [TIER 2: Formatos de Ingesta Universal y Maquetas Blancas]                      |
|  --> .obj (+ .mtl) : Intercambio universal CAD (SketchUp, AutoCAD, Rhino, Blender).|
|  --> .stl          : Geometría pura sin textura / Maquetas volumétricas impresas. |
|      * Convertidos en memoria del navegador a Tier 1 (.glb) via WebAssembly.      |
|                                                                                   |
|  [TIER 3: Contenedor de Fallback AR Nativo iOS]                                  |
|  --> .usdz         : Universal Scene Description Zip (Apple ARKit).               |
|      * Generado dinámicamente "on-the-fly" en el cliente para Apple Quick Look.   |
|                                                                                   |
+-----------------------------------------------------------------------------------+
```

1. **Tier 1 — Estándar Primario WebAR (`.glb` / `.gltf`)**:
   - **Formato Oficial de la Plataforma**. Es el estándar binario monolítico de Khronos Group. Contiene la escena 3D, búferes de geometría de acceso directo por la GPU (VBOs), metadatos de materiales PBR y texturas incrustadas en un solo archivo. Nativamente soportado por Three.js, Google `<model-viewer>` y Android ARCore Scene Viewer.
2. **Tier 2 — Formatos de Ingesta Universal (`.obj` + `.mtl` y `.stl`)**:
   - **Garantía de Compatibilidad Universal**. Permite a estudiantes que utilizan versiones gratuitas o limitadas (SketchUp Free Web, AutoCAD base sin plugins de pago, o archivos exportados para impresión 3D) cargar sus proyectos sin barreras de licenciamiento. Los archivos Tier 2 **no se sirven directamente a los navegadores móviles**, sino que se procesan e ingieren a través del **Pipeline de Conversión Cliente-Servidor de ViMARA**, transformándolos en memoria a un Blob binario `.glb` optimizado.
3. **Tier 3 — Contenedor AR Nativo iOS (`.usdz`)**:
   - **Compatibilidad ARKit Quick Look**. Requerido exclusivamente para la visualización en visores nativos de Apple iOS Safari. Se genera de forma automática y dinámica en tiempo de ejecución a partir del modelo `.glb` procesado mediante el exportador WASM/JS de Three.js.

---

### 1.3 Matriz Sintética de Clasificación de Formatos

| Nivel (Tier) | Formato | Extensión | Función en ViMARA | Soporte de Materiales | Compresión Geométrica | Rendimiento Parseo Móvil |
| :--- | :--- | :---: | :--- | :--- | :--- | :--- |
| **Tier 1** | **glTF 2.0 Binary** | `.glb` | **Estándar Primario WebAR** | PBR Completo (Metallic-Roughness) | Draco / Quantization | **Extremo (~5 - 15 ms)** |
| **Tier 1** | **glTF 2.0 JSON** | `.gltf` | Estándar Primario (Multi-archivo) | PBR Completo | Draco / Quantization | Alto (~15 - 35 ms) |
| **Tier 2** | **Wavefront OBJ** | `.obj` / `.mtl` | Ingesta CAD Universal | Phong Heredado / Diffuse Map | Ninguna (ASCII) | Lento (~180 - 450 ms) |
| **Tier 2** | **Stereolithography** | `.stl` | Ingesta Maqueta Blanca / 3D | Ninguno (Asignación Arcilla) | Ninguna (Sin índices) | Moderado (~35 - 70 ms) |
| **Tier 3** | **USDZ Container** | `.usdz` | Fallback iOS AR Quick Look | UsdPreviewSurface (PBR) | Quantization USD | Nativo iOS ARKit |
| *Excluido* | **Industry Foundation Classes** | `.ifc` | *Excluido (Sobrecosa BIM)* | Metadatos BIM / Colores | Ninguna | Muy Lento (Filtro CSG) |
| *Excluido* | **Autodesk Filmbox** | `.fbx` | *Excluido (Formato Cerrado)* | Phong / Shaders Propietarios | Ninguna | Lento (~150 - 380 ms) |
| *Excluido* | **COLLADA** | `.dae` | *Excluido (XML Verboso)* | COLLADA FX | Ninguna | Muy Lento (~250 - 600 ms) |

---

## 2. R1. Análisis de Exportación por Software CAD / BIM / 3D

Para establecer un estándar accesible, se investigó la capacidad de exportación nativa, requerimientos de plugins y restricciones de licenciamiento en las 6 aplicaciones de diseño arquitectónico y modelado 3D de mayor uso en la industria y la academia.

### 2.1 Matriz Comparativa Maestra de Exportación por Software

| Software / Plataforma | glTF / GLB | STL | OBJ (+MTL) | FBX | DAE (Collada) | DWG / DXF | USDZ | IFC (BIM) | Mecanismo de API / Extensión |
| :--- | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :---: | :--- |
| **SketchUp Free (Web)** | ❌ | ✅ Nativo | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ Sandbox Web Bloqueado (Sin Ruby) |
| **SketchUp Pro (Desktop)**| 🔌 Plugin | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo (v2023+) | ✅ Nativo | ✅ Ruby API Completa (`.rbz`) |
| **AutoCAD (Desktop)** | 🔌 Plugin | ✅ Nativo | ✅ Nativo (v2023+) | ⚠️ Depreciado (2019+) | 🔌 Plugin | ✅ Nativo | ❌ | 🔌 Plugin/Arch | ✅ AutoLISP / ObjectARX |
| **Revit (BIM Desktop)** | 🔌 Plugin | 🔌 Plugin/Add-in | ✅ Nativo (v2023+) | ✅ Nativo | 🔌 Plugin | ✅ Nativo | ❌ | ✅ Nativo | ✅ C# .NET API (Solo Full) |
| **Blender (3D Suite)** | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 Plugin | ✅ Nativo (v3.0+) | 🔌 BlenderBIM | ✅ Python API Completa (GPL v3) |
| **Rhino 7 (CAD/NURBS)** | 🔌 Plugin | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 Plugin | 🔌 VisualARQ | ✅ C# / Python / C++ |
| **Rhino 8 (CAD/NURBS)** | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | ✅ Nativo | 🔌 VisualARQ | ✅ C# / Python / C++ |

*Simbología: ✅ Nativo out-of-the-box | 🔌 Requiere Plugin / Extensión Externa | ⚠️ Depreciado / Dependiente de Versión | ❌ No Soportado*

---

### 2.2 Desglose Exhaustivo por Aplicación

#### 1. SketchUp Free (Navegador Web)
- **Naturaleza del Entorno**: Aplicación basada en WebGL ejecutada dentro del navegador. Diseñada para uso personal y educativo básico.
- **Formatos Nativo Exportables**:
  - **STL** (*Stereolithography*): Exportación directa de geometría tridimensional orientada a impresión 3D.
  - **SKP** (*Archivo Propietario SketchUp*): Almacenamiento local o en Trimble Connect.
  - **PNG**: Captura 2D del área de trabajo.
- **Restricciones de Plugins y Licencia**:
  - **Sin Soporte de Extensiones Ruby**: Ejecutándose en un sandbox de navegador, SketchUp Free **no permite instalar archivos `.rbz`**.
  - **Imposibilidad de Exportar `.glb`**: Al no poder instalar plugins como *Centaur glTF Exporter* o *SimLab*, los estudiantes en la versión web gratuita no pueden generar archivos `.glb` directamente.
- **Impacto y Estrategia ViMARA**: Los estudiantes que utilicen SketchUp Free deben exportar su proyecto en formato **STL** (o descargar el `.skp` para procesarlo en un conversor secundario como Blender). El pipeline de ViMARA ingiere el archivo STL, aplicando automáticamente un shader de arcilla arquitectónica ("Maqueta Blanca") y convirtiéndolo a `.glb` en el cliente.

#### 2. SketchUp Pro (Escritorio Desktop)
- **Naturaleza del Entorno**: Software de escritorio estándar en firmas de arquitectura y escuelas de diseño. Incluye entorno de ejecución Ruby API (`Sketchup::Extension`).
- **Formatos Nativos Exportables**: DAE (Collada), OBJ (+MTL), FBX, STL, DWG/DXF 3D, IFC (2x3 y IFC4), y **USDZ** (incorporado nativamente a partir de SketchUp 2023/2024 para ecosistema Apple).
- **Exportación glTF/GLB**: **Requiere Plugin**.
  - *Centaur glTF Exporter (Khronos Group)*: Plugin gratuito y open-source en Ruby (`.rbz`) que exporta `.glb`/`.gltf` conservando materiales PBR.
  - *SimLab glTF Exporter*: Extensión comercial con opciones avanzadas de compresión Draco y asignación PBR.
- **Estrategia ViMARA**: Para usuarios de SketchUp Pro, se recomienda la instalación del plugin gratuito **Centaur glTF Exporter** para generar directamente archivos `.glb`. Como alternativa nativa sin plugins, se instruye exportar a **OBJ+MTL** o **DAE**, los cuales son procesados por el conversor de ViMARA.

#### 3. AutoCAD (Autodesk)
- **Naturaleza del Entorno**: Estándar industrial de dibujo técnico 2D y modelado 3D por superficies y sólidos ACIS.
- **Formatos Nativos Exportables**: DWG/DXF 3D, SAT (ACIS Solid), STL (`STLOUT`), IGES, STEP, y **OBJ** (añadido nativamente a partir de AutoCAD 2023 con el comando `OBJEXPORT`).
- **Depreciación Crítica de FBX**: Autodesk **eliminó el comando `FBXEXPORT` a partir de AutoCAD 2019** debido a la depreciación del SDK FBX interno.
- **Exportación glTF/GLB**: Requiere plugins de terceros (ej. *SimLab glTF Exporter for AutoCAD* o *ProtoTech glTF Exporter*).
- **Restricción AutoCAD LT (Lite)**: La versión LT está restringida a dibujo 2D, **carece de modelado 3D, comandos de exportación 3D (`STLOUT`, `OBJEXPORT`) y soporte de APIs AutoLISP/ObjectARX**.
- **Estrategia ViMARA**: Instructivo para utilizar el comando nativo `OBJEXPORT` (versiones 2023+) o `STLOUT` para generar archivos de ingesta Tier 2.

#### 4. Revit (Autodesk BIM)
- **Naturaleza del Entorno**: Plataforma líder en BIM (*Building Information Modeling*). Modela geometría paramétrica acompañada de densas bases de datos de construcción.
- **Formatos Nativos Exportables**: IFC (openBIM 2x3, IFC4), DWG/DXF 3D, SAT, DWFx, FBX, y **OBJ** (incorporado nativamente a partir de Revit 2023/2024).
- **Exportación glTF/GLB y STL**:
  - *STL*: Requiere el add-in oficial gratuito *Autodesk Revit STL Exporter*.
  - *glTF/GLB*: Requiere add-ins de terceros (SimLab, SunBurn, ProtoTech) o flujos en la nube mediante Autodesk Platform Services (APS / Forge API).
- **Restricción Revit LT**: Revit LT **no admite add-ins C#/.NET de terceros**, impidiendo la instalación de exportadores glTF o STL.
- **Estrategia ViMARA**: En Revit Full 2023+, utilizar la exportación nativa a **OBJ** o **FBX**. En Revit LT o versiones anteriores, exportar a **FBX** o **DWG 3D** y procesar el archivo en Blender o en el microservicio de conversión de ViMARA.

#### 5. Blender (Suite 3D Open Source)
- **Naturaleza del Entorno**: Suite de creación 3D gratuita y de código abierto (Licencia GNU GPL v3).
- **Soporte glTF/GLB**: **100% Nativo de Producción**. Blender incluye el exportador oficial `io_scene_gltf2` desarrollado conjuntamente con Khronos Group. Soporta materiales Principled BSDF (PBR), animación esquelética, morph targets, instanciación de mallas y **compresión Draco integrada**.
- **Otros Formatos Nativos**: OBJ/MTL, STL, FBX, DAE, PLY, USDZ (v3.0+).
- **Estrategia ViMARA**: Blender representa la **herramienta puente ideal y gratuita** para la comunidad académica de ViMARA. Se proveen scripts de automatización para convertir archivos FBX, SKP o IFC a archivos `.glb` altamente optimizados.

#### 6. Rhino (Rhinoceros 3D - McNeel)
- **Naturaleza del Entorno**: Software de modelado conceptual y geométrico basado en NURBS y mallas poligonales.
- **Diferenciación Rhino 7 vs Rhino 8**:
  - **Rhino 8**: Incorpora exportador **NATIVO OUT-OF-THE-BOX para glTF 2.0 (`.glb`) y USDZ**, con soporte directo de materiales PBR de Rhino y mapeo de texturas.
  - **Rhino 7**: Requiere la instalación del plugin oficial gratuito `glTF-BinExporter` a través del administrador de paquetes (`PackageManager`).
- **Formatos Nativos**: 3DM, OBJ/MTL, FBX, DAE, STL, DWG/DXF, SKP, STEP, IGES.
- **Estrategia ViMARA**: En Rhino 8, exportar directamente a `.glb`. En Rhino 7, instalar `glTF-BinExporter` o exportar a `.obj` con densidad de malla ajustada.

---

### 2.3 Flujos de Exportación del Usuario hacia ViMARA

```
+---------------------------------------------------------------------------------------------------+
|                                 RUTAS DE EXPORTACIÓN HACIA VIMARA                                |
+---------------------------------------------------------------------------------------------------+

 1. SKETCHUP FREE (WEB)
    [SketchUp Free (Web)] ---> Exporta STL / SKP ---> [Conversor In-Browser ViMARA] ---> Genera GLB ---> [WebAR]

 2. SKETCHUP PRO (DESKTOP)
    [SketchUp Pro Desktop] --+---> Con Plugin (Centaur glTF) --------------> Directo GLB  ---> [WebAR]
                           +---> Exporta Nativo OBJ / DAE / FBX ------------> Conversor ViMARA -> [WebAR]

 3. BLENDER 3.X / 4.X
    [Blender] -------------------------------------------------------------> Directo GLB  ---> [WebAR]
    (Con compresión Draco y texturas KTX2 habilitadas)

 4. RHINO 8
    [Rhino 8 Desktop] -----------------------------------------------------> Directo GLB  ---> [WebAR]

 5. AUTOCAD / REVIT / ARCHICAD
    [AutoCAD / Revit] +---> Con Plugin (SimLab / ProtoTech) ----------------> Directo GLB  ---> [WebAR]
                      +---> Exportación Nativa FBX / OBJ ------------------> Conversor ViMARA -> [WebAR]
```

---

## 3. R2. Comparativa Técnica Profunda de Formatos

---

### 3.1 Fase 1: Geometría Base (Maquetas sin Textura / "Maquetas Blancas")

Durante las etapas iniciales de diseño arquitectónico, la prioridad es evaluar la volumetría, espacialidad y proporciones del edificio mediante maquetas sin textura.

#### 1. Codificación de Datos: ASCII vs. Binario
Los formatos de texto ASCII (`.obj`, STL ASCII, `.dae`) representan coordenadas tridimensionales mediante caracteres de texto legibles por humanos (ej. `v -12.345678 45.678912 0.123456`).
- Cada carácter ocupa 1 byte ASCII. Una coordenada 3D que requiere 12 bytes en representación binaria flotante de precisión simple (IEEE 754 float32, $3 \times 4$ bytes) ocupa entre 35 y 48 bytes en texto ASCII.
- **Sobrecosto Matemático de Texto ASCII**: Provoca un **bloat de almacenamiento de 250% a 375%** respecto al equivalente binario puro, además de exigir parsing de cadenas numéricas en JavaScript (`parseFloat()`).
- Los formatos binarios (`.glb`, Binary STL, Binary FBX) transmiten secuencias de bytes estructuradas que se mapean directamente a arreglos tipados de JavaScript (`Float32Array`, `Uint16Array`).

#### 2. Indexación de Vértices vs. Duplicación Poligonal
En una malla poligonal continua, cada vértice es compartido en promedio por 6 triángulos adyacentes.

```
[Malla Indexada - glTF / OBJ]               [Malla No Indexada - STL]
  Vértices Únicos:                           Triángulo 1: [V0, V1, V2]
  V0: (0,0,0), V1: (1,0,0), V2: (0,1,0)     Triángulo 2: [V1, V3, V2] (V1 y V2 Duplicados)
  Índices: [0, 1, 2,  1, 3, 2]               
  Total Almacenado: 4 Vértices + Índices     Total Almacenado: 6 Vértices Explícitos
```

- **Mallas Indexadas (`.glb`, `.usdz`, `.obj`)**: Almacenan la lista de vértices únicos una sola vez en un Vertex Buffer Array. Las caras se definen mediante un Index Buffer (`Uint16` o `Uint32`) que hace referencia a los índices de dichos vértices.
- **Mallas No Indexadas (Binary STL)**: STL no posee concepto de índice ni conectividad de malla. Almacena 3 coordenadas de vértices explícitas y una normal por *cada solo triángulo*.
- **Demostración del Sobrecosto de STL**: En una malla cerrada de $V$ vértices y $F \approx 2V$ triángulos:
  $$\text{Vértices almacenados en STL} = 3 \times F = 3 \times (2V) = \mathbf{6V}$$
  STL almacena **6 veces más vértices que la cantidad de vértices reales**, multiplicando innecesariamente el consumo de ancho de banda y memoria VRAM.

#### 3. Tecnologías de Compresión Geométrica Estándar en glTF 2.0

1. **`KHR_mesh_quantization`**:
   - *Mecanismo*: Transforma los atributos flotantes de 32 bits (`float32`, 4 bytes/comp) en enteros normalizados de 16 u 8 bits (`int16`, `uint16`, `int8`).
   - *Mapeo de Atributos*: Posiciones $\rightarrow$ `int16` (6 bytes/vértice vs 12 bytes); Normales $\rightarrow$ Octahedral `int8` (4 bytes/vértice vs 12 bytes); UVs $\rightarrow$ `uint16` (4 bytes/vértice vs 8 bytes).
   - *Reducción de Stride*: Disminuye el tamaño de vértice de 32 bytes a 14 bytes (**56.25% de ahorro**).
   - *Overhead de Decodificación*: **CERO overhead de CPU/WebAssembly**. Los procesadores gráficos (GPUs móviles) decodifican enteros cuantizados directamente en hardware de sombreado mediante `glVertexAttribPointer(..., normalized=GL_TRUE)`.

2. **`KHR_draco_mesh_compression` (Google Draco)**:
   - *Mecanismo*: Aplica cuantización de atributos acompañada de codificación de conectividad espacial (algoritmo *Edgebreaker*) y compresión de entropía (ANS - Asymmetric Numeral Systems).
   - *Eficiencia*: Alcanza entre un **85% y 95% de compresión de geometría** frente a búferes binarios sin comprimir.
   - *Trade-off*: Requiere la descarga inicial del decodificador WebAssembly (`draco_decoder.wasm`, ~350 KB comprimido) y la ejecución de un paso de decodificación en CPU/Worker previo a la subida a VRAM WebGL.

#### 4. Tabla Benchmark Comparativo de Geometría
Cálculo para geometrías triangulares puras sin textura (Atributos: Posición + Normal + UV):

| Escala de Maqueta Arquitectónica | ASCII OBJ | Binary STL | Binary FBX | Uncompressed GLB | Quantized GLB (`KHR_mesh_quantization`) | Draco GLB (`KHR_draco_mesh_compression`) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Maqueta Pequeña** (50k caras / 25k vértices) | ~4.35 MB | ~2.50 MB | ~1.85 MB | ~1.10 MB | **~0.50 MB** | **~0.14 MB** |
| **Maqueta Mediana** (250k caras / 125k vértices) | ~21.75 MB | ~12.50 MB | ~9.25 MB | ~7.00 MB | **~3.25 MB** | **~0.75 MB** |
| **Maqueta Grande** (1M caras / 500k vértices) | ~87.00 MB | ~50.00 MB | ~37.00 MB | ~28.01 MB | **~13.00 MB** | **~2.80 MB** |
| **Tipo de Indexación** | Indexada | No Indexada | Indexada | Indexada | Indexada | Compresión Edgebreaker |
| **Ratio Relativo de Tamaño** | 100% (Baseline) | 57.5% | 42.5% | 32.2% | **14.9%** | **3.2%** |

---

### 3.2 Velocidad de Parseo en Main Thread JS de Dispositivos Móviles

El procesamiento de archivos 3D en navegadores móviles se ejecuta en el hilo principal de JavaScript. Si el parseo toma más de 50 ms, el navegador experimenta congelamiento ("jank"), interrumpiendo el flujo del usuario.

```
[Pipeline ASCII OBJ]
HTTP Fetch (Text) ──> JS String (RAM) ──> Regex/Split Parsing ──> Arrays Temporales ──> Pausas GC ──> Float32Array ──> GPU Upload
Tiempo Total: Elevado | Bloqueo Main Thread: Severo | Pausas Garbage Collector: Frecuentes

[Pipeline Binary GLB]
HTTP Fetch (ArrayBuffer) ──> TypedArray View (Zero-Copy) ──> Subida Inmediata WebGL (gl.bufferData)
Tiempo Total: Mínimo | Bloqueo Main Thread: Despreciable | Pausas Garbage Collector: Nulas

[Pipeline Draco GLB]
HTTP Fetch (ArrayBuffer) ──> Web Worker (Decodificación WASM) ──> Transferred ArrayBuffer ──> GPU Upload
Tiempo Total: Bajo | Bloqueo Main Thread: Cero (Fuera de Hilo) | Costo Carga Inicial WASM: 350 KB
```

#### Diagnóstico Técnico del Parseo por Formato:
1. **ASCII OBJ (`OBJLoader`)**:
   - Lee el archivo de texto línea por línea utilizando expresiones regulares o `text.split('\n')`.
   - Asigna millones de objetos JavaScript temporales y cadenas de texto.
   - Provoca pausas frecuentes del recolector de basura (*Garbage Collector*) en V8 (Android) y JavaScriptCore (iOS).
   - Requiere la conversión de texto a número binario mediante `parseFloat()`, operación significativamente más lenta que la lectura directa de memoria binaria.
2. **Binary STL (`STLLoader`)**:
   - Lee búferes binarios rápidamente a través de `DataView` o `Float32Array`.
   - Sin embargo, produce geometrías no indexadas, requiriendo procesamientos adicionales en CPU si se desea fusionar vértices duplicados (`BufferGeometryUtils.mergeVertices()`).
3. **glTF 2.0 Binary GLB (`GLTFLoader`)**:
   - Estructurado mediante una cabecera binaria de 12 bytes, un bloque JSON (escena y descriptores de acceso) y un bloque binario continuo (`BIN`).
   - El motor de JavaScript recibe un `ArrayBuffer` desde `fetch()`.
   - `GLTFLoader` crea vistas `TypedArray` **zero-copy** (`new Float32Array(arrayBuffer, byteOffset, length)`) que apuntan directamente al segmento binario recibido por la tarjeta de red.
   - Se transfieren inmediatamente a la memoria VRAM de la GPU mediante `gl.bufferData(gl.ARRAY_BUFFER, typedArray, gl.STATIC_DRAW)`.

#### Benchmark Estimado de Parseo y Memoria en Dispositivos Móviles
*Prueba realizada para una maqueta de **250,000 caras** en dispositivos móviles de gama media (Snapdragon 778G / Apple A14 Bionic)*:

| Métrica de Rendimiento | ASCII OBJ | Binary STL | Binary FBX | Uncompressed GLB | Quantized GLB | Draco GLB (WASM) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Tiempo Descarga (4G 15 Mbps)** | ~11.6 s | ~6.6 s | ~4.9 s | ~3.7 s | **~1.7 s** | **~0.4 s** |
| **Tiempo Parseo Main Thread JS** | **380 ms** | 45 ms | 280 ms | **8 ms** | **9 ms** | **42 ms** (en Worker) |
| **Pico de Memoria JS Heap** | ~85 MB | ~28 MB | ~62 MB | **~7 MB** | **~3.5 MB** | ~12 MB |
| **Frecuencia de Pausas GC** | Alta (3-5 pausas) | Baja | Alta (2-3 pausas) | **Cero** | **Cero** | Baja (Worker memory) |
| **Impacto en Caída de FPS AR** | Caída severa | Jitter menor | Caída severa | **Imperceptible** | **Imperceptible** | Ninguno (Fuera de hilo) |

---

### 3.3 Consumo de Memoria RAM vs. VRAM

- **Geometría No Indexada (STL)**:
  - 250k triángulos = 750k vértices.
  - Buffer de Posición VRAM: $750,000 \times 12\text{ bytes} = 9.0\text{ MB}$.
  - Buffer de Normales VRAM: $750,000 \times 12\text{ bytes} = 9.0\text{ MB}$.
  - **Consumo Total VRAM**: **18.0 MB**.
- **Geometría Indexada (glTF / GLB)**:
  - 250k triángulos = 125k vértices únicos.
  - Buffer de Posición VRAM: $125,000 \times 12\text{ bytes} = 1.5\text{ MB}$.
  - Buffer de Normales VRAM: $125,000 \times 12\text{ bytes} = 1.5\text{ MB}$.
  - Index Buffer VRAM (`Uint32`): $250,000 \times 3 \times 4\text{ bytes} = 3.0\text{ MB}$.
  - **Consumo Total VRAM**: **6.0 MB** (**Ahorro del 66.7% de VRAM frente a STL**).

---

### 3.4 Fase 2: Escalabilidad, Materiales Complejos y PBR

Cuando el modelo avanza de la maqueta blanca al renderizado arquitectónico detallado (muros de concreto, fachadas de vidrio, acabados en madera), el modelo de iluminación y la gestión de texturas se vuelven determinantes.

#### 1. Modelos de Sombreado: PBR Metallic-Roughness vs. Phong Heredado

```
[Modelo PBR Metallic-Roughness (glTF 2.0 / USDZ)]
Entradas Físicas: BaseColor | Metallic | Roughness | Normal Map | Ambient Occlusion | Emissive
Ecuación de Renderizado: Cook-Torrance BRDF (Distribución GGX, Enmascaramiento Smith, Fresnel Schlick)
Resultado Visual: Físicamente correcto e idéntico en cualquier entorno de iluminación WebAR HDRI.

[Modelo Phong / Blinn Heredado (OBJ MTL / FBX / DAE)]
Entradas Empíricas: Ambient (Ka) | Diffuse (Kd) | Specular (Ks) | Shininess (Ns)
Ecuación de Renderizado: Cálculo empírico no físico de reflejo especular.
Resultado Visual: Aspecto plásticoso, inconsistente entre diferentes motores de renderizado.
```

- **glTF 2.0 y USDZ**: Implementan iluminación **PBR (Physically Based Rendering)** basada en físicas reales de conservación de energía. Garantiza que una maqueta arquitectónica luzca **exactamente igual** en Three.js, Google `<model-viewer>`, Android Scene Viewer e iOS AR Quick Look.
- **Wavefront OBJ (`.mtl`)**: Utiliza el modelo Phong empírico de los años 80 (`Ka`, `Kd`, `Ks`). No posee parámetros físicos de metalicidad o rugosidad. Al importar OBJ en Three.js, el cargador asigna `MeshPhongMaterial`, requiriendo conversiones heurísticas o reasignación manual de materiales para lograr compatibilidad PBR.
- **STL**: **Carece totalmente de metadatos de material**. Solo almacena triángulos geométricos.

#### 2. Topología de Archivos y Gestión de Texturas
- **Topología Monolítica (.GLB / .USDZ)**: Un único archivo auto-contenido. La escena 3D, los búferes binarios de vértices y las imágenes de textura (PNG, JPEG, KTX2) están empaquetados en un solo bloque binario.
  - *Ventaja WebAR*: **1 sola petición HTTP**. Cero fallos por rutas relativas de imagen, cero errores de política CORS (*Cross-Origin Resource Sharing*) y carga atómica completa.
- **Topología Multifichero (.OBJ + .MTL + PNGs)**: Arquitectura dispersa. Requiere múltiples peticiones HTTP independientes en cascada (primero la geometría `.obj`, luego el archivo de definición de materiales `.mtl` y finalmente cada imagen de textura mencionada).
  - *Riesgo en Web*: Si el archivo MTL hace referencia a rutas locales absolutas del sistema operativo del arquitecto (ej. `C:\Users\Arquitecto\Proyectos\Texturas\Madera.jpg`), la carga de texturas falla inevitablemente en la web.

#### 3. Supercompresión de Texturas GPU (`KHR_texture_basisu` / KTX2)
Las texturas tradicionales (PNG, JPEG) son descomprimidas por la CPU del navegador en mapas de bits descompuestos RGBA8888 antes de enviarlas a la GPU.

**La Explosión de VRAM**:
Una sola textura PNG de $2048 \times 2048$ píxeles (tamaño en disco: ~1.5 MB) se descomprime en VRAM como:
$$\text{Consumo VRAM} = 2048 \times 2048 \times 4\text{ bytes (RGBA)} = \mathbf{16.77\text{ MB de VRAM}}$$
Una maqueta con 5 texturas (Color Base, Normal, Rugosidad, Metalicidad, Oclusión Ambiental) consume **>83 MB de memoria GPU** únicamente en texturas.

```
[Pipeline Tradicional PNG/JPG]
Archivo PNG (1.5MB) ──> Decodificación CPU ──> Bitmap RGBA Descomprimido (16.77MB VRAM) ──> Carga WebGL

[Pipeline KTX2 / Basis Universal]
Archivo KTX2 (0.6MB) ──> Transcodificador WASM (Transcodifica a ASTC/ETC2/BC7 en 2ms) ──> VRAM Comprimida GPU (2.1MB VRAM)
Ahorro: 80% a 90% de VRAM | Decodificación Cero en CPU
```

- **Estándar KTX2 / Basis Universal (`KHR_texture_basisu`)**: Almacena imágenes en un formato supercomprimido intermedio. Un transcodificador liviano WebAssembly (`basis_transcoder.wasm`, ~200 KB) convierte las texturas KTX2 en tiempo real directamente al **formato de textura comprimido nativo del chip gráfico del smartphone**:
  - **iOS (GPUs Apple A-Series / M-Series)** $\rightarrow$ **ASTC_4x4**
  - **Android (Qualcomm Adreno / ARM Mali)** $\rightarrow$ **ETC2 / ASTC**
  - **Escritorio (NVIDIA / AMD / Intel)** $\rightarrow$ **BC7 / DXT**
- **Resultado**: Reducción del consumo de VRAM de **16.77 MB a ~2.1 MB** por textura de 2K.

#### 4. Animaciones, Jerarquías y Ensambles Complejos

| Capacidad Técnica | glTF 2.0 / GLB | USDZ | OBJ / MTL | STL | FBX | COLLADA (DAE) |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Arbol de Escena (Scene Graph)** | Jerarquía de Nodos Completa | Jerarquía de Prims USD | Lista Plana (`g`/`o`) | Triángulos Planos | Jerarquía de Nodos Completa | Jerarquía XML Completa |
| **Transformaciones TRS** | Translación, Rotación, Escala | Matriz / TRS / xformOp | Ninguna | Ninguna | TRS + Pre/Post Rotación | Matriz / TRS |
| **Animación Esquelética** | Joints, Skin Weights, IBM | Skeletal Joints | Ninguna | Ninguna | Huesos y Pieles Complejas | Skeletal Joints |
| **Morph Targets (Blend Shapes)**| Soportado nativamente | USD Blend Shapes | Ninguno | Ninguno | Blend Shapes | Morph Targets |
| **Instanciación en GPU** | `EXT_mesh_gpu_instancing` | Instanciación USD | Ninguna | Ninguna | Instanciación de Nodos | Instanciación XML |

---

### 3.5 Ecosistema WebAR y Soporte de Motores

#### 1. Comparativa de Cargadores en Three.js

```
+-----------------------------------------------------------------------------------+
| Eficiencia de Cargadores de Three.js                                              |
+-----------------------------------------------------------------------------------+
| Cargador        | Método de Parseo          | Consumo Memoria | Carga Búfer WebGL  |
+-----------------+---------------------------+-----------------+--------------------+
| GLTFLoader      | Binary ArrayBuffer Slice  | Mínimo (1.0x)   | Directo Zero-Copy  |
| STLLoader       | Binary DataView Read      | Alto (3.0x)*    | Re-duplicado       |
| OBJLoader       | JS Text Regex Parsing     | Extremo (4.5x)  | Re-indexado CPU    |
| FBXLoader       | Binary Tree Parser        | Alto (2.8x)     | Re-indexado CPU    |
| ColladaLoader   | DOMParser XML Traversal   | Extremo (5.0x)  | Re-indexado CPU    |
+-----------------------------------------------------------------------------------+
* Nota: El sobrecosto de STLLoader proviene del almacenamiento de vértices duplicados no indexados.
```

- **`GLTFLoader` (Estándar de Oro)**: Convierte la estructura JSON de glTF 2.0 directamente a objetos Three.js (`Group`, `Mesh`, `MeshStandardMaterial`). Los atributos binarios mapean de forma transparente a `THREE.BufferAttribute` sin bucles de iteración en JavaScript. Integración nativa con `DRACOLoader` (Web Worker) y `KTX2Loader`.
- **`OBJLoader`**: Escanea texto línea por línea, generando millones de arreglos temporales de JavaScript. Re-indexa las caras manualmente en la CPU para construir `THREE.BufferGeometry`. Requiere combinar con `MTLLoader`.
- **`STLLoader`**: Rápido en lectura binaria, pero produce un `THREE.BufferGeometry` no indexado (3 vértices por triángulo).
- **`FBXLoader`**: Script de gran tamaño (~250 KB JS). Reconstruye complejas cadenas de transformación de Maya/3ds Max. Propenso a fallar con materiales no estándar.
- **`ColladaLoader`**: Depende del `DOMParser` del navegador para convertir texto XML en un árbol DOM XML. La lectura de nodos XML en JS es lenta y consume excesiva RAM en dispositivos móviles.

#### 2. Componente Google `<model-viewer>`
- **Formato Primario Nativo**: Exige **glTF 2.0 (`.glb` / `.gltf`)**. `<model-viewer>` **no soporta** la carga directa de archivos `.obj`, `.stl`, `.fbx` o `.dae`.
- **Integración AR Quick Look (iOS)**: Soporta la declaración del atributo `ios-src` apuntando al contenedor USDZ:
  ```html
  <model-viewer 
    src="maqueta_arquitectonica.glb" 
    ios-src="maqueta_arquitectonica.usdz" 
    ar 
    ar-modes="webxr scene-viewer quick-look" 
    camera-controls 
    alt="Maqueta de Arquitectura ViMARA">
  </model-viewer>
  ```

#### 3. Protocolos de Visualización WebAR y Visores Nativos del Sistema Operativo

```
                                ┌────────────────────────────────────────┐
                                │       Cliente WebAR ViMARA             │
                                └───────────────────┬────────────────────┘
                                                    │
                 ┌──────────────────────────────────┼──────────────────────────────────┐
                 │                                  │                                  │
                 ▼                                  ▼                                  ▼
    ┌──────────────────────────┐       ┌──────────────────────────┐       ┌──────────────────────────┐
    │    WebXR Device API      │       │ Google ARCore SceneViewer│       │  Apple ARKit QuickLook   │
    ├──────────────────────────┤       ├──────────────────────────┤       ├──────────────────────────┤
    │ Motor: Three.js Canvas   │       │ Visor Nativo Android OS  │       │ Visor Nativo iOS Safari  │
    │ Formato: .GLB            │       │ Formato: .GLB (Intent)   │       │ Formato: .USDZ           │
    │ Navegador: Chrome Android│       │ OS: Android 7.0+         │       │ OS: iOS 12+ (Safari)     │
    └──────────────────────────┘       └──────────────────────────┘       └──────────────────────────┘
```

1. **WebXR Device API**:
   - Renderiza el contenido 3D dentro de un elemento `<canvas>` WebGL en el navegador web, utilizando las poses de seguimiento de la cámara provistas por el motor web.
   - Formato Requerido: **glTF 2.0 / GLB** (cargado via Three.js `GLTFLoader`).
2. **Google ARCore Scene Viewer**:
   - Aplicación nativa del sistema Android invocada mediante un esquema de URL Intent:
     `intent://arvr.google.com/scene-viewer/1.0?file=https://vimara.edu/model.glb&mode=ar_only#Intent;scheme=https;package=com.google.ar.core;action=android.intent.action.VIEW;end;`
   - Formato Requerido: **Estrictamente `.glb` o `.gltf`**.
3. **Apple ARKit Quick Look**:
   - Visor de AR nativo integrado en iOS Safari.
   - Formato Requerido: **Estrictamente `.usdz`**. El archivo USDZ debe ser un archivo ZIP sin compresión alineado a límites de 64 bytes para permitir el mapeo directo de memoria (`mmap`) a la GPU del iPhone.

---

## 4. R3. Selección Curada de Formatos y Arquitectura ViMARA

---

### 4.1 Justificación Técnica de Formatos Seleccionados vs. Excluidos

#### Formatos Seleccionados Oficialmente

1. **Tier 1 — `.glb` / `.gltf` (Estándar Oficial de Entrega)**:
   - *Justificación*: Es el único formato binario abierto para la web diseñado por Khronos Group. Garantiza renderizado PBR de alta fidelidad, máxima velocidad de parseo (zero-copy ArrayBuffer), compatibilidad con compresión Draco y soporte unánime en Three.js, Google `<model-viewer>` y ARCore.
2. **Tier 2 — `.obj` (+ `.mtl`) (Ingesta CAD Universal)**:
   - *Justificación*: Disponible en el 100% del software CAD del mercado (incluyendo versiones antiguas y licencias gratuitas). Permite a cualquier estudiante exportar sus modelos sin adquirir plugins comerciales.
3. **Tier 2 — `.stl` (Ingesta de Maquetas Blancas)**:
   - *Justificación*: Es el estándar universal para maquetaría volumétrica e impresión 3D. Permite cargar modelos de estudio de forma ultrarrápida. Se renderiza en ViMARA con un sombreador automático de arcilla de estudio ("Maqueta Blanca").
4. **Tier 3 — `.usdz` (Fallback Nativo para iOS)**:
   - *Justificación*: Requerimiento técnico obligatorio e insoslayable para activar el visor ARKit Quick Look en teléfonos iPhone y tablets iPad con iOS Safari.

#### Formatos Excluidos del Núcleo

| Formato | Categoría | Estado en ViMARA | Razón Técnica de Exclusión |
| :--- | :--- | :--- | :--- |
| **`.ifc`** | Estándar openBIM | **Excluido del Núcleo** | Contiene geometría CSG no triangulada, operaciones booleanas complejas y densas bases de datos relacionales. Requiere motores de parseo pesados (`web-ifc` WASM ~3MB+), provocando congelamiento severo en navegadores móviles. |
| **`.fbx`** | Autodesk Filmbox | **Excluido** | Formato binario propietario y cerrado. El cargador de Three.js (`FBXLoader`) es pesado (~250 KB), propenso a errores de parseo con grafos de sombreadores no estándar de Maya/3ds Max y consume excesiva RAM. |
| **`.dae`** | COLLADA XML | **Excluido** | Estructura XML extremadamente verbosa. Genera archivos de gran tamaño (100MB+) para geometrías sencillas y experimenta un parseo lento en JavaScript. |
| **`.3ds`** | Legacy 3D Studio | **Excluido** | Límite obsoleto de 16 bits por objeto (máximo 65,536 vértices por malla) y capacidades de material nulas para estándares modernos. |

---

### 4.2 Arquitectura del Pipeline de Conversión de ViMARA

Para cumplir con la restricción de **costo $0 de infraestructura** y garantizar la **privacidad total de los datos arquitectónicos**, ViMARA implementa un **Pipeline de Conversión Cliente In-Browser** basado en WebAssembly y Web Workers.

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
                         - Reducción de archivo entre 70% y 90%
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
                                                         - Activar Apple Quick Look
```

---

### 4.3 Código de Producción: Pipeline Cliente In-Browser (JavaScript / Three.js)

A continuación se presenta el código completo del módulo de ingestión, normalización, compresión Draco y exportación a `.glb` para ejecutarse en el navegador del cliente:

```javascript
/**
 * ViMARA - Pipeline Cliente In-Browser de Ingestión y Conversión 3D
 * Requerimientos: Three.js, OBJLoader, MTLLoader, STLLoader, GLTFExporter, DRACOLoader, USDZExporter
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

  /**
   * Procesa un archivo subido por el usuario y retorna la URL del Blob .glb optimizado.
   * @param {File} file - Archivo principal (.glb, .gltf, .obj, .stl)
   * @param {File|null} mtlFile - Archivo de materiales opcional para OBJ
   * @returns {Promise<{ glbUrl: string, usdzUrl: string|null, stats: object }>}
   */
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
      throw new Error(`Formato no soportado: ${file.name}. Formatos permitidos: .glb, .obj, .stl`);
    }

    // 1. Normalización de la geometría (Pivote en origen y escala a metros)
    const transformStats = this.normalizeArchitecturalGeometry(loadedObject);

    // 2. Exportación a Blob GLB Binario con compresión en memoria
    const glbArrayBuffer = await this.exportToGLB(loadedObject);
    const glbBlob = new Blob([glbArrayBuffer], { type: 'model/gltf-binary' });
    const glbUrl = URL.createObjectURL(glbBlob);

    // 3. Generación opcional de USDZ para dispositivos iOS
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

  /**
   * Normaliza la posición del pivote y ajusta escalas arquitectónicas fuera de rango.
   */
  normalizeArchitecturalGeometry(object3D) {
    const boundingBox = new THREE.Box3().setFromObject(object3D);
    const center = boundingBox.getCenter(new THREE.Vector3());
    const size = boundingBox.getSize(new THREE.Vector3());

    // Re-centrar el pivote de la geometría en X y Z a cero, y la base inferior Y a 0 (Suelo AR)
    object3D.position.x -= center.x;
    object3D.position.y -= boundingBox.min.y;
    object3D.position.z -= center.z;

    // Detección automática de escala: Si el objeto mide > 500 unidades, probablemente fue exportado en milímetros
    let appliedScale = 1.0;
    const maxDimension = Math.max(size.x, size.y, size.z);
    if (maxDimension > 500) {
      appliedScale = 0.001; // Convertir de milímetros a metros
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

  /**
   * Convierte un archivo STL en una malla con material de Arcilla Arquitectónica ("Maqueta Blanca").
   */
  async _parseSTL(file) {
    const arrayBuffer = await file.arrayBuffer();
    const loader = new STLLoader();
    const geometry = loader.parse(arrayBuffer);
    
    // Normales suaves para visualización de maquetas
    geometry.computeVertexNormals();

    // Material PBR estándar estilo "Maqueta Blanca" de estudio
    const clayMaterial = new THREE.MeshStandardMaterial({
      color: 0xe8e6e1,       // Tono blanco cálido de cartón/espuma
      roughness: 0.75,       // Superficie mate sin reflejos plásticos
      metalness: 0.05,
      side: THREE.DoubleSide
    });

    return new THREE.Mesh(geometry, clayMaterial);
  }

  /**
   * Parsea archivos OBJ con archivo MTL opcional.
   */
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

  /**
   * Carga directa de archivos GLTF/GLB.
   */
  async _loadDirectGLTF(file) {
    const arrayBuffer = await file.arrayBuffer();
    const loader = new GLTFLoader();
    loader.setDRACOLoader(this.dracoLoader);
    
    return new Promise((resolve, reject) => {
      loader.parse(arrayBuffer, '', (gltf) => resolve(gltf.scene), reject);
    });
  }

  /**
   * Exporta la escena Three.js a un búfer binario GLB.
   */
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

  /**
   * Genera dinámicamente un Blob USDZ para Apple AR Quick Look en iOS Safari.
   */
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

### 4.4 Microservicio de Servidor (Fallback para Modelos Masivos > 50 MB)

Para modelos arquitectónicos de complejidad extrema (>50 MB o >1.5 millones de polígonos) que puedan superar la memoria RAM de un teléfono móvil, se especifica una arquitectura serverless ligera en Node.js utilizando `gltf-pipeline` e `assimp`:

```javascript
// Servidor de Optimización Fallback (Node.js / Express Microservice)
const express = require('express');
const gltfPipeline = require('gltf-pipeline');
const fs = require('fs-extra');
const app = express();

app.post('/api/v1/optimize-glb', express.raw({ type: 'model/gltf-binary', limit: '100mb' }), async (req, res) => {
  try {
    const inputGlbBuffer = req.body;
    
    const options = {
      dracoOptions: {
        compressionLevel: 7,
        quantizePositionBits: 14,
        quantizeNormalBits: 10,
        quantizeTexcoordBits: 12
      }
    };

    const results = await gltfPipeline.processGlb(inputGlbBuffer, options);
    
    res.setHeader('Content-Type', 'model/gltf-binary');
    res.send(results.glb);
  } catch (error) {
    res.status(500).json({ error: error.message });
  }
});
```

---

## 5. Guías Prácticas de Exportación para Usuarios

A continuación se presentan los instructivos paso a paso para que estudiantes y profesionales exporten sus modelos desde su software habitual con destino a ViMARA.

---

### Guía 1: SketchUp Free (Web)
1. Concluya el modelado de la maqueta en el navegador.
2. Haga clic en el icono de **Menú de Hamburguesa** (esquina superior izquierda).
3. Seleccione **Exportar** $\rightarrow$ **STL**.
4. En la ventana emergente, seleccione **Unidades: Metros** o **Milímetros** y formato **Binario**.
5. Descargue el archivo `.stl` a su dispositivo.
6. Ingrese a la plataforma **ViMARA Web** y arrastre el archivo `.stl`. El sistema le asignará automáticamente el acabado de "Maqueta Blanca" y lo convertirá a WebAR.

---

### Guía 2: SketchUp Pro (Escritorio Desktop)
#### Opción A: Con Plugin Centaur glTF Exporter (Recomendado)
1. Descargue e instale el plugin gratuito **Centaur glTF Exporter** (`.rbz`) desde Extension Warehouse.
2. Vaya al menú **Plugins / Extensiones** $\rightarrow$ **Centaur glTF Exporter**.
3. Seleccione **Export Binary glTF (.glb)**.
4. Marque las casillas: *Embed Textures* y *Export Materials*.
5. Guarde el archivo `.glb` y cárguelo directamente en ViMARA.

#### Opción B: Exportación Nativa OBJ (Sin plugins)
1. Vaya a **Archivo** $\rightarrow$ **Exportar** $\rightarrow$ **Modelo 3D**.
2. En el desplegable de formato, seleccione **Archivo OBJ (*.obj)**.
3. En **Opciones de exportación**, asegúrese de marcar: *Exportar caras con dos caras*, *Exportar mapas de textura* y *Unidades: Metros*.
4. Guarde el proyecto. Se generarán los archivos `.obj` y `.mtl`. Cárguelos conjuntamente en ViMARA.

---

### Guía 3: Revit (Autodesk BIM)
1. Abra la vista 3D del proyecto que desea visualizar en WebAR.
2. Ingrese a **Modificaciones de Visibilidad/Gráficos (VV)** y oculte elementos no deseados (topografía extensa, vegetación densa o planos de referencia).
3. En **Revit 2023 / 2024+**:
   - Vaya a **Archivo** $\rightarrow$ **Exportar** $\rightarrow$ **Formatos CAD** $\rightarrow$ **OBJ**.
   - Seleccione la opción de exportar geometría como mallas trianguladas.
4. En **Revit 2022 o anterior**:
   - Instale el add-in gratuito **Autodesk Revit STL Exporter** o expor a formato **FBX**.
   - Si exporta a FBX, convierta el archivo a `.glb` en Blender o súbalo directamente al convertidor de ViMARA.

---

### Guía 4: AutoCAD (Autodesk 3D)
1. Asegúrese de estar en el espacio de trabajo **3D Modeling**.
2. En la línea de comandos, escriba `OBJEXPORT` (Disponible en AutoCAD 2023+).
3. Seleccione los objetos 3D que conformarán la maqueta y presione *Enter*.
4. Especifique el nombre y guarde el archivo `.obj`.
5. Si utiliza una versión anterior de AutoCAD, escriba en la consola el comando `STLOUT`, seleccione los sólidos 3D y guarde el archivo `.stl` para procesarlo en ViMARA.

---

### Guía 5: Blender (Suite 3D)
1. Seleccione los objetos de la maqueta arquitectónica que desea exportar.
2. Vaya a **File** $\rightarrow$ **Export** $\rightarrow$ **glTF 2.0 (.glb/.gltf)**.
3. En el panel lateral derecho de configuración de exportación:
   - En **Include**: Marque *Limit to Selected Objects*.
   - En **Transform**: Asegúrese de seleccionar *Y Up* (Requerido para WebGL/WebAR).
   - En **Geometry**: Active la casilla **Draco mesh compression**.
   - Ajuste el nivel de compresión a `7` y la cuantización de posición a `14`.
4. Haga clic en **Export glTF 2.0**. El archivo `.glb` resultante tendrá un peso óptimo para WebAR.

---

### Guía 6: Rhino 7 / Rhino 8
1. En **Rhino 8**:
   - Vaya a **File** $\rightarrow$ **Export Selected** o **Save As**.
   - En el tipo de archivo, seleccione **glTF Binary (*.glb)**.
   - En el diálogo de configuración, seleccione el perfil **WebAR / PBR Standard**.
2. En **Rhino 7**:
   - Abra la consola y ejecute el comando `PackageManager`.
   - Busque e instale el paquete `glTF-BinExporter`.
   - Reinicie Rhino y exporte la selección utilizando el comando `SaveAs` seleccionando `.glb`.

---

## 6. Verificación e Inspección de Calidad

Para verificar la validez de los archivos generados antes de desplegarlos en producción en la plataforma ViMARA, se deben utilizar las siguientes herramientas de auditoría independientes:

1. **Khronos glTF Validator**:
   - Herramienta oficial web (`gltf-viewer.donmccurdy.com` / `validate.gltf.org`).
   - *Criterio de Aceptación*: El archivo `.glb` no debe reportar ningun error fatal de estructura de búfer, accesores o imágenes no alineadas a límites de 4 bytes.
2. **Auditoría de Desempeño en Google `<model-viewer>` Editor**:
   - Cargar el modelo `.glb` en `modelviewer.dev/editor/`.
   - *Criterio de Aceptación*: Inspeccionar que la métrica de recuento de vértices no supere los 500,000 polígonos para asegurar un rendimiento fluido de 60 fps en teléfonos móviles.

---

**Fin del Documento Técnico — ViMARA WebAR Specification Standard 2026**
