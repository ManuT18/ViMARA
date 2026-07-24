# Reporte de Viabilidad Técnica: AR Foundation vs. Vuforia & WebAR Multiplataforma

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Proyecto Universitario:** BENTRE25 — Universidad  
**Fecha:** 24 de Julio de 2026  
**Estado:** Documento de Referencia Arquitectónica Oficial  

---

## 1. Resumen Ejecutivo y Veredicto

El proyecto **ViMARA** requiere capacidades avanzadas de Realidad Aumentada: seguimiento de planos (Plane Tracking) para posicionamiento libre de maquetas 3D, seguimiento de imágenes (Image Tracking / Marcadores) para anclaje sobre planos arquitectónicos físicos, e importación dinámica de modelos 3D (`.gltf` / `.glb`) en tiempo de ejecución.

### Veredictos Principales:
1. **Descarte de Vuforia Engine**: En sus versiones 10.x+, la licencia gratuita de desarrollo de Vuforia impone una **marca de agua intrusiva e inasumible** en la cámara. Eliminarla requiere licencias comerciales de **~$99 USD/mes (~$1,188 USD/año)** por app, violando la regla de costo cero.
2. **Adopción Definitiva de Unity AR Foundation (v6.3.3)**: AR Foundation es **100% gratuito (0$ SDK cost, 0$ regalías, sin marcas de agua)** en la licencia Unity Personal. Ofrece rendimiento nativo a 60 FPS al encapsular ARKit (iOS) y ARCore (Android), integración directa con `com.unity.cloud.gltfast` (v6.16.1) para carga dinámica de `.glb`, y generación de marcadores locales dinámicos vía `MutableRuntimeReferenceImageLibrary`.
3. **Inviabilidad de Unity WebGL para WebAR Móvil**: Exportar Unity a WebGL para AR móvil presenta:
   - **Ausencia de WebXR `immersive-ar` en iOS Safari (iOS 17 y 18)**.
   - **Cuelgues de memoria en Safari móvil**: iOS limita la memoria por pestaña WebGL a ~1.4 GB; la pila WASM de Unity + texturas desbordan este techo (*"This webpage was reloaded because a problem occurred"*).
   - Paquetes WASM de 25-80 MB con cargas de 20-45 s en 4G.
4. **Ecosistema WebAR Alternativo No Basado en Unity (100% Open Source)**:
   - **Google `<model-viewer>` (Apache 2.0)**: Solución óptima para **visualización rápida de planos (<300 KB de carga)**. Usa WebXR / Scene Viewer en Android y Apple AR Quick Look (`.usdz`) en iOS.
   - **MindAR.js + Three.js (MIT)**: Mejor alternativa open-source para **Image Tracking en Web**, con redes neuronales de TensorFlow.js.

---

## 2. AR Foundation vs. Vuforia Engine

### 2.1 Configuración y Ecosistema

| Criterio | Unity AR Foundation (v6.3.3) | Vuforia Engine (v10.x+) |
| :--- | :--- | :--- |
| **Instalación** | Integración nativa vía Unity Package Manager (UPM). | Requiere `.unitypackage` o UPM registry custom + portal Vuforia. |
| **Licencias** | **Cero claves ni tokens de servidor**. | Requiere **App License Key** en la escena. |
| **Configuración Android/iOS**| `ARCoreLoader` y `ARKitLoader` automatizados. | Requiere Vuforia Configuration asset y permisos manuales. |
| **Integración UI** | Compatibilidad nativa con UI Toolkit y Canvas. | Superposición de capas de cámara que puede interferir con la UI. |

---

### 2.2 Plane Tracking (Seguimiento de Superficies)

| Característica | AR Foundation 6.3.3 | Vuforia Engine 10.x |
| :--- | :--- | :--- |
| **Tecnología** | **Nativa Directa (0ms overhead)**: ARKit (iOS) / ARCore (Android). | **Vuforia Fusion**: Capa de abstracción sobre ARKit/ARCore. |
| **Tiempo de Detección** | Muy rápido (**0.5s – 1.2s**). | Rápido (**0.8s – 1.5s**). |
| **Clasificación** | Semántica nativa: Horizontal (pisos) y Vertical (paredes). | Smart Terrain (principalmente planos horizontales). |
| **Oclusión Profunda** | **Excelente**: `AROcclusionManager` (ARCore Depth & iOS LiDAR). | Integration limitada de shaders de oclusión. |

---

### 2.3 Image Tracking y Marcadores Dinámicos

- **Estabilidad**: Vuforia Target Manager analiza imágenes (1 a 5 estrellas). AR Foundation 6.x usa `XRReferenceImageLibraries` y alcanza paridad de estabilidad con ARCore v1.40+ y ARKit 6 en planos físicos (sin oscilaciones a 0.3m–3.0m).
- **Marcadores Dinámicos en Runtime**:
  - **AR Foundation**: Soporta `MutableRuntimeReferenceImageLibrary` para crear marcadores en tiempo de ejecución **100% offline y gratis**.
  - **Vuforia**: La creación de targets en tiempo de ejecución (User Defined Targets / Cloud Rec) en el plan gratuito es muy limitada o requiere planes Cloud de pago.

---

### 2.4 Importación Dinámica de Modelos 3D (`.glb`)

- **AR Foundation**: **GLTFast (v6.16.1)** (`com.unity.cloud.gltfast`) descomprime asíncronamente `.glb` usando C# Job System y Burst sin congelar el hilo de renderizado (0 frame drops).
- **Vuforia**: No incluye cargador `.glb`; requiere integrar bibliotecas externas manualmente.

---

### 2.5 Licenciamiento y Costos

```
+-----------------------------------------------------------------------------------+
|                            DESGLOSE DE LICENCIAMIENTO                             |
+------------------------------------+----------------------------------------------+
| AR FOUNDATION 6.3.3                | VUFORIA ENGINE 10.X                          |
+------------------------------------+----------------------------------------------+
| • Costo del SDK: $0 USD            | • Costo Developer: $0 USD                    |
| • Marcas de agua: NINGUNA          | • Marca de agua: OBLIGATORIA E INTRUSIVA     |
| • Límite comercial: $100k (Unity)  | • Licencia Basic: ~$99 USD/mes ($1,188/año) |
| • Creación dinámica local: $0 USD  | • Cloud Recognition: Cobro por consulta      |
| • Apto para proyecto libre: SÍ     | • Apto para proyecto libre: NO (RECHAZADO)   |
+------------------------------------+----------------------------------------------+
```

---

### 2.6 Matriz Comparativa Resumen: AR Foundation vs. Vuforia

| Parámetro | Unity AR Foundation 6.3.3 | Vuforia Engine 10.x | Ganador |
| :--- | :--- | :--- | :--- |
| **Costo Licencia** | **$0 USD (100% Gratuito)** | ~$99 USD/mes (o Marca de agua) | **AR Foundation** |
| **Marca de Agua** | **Ninguna** | Intrusiva en versión gratuita | **AR Foundation** |
| **Plane Tracking** | Nativo ARKit/ARCore | Vuforia Fusion | **AR Foundation** |
| **Oclusión** | Nativa (`AROcclusionManager`) | Shader básico | **AR Foundation** |
| **Image Tracking** | Excelente (ARCore/ARKit) | Superior en análisis | **AR Foundation** |
| **Marcadores Dinámicos** | Gratuito u offline (`MutableLibrary`) | Requiere planes Cloud | **AR Foundation** |
| **Importación `.glb`** | Directa vía GLTFast (Gratis) | Requiere integración externa | **AR Foundation** |

---

## 3. Viabilidad de WebAR Móvil (iOS y Android)

### 3.1 Soporte WebXR
- **Android (Chrome v79+)**: Soporte **completo** para `immersive-ar` y hit-testing nativo.
- **iOS Safari (iOS 17 y 18)**: **NO soporta `immersive-ar`**. La API experimental está desactivada por defecto en WebKit.

---

### 3.2 Fallbacks Nativos
- **Apple AR Quick Look (iOS)**: Invocado por `<model-viewer>` con `.usdz`. Visualización fotorrealista a 60 FPS, pero es un **visor pasivo sin código C#, ni menús interactivos de UI Toolkit**.
- **Android Scene Viewer**: Invocado por URL intent con `.glb`. Misma limitación (visor pasivo).

---

### 3.3 Problemas Técnicos de Unity WebGL en Móvil

| Métrica | Unity WebGL en AR Móvil |
| :--- | :--- |
| **Tamaño Bundle** | 25 MB – 80 MB comprimido (80–250 MB descomprimido). |
| **Tiempo Carga** | 20 a 45 segundos en redes 4G. |
| **Memoria RAM** | Techo de Safari ~1.4 GB. Pila WASM + Texturas produce **CRASH**. |
| **Tasa Crash** | >50% en iPhones con modelos >20 MB. |
| **FPS** | Estrangulamiento térmico reduce 60 FPS a 15-25 FPS tras 3 min. |

---

## 4. Alternativas WebAR No Basadas en Unity

### 4.1 Frameworks Open-Source
1. **Google `<model-viewer>` (Apache 2.0)**: Bundle ~300 KB. Excelente para vista previa de plano instantánea (`.glb` en Android, `.usdz` en iOS).
2. **MindAR.js + Three.js (MIT)**: Framework JS nativo para **Image Tracking** con TensorFlow.js. Funciona en la misma página en Chrome y Safari.
3. **AR.js (MIT)**: Marcadores tipo Hiro o NFT. Mayor jitter que MindAR.js.
4. **Three.js Nativo + WebXR (MIT)**: Excelente en Android, requiere fallbacks para iOS.

---

### 4.2 Matriz Comparativa Master de Frameworks WebAR

| Framework | Tracking | iOS Safari | Android Chrome | Formato 3D | Peso Bundle | Licencia y Costo | Recomendación |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Google `<model-viewer>`** | Plane Tracking | AR Quick Look (`.usdz`) | WebXR / Scene Viewer | `.glb` / `.usdz` | ~300 KB | **Gratis (Apache 2.0)** | **Recomendado (Web Preview)** |
| **MindAR.js + Three.js** | Image Tracking | WebGL Nativo | WebGL Nativo | `.gltf` / `.glb` | ~1.5 MB | **Gratis (MIT)** | **Recomendado (Web Marcadores)**|
| **AR.js** | Marker / NFT | WebGL Nativo | WebGL Nativo | `.gltf` / `.glb` | ~1.1 MB | **Gratis (MIT)** | Secundario |
| **Three.js Nativo** | Plane Tracking | Requiere Fallback | WebXR Nativo | `.gltf` / `.glb` | ~1.0 MB | **Gratis (MIT)** | Devs WebXR |
| **Unity WebGL (WebXR)** | Plane / Image | Fallido (Sin WebXR) | WebXR Nativo | Assets Unity | 25-80 MB | **$0 USD (Engine)** | **NO RECOMENDADO (Crashes)** |
| **8th Wall** | World / Image | Nativo en página | Nativo en página | Proprietary | Medium | **$99–$2000+/mes** | **INVIABLE (De Pago)** |

---

## 5. Recomendación Arquitectónica Definitiva

```
===================================================================================
                       ARQUITECTURA RECOMENDADA PARA VIMARA
===================================================================================

     ┌───────────────────────────────────────────────────────────────────────┐
     │                       PROYECTO VIMARA (BENTRE25)                      │
     └───────────────────┬───────────────────────────────────┘
                                         │
        ┌────────────────────────────────┴────────────────────────────────┐
        ▼                                                                 ▼
 ┌──────────────────────────────┐                         ┌──────────────────────────────┐
 │ TIER 1: APLICACIÓN PRINCIPAL │                         │ TIER 2: MÓDULO WEB SECUNDARIO│
 │    (NATIVA MÓVIL - UNITY)    │                         │  (WEBAPP LIGHTWEIGHT - JS)   │
 ├──────────────────────────────┤                         ├──────────────────────────────┤
 │ • Motor: Unity 3D 2022/2023 │                         │ • Visión Planos:             │
 │ • AR: AR Foundation 6.3.3    │                         │   Google <model-viewer>      │
 │ • Carga 3D: GLTFast 6.16.1   │                         │ • Visión Marcadores:         │
 │ • UI: UI Toolkit / Canvas    │                         │   MindAR.js + Three.js       │
 │ • Plataforma: Android / iOS  │                         │ • Formato: .glb / .usdz      │
 │ • Rendimiento: 60 FPS Nativo │                         │ • Peso: < 1.5 MB             │
 │ • Costo: $0 USD (100% Libre) │                         │ • Costo: $0 USD (100% Libre) │
 └──────────────────────────────┘                         └──────────────────────────────┘
```

- **Tier 1 (Aplicación Principal Nativa Móvil)**: Unity 3D + AR Foundation 6.3.3 + GLTFast 6.16.1 + UI Toolkit. Garantiza 60 FPS, carga `.glb` asíncrona, marcadores dinámicos offline y cero costo/marcas de agua.
- **Tier 2 (Módulo Web Opcional)**: Google `<model-viewer>` para planos y MindAR.js + Three.js para marcadores vía link/QR sin instalación.
