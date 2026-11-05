# Contexto del Proyecto: ViMARA (WebAR)

## Estado General
ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es una plataforma WebAR orientada a la visualización inmersiva de modelos arquitectónicos 3D a escala real directamente desde el navegador web móvil (iOS Safari y Android Chrome).

## Arquitectura y Decisiones
- **Frontend Web**: React + Vite + React Router + Lucide Icons.
- **Motor WebAR 3D**: Unity WebGL (URP) + Zappar Universal AR SDK (Soporte Dual: Instant World Tracking SLAM + Image Tracking con World Lock).
- **Cargador Universal de Modelos 3D (`RuntimeModelLoader.cs`)**:
  - **Maquetas Procedurales Instantáneas** (`ProceduralSampleBuilder.cs`): Casa Moderna con Piscina (`preset:house`), Torre Residencial Urbana (`preset:tower`) y Pabellón de Exposición (`preset:pavilion`).
  - **Archivos `.GLB` / `.GLTF`**: Carga asíncrona en tiempo de ejecución mediante **glTFast** (tanto desde URLs externas como desde `Blob URL` subidos desde el dispositivo del usuario).
  - **Archivos `.OBJ` y `.STL` (ASCII/Binario)** (`RuntimeMeshFormats.cs`): Parser en tiempo de ejecución para mallas exportadas desde SketchUp, AutoCAD, Revit, Rhino o Blender, con conversión de ejes CAD a Unity y sombreado arquitectónico URP.
  - **Normalización Automática**: Cálculo dinámico de `Bounds` para escalar proporcionalmente cualquier modelo a `0.45m` y apoyar su base exactamente sobre `Y = 0` (sobre el plano o marcador).
- **Entorno de Despliegue**: Vercel con cabeceras de permisos de sensores (`camera`, `accelerometer`, `gyroscope`, `xr-spatial-tracking`) y compresión WebGL Gzip con Decompression Fallback.
- **Flujo de Renderizado y Puente JS ↔ C#**: Carga asíncrona de Unity WebGL a través de iframe responsive (`UnityARViewer.jsx`) con paso de parámetros por URL (`?mode=...&model=...&format=...`) y cambio de modelo en caliente vía `postMessage` -> `SendMessage('AR Mode Switcher', 'LoadModelFromJS', payload)`.

## Tareas Completadas (Recientes)
- [x] Configuración de Zappar Instant World Tracking con URP (`ZAPPAR_SRP`, camera depth -1, Overlay camera).
- [x] Corrección de Input System dual en Unity (`activeInputHandler: 2`) para compatibilidad con eventos táctiles y teclado.
- [x] Construcción y exportación de Unity WebGL a `public/unity_ar/` con soporte responsive 100vw/100vh.
- [x] Integración de `UnityARViewer.jsx` en la ruta `/ar-view` de React con selector rápido de modelos en vivo.
- [x] Implementación de Modo Pantalla Completa Real (inmersivo, con ocultación dinámica de Navbar y encabezados mediante clase `ar-fullscreen-active`).
- [x] Soporte Dual de Tracking (Superficie vs Marcador `marcador_logo.zpt`) con conmutación dinámica de `AnchorOrigin` (`ARTrackingModeSwitcher.cs`).
- [x] Anclaje Persistente y World-Lock en marcador de imagen (`PersistentMarkerController.cs`).
- [x] Manipulación táctil interactiva: Rotación 360° con 1 dedo y Zoom / Escalado tipo pellizco con 2 dedos (`TouchManipulationController.cs`).
- [x] Visualizador de escaneo de plano estilo ARKit con matriz de puntos y retícula animada (`InstantTrackingController.cs`).
- [x] Corrección de congelamiento de cámara en WebGL (`Application.targetFrameRate = -1`) y activación de compresión Gzip con Decompression Fallback.
- [x] Implementación de carga dinámica multimodelo (`.glb`, `.gltf`, `.obj`, `.stl`), reparación de materiales URP para modelos `.glb` texturizados, y catálogo de maquetas arquitectónicas de prueba (`RuntimeModelLoader.cs`, `ProceduralSampleBuilder.cs`, `RuntimeMeshFormats.cs`).
- [x] Mejora de estabilidad de pose (filtro Deadzone + nivelación por gravedad en `LateUpdate`), ocultación del `Preview Object` 2D del marcador en modo Marcador (`PersistentMarkerController.cs`), y nuevo delimitador arquitectónico de planos horizontales y verticales (`InstantTrackingController.cs`).
- [x] Blindaje contra congelamiento del visor/cámara tras varios minutos de uso: desactivación explícita del tracker nativo C++/WASM inactivo (`Z.InstantWorldTrackerEnabledSet` / `Z.ImageTrackerEnabledSet`), eliminación de doble llamada SLAM por frame (`MoveAnchorOnZ = true`), límite de `devicePixelRatio <= 1.25` en móviles, compuerta CV a ~30fps y watchdog de auto-reanudación del `<video>` HTML5 en `index.html`.

## Próximos Pasos (TODO)
- [ ] Ejecutar `ViMARA > Setup AR Scene` y compilar el nuevo Build WebGL en Unity para integrar el cargador de modelos en los binarios.
- [ ] Validar en dispositivos móviles la carga de archivos `.glb`, `.stl` y `.obj` propios exportados por el usuario.

## Problemas Abiertos o Notas
- En iOS Safari, asegurar que el usuario interactúe con la pantalla para conceder permisos de sensores de movimiento y orientación (`DeviceMotionEvent.requestPermission`).
