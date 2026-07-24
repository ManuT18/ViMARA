# Contexto del Proyecto: ViMARA

## Estado General
**ViMARA** (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es un proyecto universitario (beca BENTRE25) enfocado en transformar la enseñanza y presentación profesional de la arquitectura mediante la visualización de maquetas 3D en Realidad Aumentada a escala real (1:1) o de maqueta (1:50, 1:100).

El proyecto adopta una arquitectura híbrida de 2 niveles:
1. **Tier 1 (App Nativa Móvil Unity 6 + AR Foundation 6.3.3)**: Aplicación nativa principal para máxima estabilidad de seguimiento, rendimiento 60 FPS, marcadores dinámicos offline y carga dinámica `.glb` con GLTFast.
2. **Tier 2 (WebApp / WebAR React + Three.js)**: Módulo web secundario de previsualización ultraligera (<1.5 MB) que utiliza Google `<model-viewer>` (planos) y MindAR.js (marcadores) para compartir modelos vía enlaces/QR sin instalación.

---

## Sistema de Diseño: Mobile-First Light Theme

Para optimizar la legibilidad en pantalla móvil tanto en interiores como en exteriores bajo luz solar directa, la UI utiliza un **Light Theme (Estética de Estudio de Arquitectura)**:

- **Paleta de Colores**:
  - **Fondo Neutro**: Blanco Arcilla de Estudio (`#F8F9FA` / `#FFFFFF` / `#E8E6E1`)
  - **Acento Primario**: Azul Estudio (`#0066CC` / `#0F62FE`) para acciones principales ("INICIAR AR", botones primarios).
  - **Tipografía y Neutros Oscuros**: Carbón / Pizarra Oscura (`#1A1A1A` / `#212529`) para alto contraste.
  - **Neutros Secundarios**: Gris Arquitectónico (`#6C757D` / `#E9ECEF`) para bordes, separadores y tarjetas.
  - **Estados**: Verde Éxito (`#28A745`), Amarillo Advertencia (`#FFC107`).
- **Tipografía**: Sans-serif limpia (`Inter`, `system-ui`, `-apple-system`, `sans-serif`), con jerarquía clara de títulos y etiquetas.
- **Componentes Móviles**: Tarjetas con bordes redondeados (`border-radius: 12px/16px`), áreas táctiles de mínimo 44x44 px, contenedores flotantes con translucidez (`backdrop-filter: blur(10px)`) y controles integrados para experiencia táctil en portrait.

---

## Flujo de Navegación React en 4 Pasos (Equivalente a AppUIPresenter.cs)

La experiencia de interfaz y navegación React replica fielmente el flujo y la máquina de estados presentada en la arquitectura nativa de Unity (`AppUIPresenter.cs` / `AppUIView.cs`):

```
 ┌─────────────────┐       ┌─────────────────┐       ┌─────────────────────┐       ┌─────────────────┐
 │ Paso 1:         │       │ Paso 2:         │       │ Paso 3:             │       │ Paso 4:         │
 │ MainMenu        │ ----> │ ModeSelection   │ ----> │ ModelImport         │ ----> │ AR Experience   │
 │ (Pantalla       │ Tap   │ (Selección Modo │ Tap   │ (Importación        │ Tap   │ (Experiencia    │
 │  Bienvenida)    │       │  Plano/Marcador)│       │  y Previsualización)│ "AR"  │  AR Interactiva)│
 └─────────────────┘       └─────────────────┘       └─────────────────────┘       └─────────────────┘
                                   │                            │
                            (Info / Salir)               (Global Back)
```

1. **Paso 1: MainMenu (Pantalla de Bienvenida)**
   - Vista inicial de la app ("Entrar a la App"). Al tocar la pantalla o el botón de entrada (`OnEnterAppClicked`), transiciona a la Selección de Modo.
2. **Paso 2: ModeSelection (Selección de Modo AR)**
   - El usuario elige entre dos modos de visualización:
     - **Modo Plano (Plane Tracking)** (`OnSelectPlaneClicked`): Posicionamiento libre de maquetas sobre superficies horizontales o verticales.
     - **Modo Marcador (Image Tracking)** (`OnSelectMarkerClicked`): Anclaje sobre planos arquitectónicos o marcadores físicos impresos.
   - Incluye Popup de Información (`OnSelectionInfoClicked` / `OnCloseInfoClicked`) y opción de Salir (`OnExitAppClicked`).
3. **Paso 3: ModelImport & Preview (Importación y Vista Previa 3D)**
   - Permite seleccionar archivos 3D localmente (`.glb`, `.gltf`, `.obj`, `.stl`) mediante el explorador del dispositivo (`OnOpenFileBrowserClicked`).
   - Muestra el estado del archivo (`UpdateFileStatusLabel`) y renderiza la previsualización interactiva 3D en streaming (`ShowModelPreviewStream`).
   - Tras cargar y normalizar la geometría (pivote en suelo $Y=0$, escala a metros), habilita el botón principal "INICIAR AR" (`SetStartARButtonState(true)`).
   - Botón Global de Retroceso (`OnGlobalBackClicked`) permite volver al Paso 2.
4. **Paso 4: AR Experience (Experiencia AR Interactiva)**
   - Al presionar "INICIAR AR" (`OnStartARClicked`), se lanza la sesión AR con el modelo cargado (WebXR / Scene Viewer / AR Quick Look / Three.js Canvas), permitiendo rotar, escalar y posicionar la maqueta a escala 1:1 o escala de estudio.

---

## Tareas Completadas (Recientes)
- [x] Análisis comparativo y benchmark AR Foundation vs. Vuforia (Vuforia descartado por licencias y marcas de agua).
- [x] Evaluación de compilación iOS desde Windows ($0 costo vía GitHub Actions / Codemagic y sideloading Sideloadly).
- [x] Análisis de viabilidad Unity WebGL vs. WebAR nativo (descartada compilación Unity WebGL para móviles por crashes de RAM en Safari e incoherencia de WebXR).
- [x] Definición del estándar oficial de formatos 3D (Tier 1 `.glb`/`.gltf`, Tier 2 `.obj`/`.stl`, Tier 3 `.usdz`).
- [x] Definición del sistema de diseño Mobile-First Light Theme y flujo de navegación React en 4 pasos alineado con `AppUIPresenter.cs`.
- [x] Implementación de navegación multipágina React con `react-router-dom` (páginas: `MainMenu`, `ModeSelection`, `ModelImport`, `ARVisualization`).
- [x] Creación de contexto global `AppContext` / `AppProvider` / `useApp` para estado compartido entre páginas.
- [x] Evaluación de arquitectura backend: MVP funciona 100% client-side (IndexedDB). Reporte en `Documentacion/Backend_Architecture_Report.md`.
- [x] Limpieza y depuración de documentación técnica en `Documentacion/`.
- [x] Actualización incremental del grafo de conocimiento graphify (2364 nodos, 2497 aristas, 325 comunidades).
- [x] Verificación de build de producción exitosa (`vite build` limpio, 267 kB JS / 5.7 kB CSS).

---

## Próximos Pasos (TODO)
- [ ] Revisar y refinar la UI de las 4 páginas creadas (diseño final light theme, micro-interacciones, responsive mobile).
- [ ] Integrar el pipeline cliente JS `ViMARAModelPipeline` en `ModelImport` para conversión y normalización de archivos STL/OBJ a GLB.
- [ ] Conectar el visor `<model-viewer>` y MindAR.js en la página `ARVisualization` según el modo seleccionado (Plano / Marcador).
- [ ] Hacer deploy a Vercel del nuevo flujo multipágina y verificar en dispositivo móvil real.
- [ ] (Fase 2) Evaluar integración de Supabase para catálogo compartido de modelos.
