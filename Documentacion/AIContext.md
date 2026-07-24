# Contexto del Proyecto: ViMARA

## Estado General
**ViMARA** (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es un proyecto universitario (beca BENTRE25). El objetivo es visualizar maquetas de arquitectura, exportadas desde software como **SketchUp**, en Realidad Aumentada (AR) a escala 1:1 o personalizada.

El enfoque arquitectónico del proyecto está pivotando de una aplicación nativa en Unity hacia una **WebApp (WebAR)** para maximizar la compatibilidad multiplataforma (iOS y Android sin necesidad de Mac ni complicadas instalaciones) y reducir la fricción para el usuario final.

## Requerimientos y Flujo de Usuario
- **Origen de los modelos:** Los estudiantes de arquitectura exportarán sus modelos desde SketchUp y los enviarán a sus teléfonos móviles.
- **Carga dinámica:** La aplicación debe permitir al usuario subir/cargar estos archivos directamente desde el almacenamiento local del celular en tiempo de ejecución.
- **Fases del Modelo:**
  1. Fase inicial: Maquetas blancas/grises sin texturas, geometría simple.
  2. Fase futura: Soporte para texturas y materiales detallados.
- **Formatos:** Se priorizará **.glb** (estándar óptimo para Web y AR) y se evaluará el soporte para **.stl** (común en geometría sin textura).
- **Fase Futura (Backend):** Base de datos pública/catálogo comunitario donde los estudiantes puedan compartir y visualizar maquetas de otros.

## Arquitectura Tecnológica (En Definición)
- **Descartado (Temporal o Definitivamente):** Unity3D. Se ha comprobado que exportar a iOS desde Windows implica alta fricción (servicios Cloud, cuentas de Apple) y migrar el proyecto a Web a posteriori requeriría reescribir el 100% del código en otro lenguaje.
- **Ruta Recomendada (WebAR):**
  - **Frontend:** HTML, CSS, JavaScript (Framework UI por definir, ej. React/Vite o Vanilla puro).
  - **Motor AR:** `<model-viewer>` para Plane Tracking nativo (AR Quick Look en iOS, Scene Viewer en Android) y **MindAR.js + Three.js** para Image Tracking (Marcadores) en el navegador.

## Tareas Completadas (Recientes)
- [x] Análisis comparativo AR Foundation vs Vuforia.
- [x] Evaluación de viabilidad WebAR desde Unity (Descartado).
- [x] Levantamiento de requerimientos (Fase 1 y 2): Definición del flujo de SketchUp y carga local.
- [x] Análisis de infraestructura: Se determinó que desarrollar en Unity y luego ir a Web implica reescribir el 100% del código fuente.

## Próximos Pasos (TODO)
- [ ] Tomar decisión final oficial: ¿Abandonamos Unity para crear la WebApp?
- [ ] Analizar capacidades de exportación de `.glb` desde SketchUp.
- [ ] Definir estructura del proyecto WebApp (Framework UI).
