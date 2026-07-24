# Contexto del Proyecto: ViMARA

Este archivo sirve como punto de partida para que cualquier agente de IA entienda el estado, diseño y arquitectura del proyecto al iniciar una nueva sesión de trabajo.

---

## Descripción General
**ViMARA** (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es un proyecto universitario de final de carrera (beca BENTRE25). Consiste en un sistema de visualización interactiva de maquetas y modelos tridimensionales arquitectónicos en Realidad Aumentada (AR), a escala real (1:1) o escala de maqueta de estudio (1:50, 1:100).

---

## Arquitectura y Decisiones Tecnológicas
El proyecto adopta un **Modelo Híbrido de 2 Niveles**:

1. **Tier 1: Aplicación Móvil Nativa (Unity 6 + AR Foundation 6.3.3)** — *Núcleo Principal*
   - **Rendimiento**: 60 FPS estables con modelos arquitectónicos complejos.
   - **AR Framework**: AR Foundation 6.3.3 (ARKit en iOS / ARCore en Android), 100% gratuito ($0 USD, sin marcas de agua).
   - **Carga 3D en Runtime**: `com.unity.cloud.gltfast` (GLTFast v6.16.1) para importación asíncrona de archivos `.glb` desde el celular.
   - **Marcadores**: `MutableRuntimeReferenceImageLibrary` para creación de marcadores de imagen locales en tiempo de ejecución offline.

2. **Tier 2: WebApp / WebAR (React + Three.js)** — *Módulo de Acceso Rápido*
   - **Visión de Plano**: Google `<model-viewer>` (WebXR en Android, Apple AR Quick Look `.usdz` en iOS).
   - **Visión de Marcadores**: MindAR.js + Three.js (MIT Open Source).
   - **Propósito**: Permitir previsualización instantánea vía enlaces o códigos QR sin requerir instalación previa.

---

## Sistema de Diseño UI: Mobile-First Light Theme

El sistema visual adopta una **estética limpia de estudio de arquitectura (Light Theme)** optimizada para dispositivos móviles y visualización en exterior/interior:

- **Paleta de Colores**:
  - **Fondo Neutro**: Blanco Arcilla Cálido (`#F8F9FA` / `#FFFFFF` / `#E8E6E1`) inspirada en maquetas de estudio de cartón y espuma.
  - **Acento Primario**: Azul Estudio (`#0066CC` / `#0F62FE`) para acciones principales ("INICIAR AR", botones de confirmación).
  - **Texto y Contrastes**: Carbón Oscuro (`#1A1A1A` / `#212529`) para máxima legibilidad sobre video de cámara.
  - **Neutros Secundarios**: Gris Arquitectónico (`#6C757D` / `#E9ECEF`) para bordes, tarjetas y estados inactivos.
- **Tipografía y Componentes**:
  - Fuentes sans-serif limpias (`Inter`, `system-ui`, `-apple-system`).
  - Tarjetas flotantes con bordes redondeados (`border-radius: 12px/16px`) y ligera translucidez (`backdrop-filter: blur(10px)`).
  - Controles táctiles adaptados a móviles portrait con áreas de contacto mínimas de 44x44 px.

---

## Flujo de Navegación React en 4 Pasos (Alineado con AppUIPresenter.cs)

Tanto en la aplicación nativa como en la interfaz React, la navegación sigue la máquina de estados de `AppUIPresenter.cs` / `AppUIView.cs`:

```
 [1. MainMenu] ----> [2. ModeSelection] ----> [3. ModelImport] ----> [4. AR Experience]
 (Pantalla Inicio)   (Plano vs Marcador)       (Carga y Preview 3D)  (Escena AR 1:1)
```

1. **Paso 1: MainMenu (Pantalla de Bienvenida)**
   - Evento: `OnEnterAppClicked` $\rightarrow$ Muestra el menú de selección de modo.
2. **Paso 2: ModeSelection (Selección de Modo AR)**
   - Opción A: Modo Plano (`OnSelectPlaneClicked`) $\rightarrow$ Tracking de superficies horizontales/verticales.
   - Opción B: Modo Marcador (`OnSelectMarkerClicked`) $\rightarrow$ Tracking sobre planos impresos.
   - Opciones secundarias: Popup de Información (`OnSelectionInfoClicked`) y Salir (`OnExitAppClicked`).
3. **Paso 3: ModelImport & Preview (Importación y Vista Previa 3D)**
   - Explorador de archivos (`OnOpenFileBrowserClicked`): Carga de `.glb`, `.gltf`, `.obj`, `.stl`.
   - Estado y Preview: Oculta etiquetas textuales y activa la previsualización interactiva 3D (`ShowModelPreviewStream`).
   - Validación: Habilita el botón "INICIAR AR" (`SetStartARButtonState(true)`) al completar la normalización (pivote en origen $Y=0$, escala en metros).
   - Retroceso: Botón `OnGlobalBackClicked` vuelve a Selección de Modo.
4. **Paso 4: AR Experience (Experiencia AR Interactiva)**
   - Evento: `OnStartARClicked` $\rightarrow$ Inicializa la sesión AR con la maqueta instanciada, permitiendo rotación, escalado y posicionamiento interactivo.

---

## Formatos de Archivo 3D Soportados (Estructura de Tiers)
- **Tier 1 (Estándar Primario)**: `.glb` / `.gltf` (PBR, Draco compression, cuantización KTX2).
- **Tier 2 (Ingesta Universal)**: `.obj` (+ `.mtl`) y `.stl` (convertidos en memoria cliente a Blob `.glb` con acabado de "Maqueta Blanca").
- **Tier 3 (Fallback Nativo iOS)**: `.usdz` (generado dinámicamente para Apple AR Quick Look).

---

## Estado Actual y Roadmap
1. [x] **Investigación de Viabilidad**: Confirmación de AR Foundation 6.3.3 y descarte de Vuforia por licencias/marcas de agua.
2. [x] **Estrategia de Compilación iOS**: Workflow $0 costo desde Windows usando Codemagic / GitHub Actions y Sideloadly.
3. [x] **Estándar de Formatos y Pipeline**: Especificación de pipeline cliente e ingestión `.glb`/`.stl`/`.obj`.
4. [x] **Diseño de Interfaz**: Definición del sistema de diseño Light Theme y flujo de navegación React en 4 pasos.
5. [ ] **Fase Siguiente**: Implementación de los componentes de la interfaz WebApp y pipeline de visualización.
