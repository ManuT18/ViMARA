# Contexto del Proyecto: ViMARA (WebAR)

## Estado General
ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es una plataforma WebAR orientada a la visualización inmersiva de modelos arquitectónicos 3D a escala real directamente desde el navegador web móvil (iOS Safari y Android Chrome), combinando una SPA en React con un motor 3D embebido en Unity WebGL + Zappar Universal AR.

## Arquitectura y Decisiones
- **Frontend Web (`src/`)**: React 19 + Vite + React Router + Lucide Icons.
  - **Catálogo y Constantes Centralizadas (`src/constants/arConstants.js`)**: Modos de tracking (`TRACKING_MODES`), formatos admitidos (`SUPPORTED_EXTENSIONS`) y maquetas de prueba (`SAMPLES`).
  - **Gestión de Estado y Memoria (`src/context/AppProvider.jsx`)**: Administración del modo de tracking y del modelo activo con revocación automática de `Blob URLs` (`URL.revokeObjectURL`) al cambiar de archivo o reiniciar sesión.
  - **Puente React ↔ Unity (`src/components/UnityARViewer.jsx` y `public/unity_ar/index.html`)**: Carga inicial por parámetros de URL (`?mode=...&model=...&format=...`) y sincronización posterior sin duplicados vía `postMessage` (`LOAD_MODEL`, `SET_MODE`, `RESIZE_VIEWPORT`).
- **Motor WebAR 3D (`unity_ar/Assets/Scripts/`, ensamblado `ViMARA`)**: Unity WebGL (URP) + Zappar Universal AR SDK:
  - **Conmutador Dual (`ARTrackingModeSwitcher.cs`)**: Alterna entre seguimiento por superficie (`ZapparInstantTrackingTarget`) y seguimiento por marcador (`ZapparImageTrackingTarget` con `marcador_logo.zpt`), desactivando en C++/WASM el tracker inactivo.
  - **Seguimiento por Plano (`InstantTrackingController.cs`)**: Visualizador procedural de **"Mantel de Puntos" (Dot Grid Tablecloth)** con caída radial suave de tamaño y opacidad (sin bordes cuadrados) y shader `Sprites/Default` transparente, fijación por toque y destrabe mediante toque corto (*tap* vs *drag/pinch*).
  - **Seguimiento por Marcador (`PersistentMarkerController.cs`)**: Ocultación automática del `Preview Object` 2D, filtro anti-temblor con zona muerta adaptativa (*Deadzone*) en `LateUpdate`, nivelación gravitatoria para marcadores horizontales y *World Lock* persistente.
  - **Manipulación Táctil (`TouchManipulationController.cs`)**: Rotación 360° con 1 dedo y escalado uniforme con 2 dedos (*pinch-to-zoom* con rango ampliado de `0.05x` a `10.0x`, permitiendo hasta 4.5 metros) con bloqueo de rotación accidental al soltar un dedo.
  - **Cargador Universal 3D (`RuntimeModelLoader.cs`, `RuntimeMeshFormats.cs`, `ProceduralSampleBuilder.cs`)**: Soporte para maquetas procedurales (`preset:house`, `preset:tower`, `preset:pavilion`), `.glb`/`.gltf` vía `glTFast` (con decodificación única en WASM, clonación al contenedor secundario y conversión de materiales a URP/Lit), `.stl` (binario/ASCII) y `.obj`, liberando explícitamente mallas, materiales e instancias `GltfImport` al cambiar de modelo.

## Tareas Completadas (Recientes)
- [x] Configuración de Zappar Instant World Tracking e Image Tracking (`marcador_logo.zpt`) con URP y soporte responsive 100vw/100dvh.
- [x] Implementación de Modo Pantalla Completa Real (con ocultación dinámica de Navbar mediante `ar-fullscreen-active` y reajuste de viewport).
- [x] Anclaje Persistente (*World Lock*), estabilización adaptativa en marcador y retícula interactiva con destrabe por toque corto en modo plano.
- [x] Manipulación táctil interactiva (rotación 360° con 1 dedo y zoom con 2 dedos sin giros espurios) con rango ampliado hasta 10.0x.
- [x] Carga dinámica multimodelo (`.glb`, `.gltf`, `.obj`, `.stl` y presets procedurales) con normalización automática a `0.45m` y apoyo sobre `Y = 0`.
- [x] Corrección de flujo y estado de inicio: inicialización limpia sin preselección forzada (`trackingMode: null`, `currentModel: null`), asegurando que la app comience en la pantalla de bienvenida (`MainMenu`) y guiando al usuario paso a paso sin tarjetas preseleccionadas.
- [x] Blindaje contra congelamientos WebGL: resolución de la causa raíz de los 30 segundos (bloqueo por licencia comercial de Zappar `licerr` al hostear fuera de su plataforma en Vercel), empaquetando localmente `zappar-cv.js` junto a todos sus binarios dependientes (`c85c7df8e3132b359723.wasm` y modelos `.zbin`) con neutralización de `licerr` y normalización de host a `127.*`; activación de `m_EnableRenderCompatibilityMode: 1` en Unity 6 URP (evitando conflictos de Render Graph con `GL.Begin(GL.QUADS)` y `GL.IssuePluginEvent` de Zappar), intercepción temprana de `document.createElement("video")` para montar en el DOM el `<video>` fuera de pantalla que crea `zappar-cv.js` en memoria, desactivación de `DepthTexture`/`HDR`/`MSAA`/`vSync`, `[DefaultExecutionOrder(-100)]` en `ARTrackingModeSwitcher`, límite de `devicePixelRatio <= 1.25` en móviles y truncado de sección `name` en `unity_ar.wasm`.
- [x] Rediseño del visualizador de detección de plano en `InstantTrackingController.cs`: reemplazo del `Quad` opaco (`Universal Render Pipeline/Unlit` sin textura) por un **"Mantel de Puntos" procedural (`CreateDotTableclothTexture`)** con desvanecimiento radial suave y shader transparente `Sprites/Default`.
- [x] Auditoría y refactorización integral del proyecto: centralización de constantes en `src/constants/arConstants.js`, liberación de `Blob URLs` en `AppProvider.jsx`, eliminación de código y dependencias muertas (`ModelViewer.jsx`, assets de plantilla, scripts de UI legacy en Unity y docs duplicadas), optimización de memoria en `RuntimeModelLoader.cs` (instanciación única de `GltfImport` + limpieza de `Mesh`/`Material`), y normalización de ensamblados (`ViMARA` y `ViMARA.Editor`).
- [x] Compilación y despliegue WebGL en Vercel: regeneración exitosa de los artefactos de compilación WebGL (`unity_ar.loader.js` y `unity_ar.wasm`) con todas las optimizaciones, commiteados y pusheados a la rama principal (`main`).

## Próximos Pasos (TODO)
- [ ] Validar en la URL de despliegue en Vercel el flujo completo: inicio limpio en el menú principal, carga de visor con mantel de puntos, manipulación táctil ampliada hasta 10x y ausencia de carteles/congelamiento de 30 segundos.
- [ ] Pruebas finales de usabilidad con modelos CAD exportados (.glb/.stl) en dispositivos iOS y Android.

## Problemas Abiertos o Notas
- En iOS Safari, el selector de archivos no debe incluir el atributo `accept` restrictivo para permitir elegir archivos `.stl` y `.glb` desde la app Archivos.
