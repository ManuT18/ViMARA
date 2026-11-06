# ViMARA — Visualizador de Maquetas de Arquitectura en Realidad Aumentada

**ViMARA** es una Single Page Application (SPA) con motor **Unity WebGL + Zappar Universal AR** integrada, diseñada para la visualización inmersiva de maquetas arquitectónicas 3D directamente desde navegadores móviles (iOS Safari y Android Chrome) sin requerir instalaciones externas.

## Características Principales

- **Tracking Dual en Tiempo Real**:
  - **Seguimiento por Plano (Instant World Tracking / SLAM)**: Detección de superficies con retícula interactiva, colocación por toque y destrabe por toque corto para reubicar la maqueta.
  - **Seguimiento por Marcador (Image Tracking + World Lock)**: Anclaje sobre el marcador `marcador_logo.zpt` con estabilización adaptativa (*Deadzone*), nivelación gravitatoria automática y persistencia espacial cuando el marcador sale de cámara.
- **Carga Dinámica Multimodelo**:
  - **Maquetas Procedurales Instantáneas**: Casa Moderna con Piscina (`preset:house`), Torre Residencial Urbana (`preset:tower`) y Pabellón de Exposición (`preset:pavilion`).
  - **Formatos CAD/3D Estándar**: Importación en tiempo de ejecución de archivos `.glb`, `.gltf` (vía `glTFast` con conversión automática de materiales a URP/Lit), `.stl` (binario y ASCII) y `.obj`.
  - **Normalización Automática**: Ajuste proporcional de escala a `0.45m` y centrado con apoyo exacto sobre `Y = 0`.
- **Manipulación Táctil e Inmersión**:
  - Rotación 360° con 1 dedo y escalado tipo pellizco (*pinch-to-zoom*) con 2 dedos.
  - Modo de Pantalla Completa real adaptado a iOS Safari, Android y Desktop.

## Estructura del Proyecto

```text
├── public/
│   ├── markers/             # Marcadores arquitectónicos imprimibles
│   └── unity_ar/            # Build WebGL exportado desde Unity y plantilla activa
├── src/
│   ├── components/          # Componentes UI (Navbar, FileUploader, SampleSelector, UnityARViewer, InfoModal)
│   ├── constants/           # Catálogo de maquetas, modos de tracking y formatos soportados
│   ├── context/             # Estado global (AppProvider con gestión de memoria de Blob URLs)
│   └── pages/               # Flujo en 4 vistas (MainMenu, ModeSelection, ModelImport, ARVisualization)
└── unity_ar/
    └── Assets/
        ├── Scenes/          # Escena principal WebAR_InstantTracking.unity
        ├── Scripts/AR/      # Controladores C# de tracking, estabilización, gestos e importación 3D
        ├── Scripts/Editor/  # Herramientas de autoconfiguración y compilación rápida WebGL (<30s)
        └── WebGLTemplates/  # Plantilla WebGL personalizada de Zappar
```

## Comandos de Desarrollo

```bash
# Iniciar servidor de desarrollo Vite
npm run dev

# Ejecutar linter (Oxlint)
npm run lint

# Generar build de producción de la SPA
npm run build
```

## Flujo de Compilación en Unity

1. Abrir la carpeta `unity_ar/` en Unity Editor.
2. En el menú superior, ejecutar **`ViMARA > Setup AR Scene`** para verificar referencias y shaders URP/glTFast.
3. Ejecutar **`ViMARA > ⚡ Compilar WebGL Rápido (Desarrollo <30s)`** (`Ctrl+Shift+B`) para exportar directamente a `public/unity_ar/` optimizando automáticamente el tamaño de `unity_ar.wasm` (<50 MB).
