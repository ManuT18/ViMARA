# Contexto del Proyecto: ViMARA (WebAR)

## Estado General
ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es una plataforma WebAR orientada a la visualización inmersiva de modelos arquitectónicos 3D a escala real directamente desde el navegador web móvil (iOS Safari y Android Chrome).

## Arquitectura y Decisiones
- **Frontend Web**: React + Vite + React Router + Lucide Icons.
- **Motor WebAR 3D**: Unity WebGL (URP) + Zappar Universal AR SDK (Soporte Dual: Instant World Tracking SLAM + Image Tracking con World Lock).
- **Entorno de Despliegue**: Vercel con cabeceras de permisos de sensores (`camera`, `accelerometer`, `gyroscope`, `xr-spatial-tracking`) y tipos MIME configurados para WebAssembly y compresión de Unity.
- **Flujo de Renderizado**: Carga asíncrona optimizada de Unity WebGL a través de iframe responsive (`UnityARViewer.jsx`) dentro de la vista `ARVisualization.jsx`.
- **Rendimiento Térmico y Batería**: Limitación de render scale en móviles (`devicePixelRatio` cap a 1.35) y fotogramas limitados a 60 FPS (`Application.targetFrameRate = 60`) para evitar calentamiento de GPU.

## Tareas Completadas (Recientes)
- [x] Configuración de Zappar Instant World Tracking con URP (`ZAPPAR_SRP`, camera depth -1, Overlay camera).
- [x] Corrección de Input System dual en Unity (`activeInputHandler: 2`) para compatibilidad con eventos táctiles y teclado.
- [x] Construcción y exportación de Unity WebGL a `public/unity_ar/` con soporte responsive 100vw/100vh.
- [x] Integración de `UnityARViewer.jsx` en la ruta `/ar-view` de React.
- [x] Implementación de Modo Pantalla Completa Real (inmersivo, con ocultación dinámica de Navbar y encabezados mediante clase `ar-fullscreen-active`).
- [x] Soporte Dual de Tracking (Superficie vs Marcador `marcador_logo.zpt`) con conmutación dinámica de `AnchorOrigin` (`ARTrackingModeSwitcher.cs`).
- [x] Anclaje Persistente y World-Lock en marcador de imagen (`PersistentMarkerController.cs`).
- [x] Manipulación táctil interactiva: Rotación 360° con 1 dedo y Zoom / Escalado tipo pellizco con 2 dedos (`TouchManipulationController.cs`).
- [x] Visualizador de escaneo de plano estilo ARKit con matriz de puntos y retícula animada (`InstantTrackingController.cs`).
- [x] Herramienta de autoconfiguración de escena en Unity Editor (`ViMARA > Setup AR Scene`).
- [x] Optimización de compilación rápida en Unity (`ViMARA > Fast WebGL Build (<30s)`).
- [x] Despliegue sincronizado en repositorio remoto (`main`).

## Próximos Pasos (TODO)
- [ ] Validar el anclaje persistente y los gestos táctiles en dispositivos móviles reales (iPhone y Android).
- [ ] Implementar carga dinámica de modelos 3D (`.glb` / `.gltf`) en tiempo de ejecución con **glTFast**.
- [ ] Normalización automática de escala y pivote para maquetas externas importadas por el usuario.

## Problemas Abiertos o Notas
- En iOS Safari, asegurar que el usuario interactúe con la pantalla para conceder permisos de sensores de movimiento y orientación (`DeviceMotionEvent.requestPermission`).
