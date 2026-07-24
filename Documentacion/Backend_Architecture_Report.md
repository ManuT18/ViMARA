# Informe de Arquitectura de Backend y Evaluación de Infraestructura para ViMARA WebAR

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada - Beca BENTRE25)  
**Autor:** Backend Architect Specialist  
**Fecha de Evaluación:** 24 de Julio de 2026  
**Ubicación:** `Documentacion/Backend_Architecture_Report.md`  
**Estado:** Final / Propuesta Ejecutiva y Técnica Aprobada  

---

## 1. Resumen Ejecutivo y Conclusiones Directas

### Conclusión Principal
**Para la versión MVP (Fase 1), UN BACKEND ES TOTALMENTE INNECESARIO Y CONTRAFLUENTE.**  
La arquitectura actual de ViMARA basada en React 19 + Vite + `<model-viewer>` + Three.js es capaz de operar al 100% en el cliente (Browser/Mobile), proporcionando latencia cero, costo de infraestructura nulo ($0/mes) y eliminando las barreras de autenticación para los estudiantes de arquitectura.

### Estrategia Recomendada por Fases
- **Fase 1 (MVP - "Local & Zero-Friction"): Architecture No-Backend (Static SPA)**
  - Carga local de archivos `.glb`, `.stl` y `.obj` desde el almacenamiento del dispositivo móvil.
  - Almacenamiento local persistente utilizando `IndexedDB` (vía Dexie.js).
  - Renderizado 3D y WebAR nativo en el cliente (AR Quick Look en iOS, Scene Viewer en Android, MindAR.js en navegador).
  - Hosting estático de alto rendimiento en Vercel, Cloudflare Pages o GitHub Pages (**Costo: $0/mes**).
- **Fase 2 ("Catálogo Comunitario y Compartición por QR"): Serverless / BaaS (Backend-as-a-Service)**
  - Adición de **Supabase** (Auth + PostgreSQL Database) y **Cloudflare R2 / Supabase Storage** (Storage de activos 3D con $0 de tarifa por transferencia de datos / egress).
  - Permite publicar maquetas en un catálogo público y generar URLs / códigos QR para visualización remota.
- **Fase 3 ("Conversión CAD/BIM Pesada y Pipeline Automatizado"): Backend Worker Asíncrono**
  - Contenedores Docker en Cloud Run / Railway ejecutando microservicios con Blender Headless, Assimp C++ SDK y Draco CLI para conversión masiva de formatos propietarios `.skp` (SketchUp nativo), `.fbx` o `.ifc` a `.glb` comprimidos.

---

## 2. Evaluación del Pipeline de Archivos 3D (Formatos y Procesamiento)

### 2.1. Formatos de Entrada en Maquetación Arquitectónica
1. **`.glb` / `.gltf` (Estándar WebAR):** Formato binario óptimo para la web. Soporta geometría, materiales PBR, texturas y animación. Soportado de forma directa por `<model-viewer>`, WebXR, Scene Viewer y AR Quick Look. No requiere conversión.
2. **`.stl` (Geometría / Maqueta Blanca):** Formato binario o ASCII ampliamente utilizado en impresión 3D y maquetas conceptuales de arquitectura. Contiene solo datos geométricos de vértices y normales (sin colores ni texturas).
3. **`.obj` + `.mtl` (Geometría y Materiales Tradicionales):** Formato de texto plano. Parseable en cliente mediante `Three.js OBJLoader`.
4. **`.skp` / `.fbx` / `.ifc` (Formatos CAD/BIM Nativos):** Formatos complejos propietarios. Requieren bibliotecas C++ pesadas (SketchUp C API, Open Asset Import Library / Assimp, Blender Python API) para su parsing y conversión a glTF.

### 2.2. Comparativa de Estrategias de Procesamiento de Archivos 3D

| Criterio de Evaluación | OPCIÓN A: Client-Side (Wasm / JS) | OPCIÓN B: Serverless Functions (AWS Lambda / Vercel) | OPCIÓN C: Dedicated Backend Worker (Docker / Queue) |
| :--- | :--- | :--- | :--- |
| **Formatos Soportados** | `.glb`, `.stl`, `.obj` | `.gltf` (optimizaciones y compresión Draco ligera) | `.skp`, `.fbx`, `.ifc`, `.obj`, `.stl`, `.glb` |
| **Costo de Infraestructura** | **$0 / mes (Totalmente Gratis)** | Pay-per-execution (Tier gratuito amplio) | $10 - $50+ / mes (Servidor en ejecución constante) |
| **Latencia de Usuario** | **Instantánea (0s de red, procesamiento local)** | 3s - 15s (dependiendo del Payload y Cold Start) | 10s - 60s (procesamiento asíncrono en cola) |
| **Límites Técnicos** | Memoria RAM del navegador móvil (~500MB límite WebGL en iOS Safari) | Timeouts (10s-60s), Límite payload API (6MB-10MB), Imposible incluir binarios pesados (>500MB Blender) | Sin límites de memoria/tiempo. Escalado horizontal con CPU/RAM dedicada. |
| **Privacidad de Datos** | **100% Local (El archivo nunca sale del dispositivo)** | Requiere transmisión a la nube | Requiere transmisión a la nube |
| **Veredicto para ViMARA** | **RECOMENDADO PARA MVP (Fase 1)** | **RECOMENDADO PARA FASE 2 (Micro-optimizaciones)** | **RECOMENDADO PARA FASE 3 (Conversión CAD compleja)** |

### 2.3. Propuesta de Pipeline
- **MVP (Fase 1):** Establecer como estándar el uso de `.glb` (exportado directamente desde SketchUp 2021+ o plugins gratuitos como *SimLab glTF Exporter*). Para geometrías `.stl` y `.obj`, utilizar los cargadores nativos de Three.js (`STLLoader` y `OBJLoader`) que operan directamente en la memoria del navegador.
- **Fase 2:** Incorporar Web Workers en el cliente utilizando `@gltf-transform/core` y `Draco-WASM` para realizar simplificación de mallas y compresión geométrica en el navegador antes de subir al catálogo cloud.
- **Fase 3:** Implementar un servicio desacoplado en Python/Node.js en Docker (Cloud Run) respaldado por Redis/Celery para convertir archivos `.skp` subidos por los usuarios que no dispongan de plugins de exportación glTF.

---

## 3. Evaluación de Almacenamiento y Hosting de Activos 3D

### 3.1. Local Storage e IndexedDB (Solución No-Backend para MVP)
- **Mecanismo:** Utilizar la biblioteca `Dexie.js` (wrapper ligero de `IndexedDB`) para almacenar los archivos 3D (`Blob` de `.glb`, `.stl`, `.obj`) en la memoria persistente del navegador del teléfono.
- **Ventajas:**
  - Carga off-line instantánea sin consumo de datos móviles.
  - Soporta cientos de Megabytes de almacenamiento por origen (hasta el 50% del disco disponible en la mayoría de navegadores modernos).
  - $0 de costo de almacenamiento y $0 de ancho de banda.
  - Privacidad total para maquetas académicas no publicadas.
- **Desventajas:** Los datos residen en el navegador local; no permite compartir un enlace o código QR con otros usuarios.
- **Veredicto:** **Solución principal e indispensable para el MVP.**

### 3.2. Storage Cloud (Fase 2 Catálogo Comunitario)
Cuando se requiera compartir maquetas mediante enlaces o códigos QR, se deberán almacenar los modelos 3D en la nube.

| Proveedor Storage Cloud | Costo Almacenamiento | Costo Transferencia (Egress) | Compatibilidad WebAR / CORS | Veredicto y Evaluación |
| :--- | :--- | :--- | :--- | :--- |
| **Cloudflare R2** | $0.015 / GB / mes | **$0 (Egress GRATIS sin límites)** | Excelente (Configuración CORS directa) | **10/10 - RECOMENDADO PARA FASE 2.** Elimina los costos por descarga masiva de modelos 3D pesados. |
| **Supabase Storage** | 1 GB Gratis (luego $0.021/GB) | 2 GB Gratis / mes | Excelente (Integrado con PostgreSQL Auth) | **9/10 - Alternativa Directa.** Ideal si se usa Supabase para base de datos y autenticación. |
| **Firebase Storage** | 5 GB Gratis | 1 GB / día Gratis | Buena | **7/10.** Adecuado, pero con riesgo de costos por Egress al escalar. |
| **AWS S3** | 5 GB Gratis (12 meses) | Costoso fuera de Free Tier | Requiere configuración manual compleja | **6/10.** Sobrecarga administrativa innecesaria para el proyecto. |

---

## 4. Evaluación de Autenticación y Gestión de Usuarios

### 4.1. MVP (Fase 1): Modo Anónimo / Sin Registro ("Zero Friction")
- **Arquitectura:** Single Page Application (SPA) 100% cliente.
- **Flujo de Usuario:** El estudiante abre la WebApp en su teléfono móvil o laptop, selecciona un archivo 3D de su disco local o elige un modelo de muestra, y visualiza la maqueta en 3D/AR en menos de 2 segundos.
- **Justificación:** Exigir un inicio de sesión para visualizar una maqueta local guardada en el propio teléfono agrega fricción innecesaria y ralentiza la adopción académica.

### 4.2. Fase 2: Autenticación Comunitario con BaaS (Backend-as-a-Service)
- **Necesidad:** Requerida únicamente cuando se implemente el catálogo compartido, perfiles de estudiante y favoritos.
- **Solución Recomendada:** **Supabase Auth**.
  - Soporta autenticación mediante correo/contraseña, Magic Links y proveedores OAuth (Google, GitHub).
  - Integración nativa con **Row Level Security (RLS)** en PostgreSQL para garantizar que solo el propietario de una maqueta pueda editarla o eliminarla.
  - SDK ultraligero e integrable con React (`@supabase/supabase-js`).

---

## 5. Gestión de Datos de Realidad Aumentada (Marcadores y Tracking de Planos)

### 5.1. Tracking de Planos (Plane Tracking)
- **Mecanismo:** El cálculo del entorno físico, detección de pisos/mesas y anclaje SLAM se realiza totalmente en hardware local mediante las APIs nativas delegadas por `<model-viewer>`:
  - **Android:** Google Scene Viewer (ARCore).
  - **iOS:** AR Quick Look (ARKit).
  - **Navegador Web:** WebXR Device API.
- **Necesidad de Backend:** **CERO (0%)**. No se requieren servidores para procesar o almacenar datos de tracking de planos.

### 5.2. Tracking de Marcadores (Image Target Tracking)
- **Mecanismo:** Proyección de maquetas sobre marcadores impresos o láminas de arquitectura (usando **MindAR.js**).
- **Procesamiento de Marcadores (Compilación de Archivos `.mind`):**
  - Los marcadores requieren compilar la imagen fuente (ej. plano de planta en PNG/JPG) en un archivo de patrones geométricos (`.mind`).
  - **MindAR incluye un compilador offline en Web Worker (`mindar-image-wfc.prod.js`)** que ejecuta la extracción de caracteristicas mediante WebAssembly directamente en el navegador del usuario en 2-4 segundos.
- **Necesidad de Backend:**
  - **MVP (Fase 1):** **0% Backend.** El usuario sube la imagen del plano en el navegador, el cliente la compila a `.mind` en segundo plano y ejecuta la cámara AR.
  - **Fase 2:** Guardar la imagen original y la compilación `.mind` en Supabase Storage junto con los metadatos del modelo 3D.

---

## 6. Hoja de Ruta Arquitectónica y Recomendación de Tech Stack

### 6.1. Diagrama de Evolución de la Arquitectura

```
========================================================================================
[ FASE 1: MVP No-Backend (Local & Zero-Friction) ] - RECOMENDADO ACTUALMENTE
========================================================================================
 Cliente (Smartphone / Laptop Browser)
  ├── UI Framework: React 19 + Vite
  ├── Storage Local: IndexedDB (Dexie.js) -> Guarda .glb / .stl localmente
  ├── Renderizador 3D: Three.js + Google <model-viewer>
  ├── Parsers Cliente: STLLoader / OBJLoader (Three.js en navegador)
  ├── Tracking AR: AR Quick Look (iOS) / Scene Viewer (Android) / MindAR.js (WebWorker)
  └── Hosting: Static CDN (Vercel / Cloudflare Pages / GitHub Pages)
      └── COSTO INFRAESTRUCTURA: $0.00 / mes
========================================================================================
                                      │
                                      ▼ (Al requerir catálogo público y códigos QR)
========================================================================================
[ FASE 2: Serverless / BaaS Híbrido (Catálogo Comunitario) ]
========================================================================================
 Cliente (React 19 SPA)
  ├── Auth: Supabase Auth (Google OAuth / Email)
  ├── Database: Supabase PostgreSQL (Metadatos de modelos, autores, carreras)
  ├── Cloud Storage: Cloudflare R2 / Supabase Storage (Modelos .glb públicos + .mind)
  └── Generador QR: Lib cliente en JS para compartir URLs públicas
      └── COSTO INFRAESTRUCTURA: $0.00 - $5.00 / mes (Tier Gratis de Supabase y R2)
========================================================================================
                                      │
                                      ▼ (Al requerir subir archivos .SKP o .FBX sin exportar)
========================================================================================
[ FASE 3: Full Pipeline con Backend Worker Asíncrono ]
========================================================================================
 Backend Serverless & Workers
  ├── Worker Container: Docker en Cloud Run / Railway / EC2
  ├── Motor Conversión: Assimp C++ SDK / Headless Blender Python API / Draco CLI
  ├── Cola de Tareas: Redis + Celery / BullMQ
  └── Flujo: Archivo CAD (.skp/.fbx) ──> Queue ──> Worker ──> .glb comprimido ──> Cloudflare R2
      └── COSTO INFRAESTRUCTURA: $15.00 - $50.00 / mes
========================================================================================
```

### 6.2. Cuadro Comparativo Matriz de Decisiones Arquitectónicas

| Criterio / Dimensión | Fase 1: MVP (Actual) | Fase 2: Catálogo & QR | Fase 3: Worker CAD |
| :--- | :--- | :--- | :--- |
| **¿Requiere Backend?** | **NO** | **SÍ (Serverless / BaaS)** | **SÍ (Worker Dedicado)** |
| **Tech Stack Recomendado** | React 19, Vite, `<model-viewer>`, Three.js, Dexie.js | Supabase (Auth + DB) + Cloudflare R2 | Docker, Cloud Run, Python/Celery, Blender CLI |
| **Hosting & Despliegue** | CDN Estático (Vercel / Cloudflare Pages) | Serverless BaaS | Cloud Containers |
| **Costo Mensual Estimado** | **$0.00 / mes** | **$0.00 / mes (Tier Gratis)** | **$15.00 - $50.00 / mes** |
| **Complejidad de Desarrollo** | Muy Baja | Baja - Media | Alta |
| **Fricción para el Usuario** | Cero (Sin registro ni login) | Baja (Login solo para publicar) | Cero (Procesamiento automático) |
| **Tiempo de Entrega (ETA)** | **Inmediato (1-2 días)** | 1-2 Semanas | 3-4 Semanas |

---

## 7. Plan de Acción Recomendado

1. **Aprobar la Arquitectura No-Backend para el MVP de ViMARA:**
   - Mantener la aplicación como una Single Page Application (SPA) pura desplegada en hosting estático ($0 costo).
2. **Integrar `Dexie.js` para Persistencia Local:**
   - Permitir que las maquetas cargadas por los estudiantes se almacenen en IndexedDB para que no se borren al recargar la página.
3. **Optimizar Parsers Cliente de Three.js:**
   - Asegurar el correcto funcionamiento de `STLLoader` y `OBJLoader` para permitir la previsualización directa de geometrías `.stl` y `.obj` en el cliente sin requerir servicios en la nube.
4. **Publicar Guía de Exportación 3D para Estudiantes:**
   - Documentar el procedimiento recomendado para exportar archivos `.glb` directamente desde SketchUp y Rhino.
5. **Reservar la Arquitectura Serverless (Supabase + Cloudflare R2) para la Fase 2:**
   - Iniciar la implementación del catálogo compartido y generación de códigos QR únicamente cuando la experiencia WebAR local esté totalmente validada.
