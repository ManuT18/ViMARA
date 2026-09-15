# Contexto del Proyecto: ViMARA (WebAR)

## Estado General
ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es una plataforma WebAR orientada a la visualización inmersiva de modelos arquitectónicos 3D a escala real directamente desde el navegador web móvil (iOS Safari y Android Chrome).

## Arquitectura y Decisiones
- **Frontend Web**: React + Vite + React Router + Lucide Icons.
- **Motor WebAR 3D**: Unity WebGL + Zappar Universal AR SDK (Instant World Tracking SLAM).
- **Entorno de Despliegue**: Vercel con cabeceras de permisos de sensores (`camera`, `accelerometer`, `gyroscope`, `xr-spatial-tracking`) y tipos MIME configurados para WebAssembly y compresión de Unity.
- **Flujo de Renderizado**: Carga asíncrona optimizada de Unity WebGL a través de iframe responsive (`UnityARViewer.jsx`) dentro de la vista `ARVisualization.jsx`.

## Tareas Completadas (Recientes)
- [x] Configuración de Zappar Instant World Tracking con URP (`ZAPPAR_SRP`, camera depth -1, Overlay camera).
- [x] Corrección de Input System dual en Unity (`activeInputHandler: 2`) para compatibilidad con eventos táctiles y teclado.
- [x] Construcción y exportación de Unity WebGL a `public/unity_ar/` con soporte responsive 100vw/100vh.
- [x] Integración de `UnityARViewer.jsx` en la ruta `/ar-view` de React.
- [x] Configuración de `vercel.json` con permisos de seguridad y optimizaciones de streaming.
- [x] Creación de la rama `feature/unity-webar-integration`.
- [x] Actualización de paneles informativos en `MainMenu.jsx` destacando capacidades WebAR, SLAM y flujo BIM/3D.

## Próximos Pasos (TODO)
- [ ] Probar la experiencia WebAR desplegada en Vercel desde dispositivos móviles reales.
- [ ] Implementar carga dinámica de modelos 3D (`.glb` / `.gltf`) directamente hacia el visor de Unity.
- [ ] Agregar controles de interfaz superpuestos en React (reinicio de anclaje, selector de modelos, ajuste de escala).

## Problemas Abiertos o Notas
- En iOS Safari, asegurar que el usuario interactúe con la pantalla para conceder permisos de sensores de movimiento y orientación (`DeviceMotionEvent.requestPermission`).
