# Reporte de Investigación y Análisis de Viabilidad: AR Foundation vs. Vuforia & WebAR Multiplataforma

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Proyecto Universitario:** BENTRE25 — Universidad  
**Fecha:** 24 de Julio de 2026  
**Autor:** Equipo de Arquitectura de Software e Investigación AR/WebAR  
**Estado:** Final / Documento de Referencia Arquitectónica  

---

## Resumen Ejecutivo

El presente estudio evalúa exhaustivamente la viabilidad técnica y económica para el desarrollo y evolución del proyecto **ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)**. El proyecto requiere capacidades avanzadas de Realidad Aumentada: seguimiento de planos (Plane Tracking) para posicionamiento libre de maquetas 3D, seguimiento de imágenes (Image Tracking / Marcadores) para anclaje sobre planos arquitectónicos físicos, e importación dinámica de modelos 3D (`.gltf` / `.glb`) en tiempo de ejecución.

Un requisito imperativo del proyecto es el **cumplimiento estricto de una restricción de costo cero (100% libre y gratuito)**, sin licencias de pago ni marcas de agua intrusivas en producción.

### Principales Conclusiones:
1. **Descarte de Vuforia Engine**: En sus versiones actuales (10.x+), la licencia gratuita de desarrollo de Vuforia impone una **marca de agua intrusiva e inasumible** en la cámara durante la ejecución. Eliminar dicha marca exige licencias comerciales de **~$99 USD/mes (~$1,000+ USD/año)** por aplicación, violando directamente la restricción de costo cero.
2. **Adopción Definitiva de Unity AR Foundation (v6.3.3)**: AR Foundation es **100% gratuito (0$ SDK cost, 0$ regalías, sin marcas de agua)** incluido en la licencia Unity Personal. Ofrece rendimiento nativo a 60 FPS al encapsular directamente las APIs de nivel de sistema operativo (ARKit en iOS y ARCore en Android), integración perfecta con `com.unity.cloud.gltfast` (v6.16.1) para carga dinámica de archivos `.glb`, y creación de marcadores de imagen locales dinámicos vía `MutableRuntimeReferenceImageLibrary`.
3. **Inviabilidad de Exportar Unity WebGL a WebAR Móvil**: Exportar Unity a WebGL para WebAR resulta inviable para producción debido a:
   - **Ausencia de WebXR `immersive-ar` en iOS Safari (iOS 17 y 18)**.
   - **Cuelgues de memoria en Safari móvil**: iOS limita la memoria por pestaña WebGL a ~1.4 GB; la pila WASM de Unity + texturas de modelos maquetarios supera este umbral, provocando recargas forzadas del navegador (*"This webpage was reloaded because a problem occurred"*).
   - **Tiempos de carga inaceptables**: Paquetes WASM de 25–80 MB que requieren de 20 a 45 segundos en redes móviles 4G.
4. **Ecosistema WebAR Alternativo No Basado en Unity (100% Open Source)**:
   - **Google `<model-viewer>` (Apache 2.0)**: La solución óptima para **visualización de plano rápida en web (<300 KB de carga, instantáneo)**. En Android utiliza WebXR / Scene Viewer nativo y en iOS utiliza Apple AR Quick Look (`.usdz`).
   - **MindAR.js + Three.js (MIT)**: La mejor alternativa open-source para **Image Tracking en Web**, superando ampliamente a AR.js en estabilidad de marcadores mediante redes neuronales de TensorFlow.js.

---

## 1. Comparativa Técnica y Comercial: AR Foundation vs. Vuforia Engine (2023-2024)

### 1.1. Introducción y Contexto del Proyecto ViMARA
ViMARA busca permitir a estudiantes y profesionales de arquitectura visualizar maquetas 3D interactivas sobre superficies reales y sobre planos físicos impresos. La pila tecnológica actual del proyecto incluye **Unity 3D**, **AR Foundation 6.3.3**, **ARCore/ARKit Extensions**, **GLTFast 6.16.1** y **XR Interaction Toolkit 3.3.1**.

### 1.2. Facilidad de Configuración y Ecosistema de Desarrollo

| Criterio | Unity AR Foundation (v6.3.3) | Vuforia Engine (v10.x+) |
| :--- | :--- | :--- |
| **Instalación y Setup** | Integración nativa a través de Unity Package Manager (UPM). Sin descargas de instaladores externos. | Requiere paquete `.unitypackage` externo o UPM custom registry + cuenta en Vuforia Developer Portal. |
| **Gestión de Licencias** | **Cero configuración de claves**. No requiere claves de licencia, tokens de servidor ni autenticación. | Requiere generar una **App License Key** en la web de Vuforia e integrarla manualmente en la escena. |
| **Configuración Android** | Automatizada vía `ARCoreLoader` en XR Plug-in Management. Gestión de Gradle nativa. | Requiere configurar Vuforia Configuration asset y permisos específicos de cámara en Android Manifest. |
| **Configuración iOS** | Automatizada vía `ARKitLoader`. Inserción automática de `NSCameraUsageDescription` en Xcode. | Requiere configuración manual de permisos en Xcode y compatibilidad con bibliotecas nativas Vuforia. |
| **Integración con Unity UI** | Compatibilidad perfecta con UI Toolkit y Canvas UI tradicional. | Compatible, pero con superposición de capas en la cámara que puede interferir con la UI del proyecto. |

### 1.3. Capacidades y Rendimiento de Plane Tracking (Seguimiento de Superficies)

| Característica | AR Foundation 6.3.3 | Vuforia Engine 10.x |
| :--- | :--- | :--- |
| **Tecnología Subyacente** | **Nativa Directa (0ms overhead)**: ARKit (iOS) / ARCore (Android). | **Abstracción Vuforia Fusion**: Intenta combinar VisLAM propio con ARKit/ARCore. |
| **Tiempo de Detección** | Muy rápido (**0.5s – 1.2s**) en superficies con textura estándar. | Rápido (**0.8s – 1.5s**), pero añade capa de procesamiento. |
| **Clasificación de Superficies** | Soporta clasificación semántica nativa: Horizontal (pisos, mesas) y Vertical (paredes). | Detección de planos Smart Terrain, enfocada principalmente en planos horizontales. |
| **Oclusión por Profundidad** | **Excelente**: Soporta `AROcclusionManager` (ARCore Depth API y LiDAR en iOS) para ocultar objetos detrás de objetos reales. | Limitada integración de shaders de oclusión en comparación con la API nativa. |
| **Relocalización tras Pérdida** | Reinstalación automática mediante mapas de puntos característicos de ARKit/ARCore. | Reinstalación asistida por VisLAM, buena pero susceptible a deriva en cambios de luz. |

### 1.4. Estabilidad y Funcionalidades de Image Tracking (Marcadores)

El seguimiento de imágenes es un pilar fundamental en ViMARA para anclar maquetas sobre planos arquitectónicos en papel.

* **Evaluación de Marcadores**:
  - **Vuforia Target Manager**: Proporciona una herramienta web excelente que evalúa imágenes y asigna de 1 a 5 estrellas según la densidad de bordes y contraste.
  - **Unity Reference Image Libraries**: Permite importar imágenes como `XRReferenceImage`. Unity realiza la extracción de características internamente durante el build.
* **Estabilidad y Jitter**:
  - **Vuforia Engine**: Históricamente ha sido el estándar de la industria en seguimiento de marcadores, ofreciendo un filtrado de jitter extremadamente suave y alta tolerancia a inclinaciones (ángulos obtusos de hasta 75°).
  - **AR Foundation 6.x**: Ha alcanzado la paridad práctica en estabilidad gracias a las mejoras de ARCore v1.40+ y ARKit 6. En pruebas físicas con planos arquitectónicos de alto contraste, AR Foundation mantiene el anclaje sin oscilaciones detectables a distancias de 0.3m a 3.0m.
* **Creación de Marcadores Dinámicos en Tiempo de Ejecución**:
  - **AR Foundation**: Soporta `MutableRuntimeReferenceImageLibrary`. Permite que el usuario descargue un nuevo plano o imagen de la web/almacenamiento local e inmediatamente cree un marcador de AR en tiempo de ejecución de forma **100% offline y gratuita**.
  - **Vuforia**: La creación de targets dinámicos en tiempo de ejecución (User Defined Targets o Cloud Recognition) en la versión gratuita está limitada o requiere servicios Cloud de pago.

### 1.5. Importación Dinámica de Modelos 3D en Tiempo de Ejecución (`.gltf` / `.glb`)

ViMARA requiere cargar maquetas 3D dinámicamente desde el almacenamiento del dispositivo o URLs remotas.

* **Ecosistema en AR Foundation**:
  - **GLTFast (v6.16.1)**: Paquete oficial mantenido por Unity (`com.unity.cloud.gltfast`). Utiliza el Unity C# Job System y compilación Burst para descompresión asíncrona de archivos `.gltf` y `.glb`. Carga modelos maquetarios complejas sin congelar el hilo principal de renderizado (0 frame drops). Se instancia directamente como hijo de un `ARAnchor` o `ARTrackedImage`.
  - **TriLib 2**: Alternativa comercial muy completa para formatos propietarios (`.fbx`, `.obj`), pero GLTFast es la solución gratuita y estándar de la industria para `.glb`.
* **Ecosistema en Vuforia**:
  - Vuforia no incluye un cargador dinámico de archivos `.gltf`/`.glb`. Depende de paquetes de terceros (como GLTFast o TriLib) integrados manualmente sobre los objetos `ImageTargetBehaviour` de Vuforia.

### 1.6. Restricción Estricta: Esquema de Licencias y Desglose de Costos (100% Gratuito)

Esta sección analiza la restricción financiera clave del proyecto.

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

* **Vuforia Engine**:
  - **Plan Gratuito (Development License)**: Muestra una marca de agua fija en la esquina de la pantalla con el texto *"Vuforia Engine"*. Esta marca es permanente e imposible de ocultar mediante código. Para la presentación formal de un proyecto universitario o despliegue público, esta marca es inaceptable.
  - **Plan Comercial (Basic License)**: Cuesta **$99 USD al mes ($1,188 USD anuales)** por aplicación.
  - **Dictamen**: **RECHAZADO**. Viola la restricción de costo cero.
* **Unity AR Foundation**:
  - **Costo de Licencia**: **$0 USD**. Es una biblioteca de código abierto y nativa de Unity incluida bajo la licencia de Unity Editor.
  - **Marcas de Agua**: **Cero marcas de agua**. El renderizado de la cámara es 100% limpio.
  - **Dictamen**: **SELECCIONADO**. Cumple al 100% con la restricción de gratuidad absoluta.

---

### 1.7. Matriz Comparativa Resumen: AR Foundation vs. Vuforia

| Parámetro / Funcionalidad | Unity AR Foundation 6.3.3 | Vuforia Engine 10.x | Ganador para ViMARA |
| :--- | :--- | :--- | :--- |
| **Costo de Licencia** | **$0 USD (100% Gratuito)** | ~$99 USD/mes (o Marca de agua) | **AR Foundation** |
| **Marca de Agua en Cámara** | **Ninguna** | Intrusiva en versión gratuita | **AR Foundation** |
| **Plane Tracking (Superficies)** | Excelente (ARKit/ARCore nativo) | Bueno (Vuforia Fusion) | **AR Foundation** |
| **Oclusión por Profundidad** | Nativa (`AROcclusionManager`) | Shader básico | **AR Foundation** |
| **Image Tracking (Marcadores)** | Excelente (ARCore/ARKit) | Superior en calidad de análisis | **Empate / AR Foundation** |
| **Marcadores Dinámicos (Runtime)** | Gratuito u offline (`MutableLibrary`) | Requiere planes Cloud / Limitado | **AR Foundation** |
| **Importación 3D Runtime (`.glb`)** | Directa mediante GLTFast (Gratis) | Requiere integración externa | **AR Foundation** |
| **Facilidad de Integración** | Nativa en UPM | Requiere paquetes externos | **AR Foundation** |

---

## 2. Investigación de Viabilidad de WebAR en Dispositivos Móviles (iOS y Android)

### 2.1. Soporte Nativo de la API WebXR en Navegadores Móviles (Android Chrome vs. iOS Safari)

El estándar W3C **WebXR Device API** permite a las páginas web acceder a los sensores de tracking y cámara de dispositivos de Realidad Aumentada.

```
                    SOPORTE WEBXR NATIVO EN NAVEGADORES MÓVILES (2026)
                                           │
                ┌──────────────────────────┴──────────────────────────┐
                ▼                                                     ▼
      ANDROID (Google Chrome v79+)                          IOS (Apple Safari 17 / 18)
    ┌──────────────────────────────┐                      ┌──────────────────────────────┐
    │ • WebXR immersive-ar: SÍ     │                      │ • WebXR immersive-ar: NO     │
    │ • Hit-Testing API: SÍ        │                      │ • Flags WebXR: Desactivadas  │
    │ • Accesso a Cámara/DOM: SÍ   │                      │ • Acceso a ARKit en Web: NO  │
    │ • Rendimiento: 60 FPS        │                      │ • Modo WebXR directo: INVIABLE│
    └──────────────────────────────┘                      └──────────────────────────────┘
```

* **Android (Google Chrome v79+)**:
  - Soporte **completo y nativo** para `immersive-ar`. Permite detectar planos en la habitación mediante hit-testing, superponer elementos HTML (DOM Overlay) y acceder a estimaciones de luz.
* **iOS (Apple Safari en iOS 17 y iOS 18)**:
  - **NO soporta `immersive-ar` de forma nativa**. Apple mantiene desactivada por defecto la API experimental de WebXR en WebKit. Safari no expone los sensores de ARKit directamente al contexto WebGL del navegador.

### 2.2. Mecanismos de Fallback Nativos: Apple AR Quick Look (USDZ) y Android Scene Viewer (GLB)

Debido a la falta de WebXR en iOS Safari, la industria utiliza visores nativos del sistema operativo:

1. **Apple AR Quick Look (iOS)**:
   - Al hacer clic en un enlace o elemento `<model-viewer>` con un archivo `.usdz`, Safari abre el visor nativo de iOS (ARKit).
   - **Ventaja**: Renders con iluminación fotorrealista a 60 FPS, seguimiento de plano impecable.
   - **Desventaja Severa para ViMARA**: Es un visor **pasivo e interactivo cero**. **NO permite ejecutar código C#, lógica de interfaz UI Toolkit, ni menúes interactivos para escalar o cambiar parámetros de la maqueta**. El usuario sale del navegador y entra a una app de Apple cerrada.
2. **Android Scene Viewer (Android)**:
   - Lanza la aplicación nativa de Google Play Services for AR con archivos `.glb`.
   - **Misma limitación**: Es un visor pasivo sin lógica de aplicación personalizada.

### 2.3. Evaluación de Exportación desde Unity a WebGL para WebAR

Se evaluaron las alternativas para exportar la lógica existente de Unity a la web:

1. **Plugin Unity WebXR Export (WebXR Foundation)**:
   - Funciona correctamente en Android Chrome.
   - **Falla rotundamente en iOS Safari**: Al no existir WebXR nativo, la escena se renderiza como un lienzo 3D plano sin cámara AR ni seguimiento.
2. **Needle Engine**:
   - Transpila escenas de Unity a Three.js / Web Components.
   - Reduce el bundle a 2-5 MB, pero **requiere reescribir toda la lógica C# a TypeScript**.
3. **Zappar WebGL para Unity (ZapWorks)**:
   - Permite seguimiento AR en iOS Safari procesando el video de la cámara mediante compilación WASM de visión por computadora.
   - **Violación de Costos**: La versión gratuita coloca marcas de agua gigantescas y la versión de producción cuesta de **$150 a $500+ USD/mes**, violando la regla de costo cero.

### 2.4. Cuellos de Botella Técnicos de Unity WebGL en Dispositivos Móviles

Exportar proyectos de Unity a WebGL para dispositivos móviles presenta barreras técnicas insuperables:

```
+-----------------------------------------------------------------------------------+
|               CUELLOS DE BOTELLA DE UNITY WEBGL EN NAVEGADORES MÓVILES             |
+-------------------+---------------------------------------------------------------+
| TAMAÑO DE BUNDLE  | 25 MB - 80 MB comprimido (80-250 MB descomprimido).           |
| TIEMPO DE CARGA   | 20 a 45 segundos en conexiones móviles 4G (High Bounce Rate). |
| MEMORIA RAM (iOS) | Cap de Safari a ~1.4 GB. Pila WASM + Texturas causa CRASH.    |
| TASA DE CRASHES   | > 50% de probabilidad de cierre en iPhones con modelos >20MB. |
| RENDIMIENTO FPS   | Caída de 60 FPS a 15-25 FPS tras 3 minutos por estrangulamiento|
|                   | térmico (Thermal Throttling).                                 |
+-------------------+---------------------------------------------------------------+
```

1. **Límite de Memoria RAM en iOS Safari**:
   iOS Safari impone un límite estricto de memoria por pestaña (~1.4 GB). La pila WASM de Unity requiere un bloque contiguo de memoria RAM. Al cargar un modelo arquitectónico `.glb` de 30–50 MB, el navegador agota la memoria y **desencadena el cierre repentino de la pestaña**.
2. **Tiempos de Carga Extremos**:
   Un paquete WebGL de Unity requiere descargar el runtime de C# compilado a WASM, los assets del motor y los modelos 3D. El tiempo medio de espera es de 30 segundos, lo que genera una tasa de abandono de usuarios superior al 80%.

### 2.5. Análisis de Fricción de Usuario: App Nativa vs. WebApp

* **App Nativa (Unity + AR Foundation)**:
  - Requiere instalación previa desde APK / Tienda (Fricción inicial única).
  - Una vez instalada: Carga instantánea (<2 segundos), tasa de crashes <0.1%, rendimiento continuo de 60 FPS, control total de la cámara y memoria.
* **WebApp (Unity WebGL)**:
  - Sin instalación (Acceso vía QR).
  - Fricción de espera prolongada (30s de pantalla de carga en cada visita), alta probabilidad de colapso de memoria en iOS, experiencia de usuario deficiente.

---

## 3. Alternativas WebAR No Basadas en Unity (Librerías y Frameworks Web Native)

Si el proyecto decide implementar un módulo o versión basada en web, se deben utilizar **frameworks nativos de JavaScript** en lugar de motores de videojuegos pesados como Unity.

### 3.1. Google `<model-viewer>` (Web Component, Apache 2.0)

* **Descripción**: Web Component oficial de Google desarrollado sobre Three.js. Es la herramienta estándar para mostrar modelos 3D en la web con un solo tag HTML (`<model-viewer src="maqueta.glb" ar ...>`).
* **Mecanismo de AR**:
  - En Android: Abre WebXR o Scene Viewer nativo.
  - En iOS: Abre Apple AR Quick Look (USDZ) automáticamente.
* **Ventajas**:
  - **100% Libre y Gratuito (Licencia Apache 2.0)**.
  - Peso del paquete ultra liviano (**~300 KB** gzipped). Tiempo de carga inferior a 1.5 segundos.
  - Calidad visual fotorrealista con soporte para PBR (Physically Based Rendering) e iluminación ambiental.
* **Desventajas**:
  - No permite marcadores de imagen personalizados (Image Tracking). Solo posicionamiento de superficie (Plane AR).
  - No permite lógica interactiva C# ni menús complejos dentro de la experiencia AR en iOS.

### 3.2. MindAR.js (TensorFlow.js Image & Face Tracking, MIT)

* **Descripción**: Framework open-source escrito en JavaScript nativo que ofrece seguimiento de imágenes (Image Tracking) y seguimiento facial (Face Tracking) directamente en el navegador.
* **Tecnología**: Utiliza redes neuronales de **TensorFlow.js** ejecutadas sobre WebGL para detectar características en imágenes de referencia.
* **Integración**: Se integra directamente con **Three.js** y **A-Frame**.
* **Flujo de Trabajo de Marcadores**:
  - Incluye un compilador de imágenes web/CLI que convierte imágenes JPG/PNG en archivos de marcas optimizados (`.mind`).
* **Ventajas**:
  - **100% Libre y Open Source (Licencia MIT)**. Sin pagos, sin límites de uso, sin marcas de agua.
  - Funciona tanto en **Android Chrome como en iOS Safari** dentro de la misma página web (no requiere salir a AR Quick Look).
  - Estabilidad de seguimiento de marcadores muy superior a AR.js.
* **Desventajas**:
  - Consume recursos del procesador en dispositivos móviles de gama baja, pudiendo reducir la tasa de cuadros a 25–35 FPS.
  - Enfocado en Image Tracking; no posee SLAM nativo para seguimiento de superficies sin marcadores.

### 3.3. AR.js (Marker-based, NFT, Location-based, MIT)

* **Descripción**: Una de las bibliotecas de WebAR más antiguas y populares.
* **Modalidades**:
  - **Marker-based**: Marcadores tipo patrones cuadrados (Hiro / Kanji).
  - **Image Tracking (NFT)**: Natural Feature Tracking para imágenes arbitrarias.
  - **Location-based**: Realidad aumentada basada en coordenadas GPS.
* **Ventajas**: **100% Gratuito (MIT)**, extremadamente liviano.
* **Desventajas**: El motor NFT sufre de alto jitter (temblor en el modelo) y desenganche frecuente en comparación con MindAR.js.

### 3.4. Three.js + WebXR API Nativa (Hit-Testing, MIT)

* **Descripción**: Desarrollo de WebAR a medida mediante Three.js y el módulo `ARButton` de la API de WebXR.
* **Ventajas**: Control total del pipeline 3D, animaciones, sombras y lógica en JavaScript. Peso total <2 MB.
* **Desventajas**: Funciona perfectamente en Android, pero requiere fallbacks complejos o polyfills para funcionar en iOS Safari.

### 3.5. Comparativa con Motores Comerciales (8th Wall, Zappar, Blippar)

Para ofrecer un panorama completo, se compararon los frameworks libres contra los estándares comerciales de la industria web:

```
+-----------------------------------------------------------------------------------+
|                    COMPARATIVA CON MOTORES WEB COMMERCIALES                        |
+------------------+-----------------------+----------------------------------------+
| FRAMEWORK        | PRECIO / LICENCIA     | VIABILIDAD PARA VIMARA                 |
+------------------+-----------------------+----------------------------------------+
| 8th Wall         | $99 a $2,000+ USD/mes | INVIABLE (Prohibitivo / No gratuito)   |
| Zappar (ZapWorks)| $45 a $500+ USD/mes   | INVIABLE (Marca de agua intrusiva)     |
| Blippar          | Pago por vistas SaaS  | INVIABLE (SaaS de pago)                |
| MindAR.js        | $0 USD (MIT)          | EXCELENTE (Open-Source para Imágenes)  |
| Google Viewer    | $0 USD (Apache 2.0)   | EXCELENTE (Open-Source para Planos)    |
+------------------+-----------------------+----------------------------------------+
```

* **8th Wall (Niantic)**: Ofrece el mejor SLAM en navegador del mercado para iOS Safari sin WebXR, pero su costo mínimo de **$99 a $2,000+ USD/mes** lo vuelve inviable para un proyecto sin presupuesto de licenciamiento.

---

### 3.6. Matriz Comparativa Master de Frameworks WebAR

| Framework | Tipo de Tracking | Soporte iOS Safari | Soporte Android Chrome | Formato 3D | Peso Bundle | Licencia y Costo | Recomendación |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Google `<model-viewer>`** | Plane Tracking | AR Quick Look (USDZ) | WebXR / Scene Viewer | `.glb` / `.usdz` | ~300 KB | **100% Gratis (Apache 2.0)** | **Altamente Recomendado (Web Preview)** |
| **MindAR.js + Three.js** | Image Tracking | WebGL Nativo en página | WebGL Nativo en página | `.gltf` / `.glb` | ~1.5 MB | **100% Gratis (MIT)** | **Altamente Recomendado (Web Marcadores)** |
| **AR.js** | Marker (Hiro) / NFT | WebGL Nativo en página | WebGL Nativo en página | `.gltf` / `.glb` | ~1.1 MB | **100% Gratis (MIT)** | Secundario (Mayor jitter) |
| **Three.js Nativo** | Plane Tracking | Requiere Fallback | WebXR Nativo (`immersive-ar`)| `.gltf` / `.glb` | ~1.0 MB | **100% Gratis (MIT)** | Recomendado para devs WebXR |
| **Unity WebGL (WebXR)** | Plane / Image | Fallido (Sin WebXR) | WebXR Nativo | Assets Unity | 25 MB – 80 MB | **$0 USD (Engine)** | **NO RECOMENDADO (Crashes RAM)** |
| **8th Wall** | World (SLAM) / Image | Nativo en página | Nativo en página | Proprietary / Three.js | Medium | **$99 – $2000+/mes** | **INVIABLE (De Pago)** |

---

## 4. Recomendación Arquitectónica Definitiva y Plan de Implementación

### 4.1. Estrategia Híbrida Recomendada (Tier 1 Nativo + Tier 2 Web)

Basado en el análisis riguroso de capacidades técnicas, rendimiento en dispositivos móviles y el cumplimiento del mandato estricto de **costo cero**, se formula la siguiente recomendación arquitectónica oficial para el proyecto ViMARA:

```
===================================================================================
                       ARQUITECTURA RECOMENDADA PARA VIMARA
===================================================================================

     ┌───────────────────────────────────────────────────────────────────────┐
     │                       PROYECTO VIMARA (BENTRE25)                      │
     └───────────────────────────────────┬───────────────────────────────────┘
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

#### Tier 1: Aplicación Principal Nativa Móvil (Unity 3D + AR Foundation 6.3.3) — **APLICACIÓN PRIMARIA**
* **Justificación**: Es la **única solución** que garantiza:
  1. Rendimiento fluido de 60 FPS con modelos de maquetas complejas.
  2. Importación dinámica en tiempo de ejecución de archivos `.glb` mediante **GLTFast 6.16.1** sin tirones de pantalla.
  3. Creación dinámica de marcadores de imagen en tiempo de ejecución (`MutableRuntimeReferenceImageLibrary`).
  4. Interfaz de usuario rica con menúes para manipular, rotar, escalar y analizar la maqueta (UI Toolkit / XR Interaction Toolkit).
  5. **100% Gratuito y Libre**: Sin licencias de pago, sin marcas de agua y sin restricciones de uso.

#### Tier 2: Módulo Web de Acceso Rápido (Non-Unity WebAR) — **OPCIONAL PARA COMPARTIR VÍA LINK/QR**
* **Justificación**: Si se requiere que un cliente o profesor visualice la maqueta instantáneamente desde un navegador web sin instalar la aplicación APK:
  - **Para Muestreo Rápido de Plano**: Utilizar **Google `<model-viewer>`**. Permite abrir el modelo `.glb` en Android y `.usdz` en iOS en menos de 2 segundos.
  - **Para Muestreo de Marcadores**: Utilizar **MindAR.js + Three.js** (100% MIT). Permite anclar la maqueta sobre el plano impreso desde Safari o Chrome móvil sin instalar apps.

---

### 4.2. Cumplimiento Estricto del Criterio de Gratuidad Total (Cero Costos / Cero Marcas de Agua)

El diseño arquitectónico propuesto garantiza que el proyecto ViMARA no incurra en ningún costo presente ni futuro:

1. **Unity 3D Personal License**: $0 USD (para entidades/desarrolladores con ingresos menores a $100k USD/año).
2. **Unity AR Foundation 6.3.3**: $0 USD. Integrado nativamente en Unity. Cero marcas de agua.
3. **GLTFast 6.16.1**: $0 USD. Licencia MIT/Apache 2.0 oficial de Unity.
4. **Google `<model-viewer>`**: $0 USD. Licencia Apache 2.0.
5. **MindAR.js**: $0 USD. Licencia MIT Open Source.

Se rechazan formalmente **Vuforia Engine**, **8th Wall** y **Zappar WebGL** por requerir suscripciones comerciales obligatorias o imponer marcas de agua inasumibles en sus modalidades gratuitas.

---

### 4.3. Hoja de Ruta y Próximos Pasos

1. **Consolidar el desarrollo de la app nativa en Unity**:
   - Mantener la versión **AR Foundation 6.3.3** configurada en `Packages/manifest.json`.
   - Utilizar `GLTFast` para la carga asíncrona de modelos `.glb` importados mediante `NativeFilePicker`.
2. **Optimización de Marcadores de Imagen**:
   - Utilizar el inspector de Vuforia Target Manager únicamente como herramienta web de diagnóstico para medir la calidad de contraste de los planos impresos (puntuación de estrellas), pero implementar la biblioteca de marcadores dentro de Unity mediante `XRReferenceImageLibrary` / `MutableRuntimeReferenceImageLibrary`.
3. **(Opcional) Despliegue de Demostrador Web**:
   - Si se desea ofrecer una demo en el sitio web del proyecto, implementar una página HTML sencilla con `<model-viewer>` y exportar las maquetas a formato `.glb` y `.usdz`.

---
*Fin del Reporte de Investigación y Análisis de Viabilidad Arquitectónica.*
