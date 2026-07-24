# Original User Request

## 2026-07-24T04:51:54Z

El objetivo es realizar una investigación exhaustiva y comparativa entre AR Foundation y Vuforia, y evaluar la viabilidad técnica de migrar o exportar el proyecto actual a una WebApp (WebAR) para lograr compatibilidad multiplataforma en dispositivos móviles (iOS y Android). El resultado será un reporte en Markdown con recomendaciones claras.

Working directory: ~/teamwork_projects/ar_webapp_analysis
Integrity mode: development

## Requirements

### R1. Comparativa AR Foundation vs Vuforia
Analizar y comparar ambas herramientas. El enfoque debe estar en las versiones actuales (2023-2024), facilidad de configuración, capacidades de Plane Tracking y fundamentalmente **Image Tracking (marcadores)** para lograr mayor estabilidad. También evaluar la facilidad para importar modelos dinámicamente en tiempo de ejecución. 
**Restricción Estricta:** Se debe analizar en detalle el esquema de costos y licencias. El proyecto exige que la herramienta elegida sea completamente libre y gratuita, sin necesidad de pagar licencias (ej. evaluar si Vuforia impone marcas de agua inasumibles o licencias de pago para producción).

### R2. Viabilidad de WebAR
Investigar si es posible crear una WebApp con capacidades de Realidad Aumentada (WebXR/WebAR) que funcione correctamente en navegadores de iOS y Android. No es strictly necesario que la solución provenga de exportar desde Unity (WebGL); se pueden considerar otros motores u enfoques de desarrollo diferentes si Unity no es la mejor herramienta para este fin.

### R3. Alternativas WebAR
Dado que se pueden considerar otros enfoques fuera de Unity, investigar y recomendar frameworks o librerías alternativas (por ejemplo, Model-viewer, Three.js, 8th Wall) que permitan cumplir los objetivos del proyecto (AR en Web con Image y Plane tracking), siempre teniendo en cuenta la restricción de que sean herramientas gratuitas/open source.

## Acceptance Criteria

### Evaluación del Reporte de Análisis
- [ ] El reporte contiene una tabla comparativa clara entre AR Foundation y Vuforia que incluya soporte de marcadores y limitaciones de licencias gratuitas.
- [ ] El reporte establece explícitamente cuáles son las opciones para desarrollo WebAR (sea desde Unity u otros motores/frameworks).
- [ ] El reporte incluye alternativas no basadas en Unity con una descripción de sus ventajas, desventajas y costos (resaltando opciones 100% gratuitas).
- [ ] El documento final termina con una recomendación arquitectónica definitiva teniendo en cuenta que se busca gratuidad total y compatibilidad web/móvil.

## 2026-07-24T02:36:21-03:00

El objetivo es realizar un análisis técnico sobre dos grandes desafíos de infraestructura para el proyecto ViMARA (desarrollado en Unity). Primero, evaluar la complejidad y opciones reales para compilar y exportar la aplicación hacia iOS trabajando exclusivamente desde un entorno Windows, priorizando soluciones gratuitas o de muy baja fricción. Segundo, analizar la complejidad técnica y el esfuerzo real requerido para desarrollar la aplicación completamente en Unity y luego "migrarla" hacia un entorno Web (WebAR) una vez finalizada.

Working directory: ~/teamwork_projects/unity_ios_web_migration

## Requirements

### R1. Exportación a iOS desde Windows
Investigar las alternativas actuales (2024) para compilar un proyecto Unity a iOS sin poseer una Mac local. Evaluar opciones como Unity Cloud Build, GitHub Actions con macOS runners, o servicios de terceros. 
**Restricción Estricta:** Se debe enfatizar la fricción (facilidad de uso) y el costo (buscando opciones 100% gratuitas). Detallar los pasos necesarios para la firma de la app (Apple Developer Program) y si es obligatorio pagar los $99/año de Apple para probarla en un dispositivo físico.

### R2. Análisis de Migración Unity -> Web
Investigar la complejidad técnica de construir la aplicación completa en Unity (C#) y luego migrar el código y la lógica a una solución WebAR nativa (como Three.js / HTML / JS). 
**Enfoque:** Aclarar si esto es una "traducción" automática, una "exportación", o si en realidad implica reescribir toda la aplicación desde cero en otro lenguaje.

## Acceptance Criteria

### Evaluación del Reporte
- [ ] El reporte detalla al menos 2 métodos para compilar para iOS desde Windows.
- [ ] El reporte establece explícitamente los costos ocultos (ej. licencias de Apple, costos de Cloud Build).
- [ ] El reporte aclara de forma contundente (sí/no) si desarrollar en Unity y luego "migrar" a la Web significa tener que reescribir todo el código.

## 2026-07-24T05:59:04Z

El objetivo es investigar y definir la lista de estándares de archivos 3D óptimos para el proyecto WebAR ViMARA, considerando que el flujo de trabajo principal de los usuarios se origina en software de modelado 3D (priorizando **SketchUp**, pero incluyendo otros del rubro). La meta es ofrecer variedad y gran compatibilidad (soportando múltiples formatos clave) sin perjudicar el rendimiento web.

Working directory: ~/teamwork_projects/3d_format_analysis
Integrity mode: development

## Requirements

### R1. Análisis de Exportación (SketchUp + Alternativas)
Investigar los formatos de exportación 3D que ofrece **SketchUp** (versiones web gratuitas y Pro de escritorio) y al menos otros dos o tres programas clave del rubro arquitectura/diseño (ej. AutoCAD, Revit, Blender, Rhino). Identificar qué formatos se exportan nativamente y cuáles requieren plugins adicionales.

### R2. Comparativa Técnica de Formatos
Analizar técnicamente los formatos exportables más comunes (`.glb` / `.gltf`, `.stl`, `.obj`, `.fbx`, `.dae`, etc.) teniendo en cuenta el contexto de una WebApp móvil. La evaluación debe considerar:
1. **Fase 1 del proyecto:** Carga de geometría base (maquetas sin texturas). Peso del archivo y rapidez de carga en el navegador.
2. **Fase 2 del proyecto (Escalabilidad):** Capacidad del formato para soportar texturas y materiales complejos en el futuro.
3. **Soporte WebAR:** Compatibilidad y facilidad de parseo en librerías web como Three.js y Google `<model-viewer>`.

### R3. Definición del Abanico de Formatos Compatibles
Proponer una **lista curada y reducida de formatos (ej. 2 a 4 tipos de archivos)** que la aplicación ViMARA deba soportar de forma oficial. Se debe buscar el equilibrio perfecto entre ofrecer gran compatibilidad a los usuarios de diferentes softwares y mantener la infraestructura de la aplicación robusta y optimizada.

## Acceptance Criteria

### Evaluación del Reporte
- [ ] El reporte lista claramente los formatos que SketchUp y otros softwares líderes pueden exportar.
- [ ] El reporte compara directamente `.stl`, `.obj` y `.glb`/`.gltf` frente al soporte de texturas futuras, peso web y soporte de Three.js / model-viewer.
- [ ] El documento finaliza con un veredicto recomendando una pequeña lista de formatos oficiales para ViMARA, justificando por qué brindan la mejor compatibilidad general.
