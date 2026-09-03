# Informe Técnico de Arquitectura y Decisiones de Diseño: Proyecto ViMARA

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Marco Institucional:** Beca BENTRE25 — Departamento de Ciencias e Ingeniería de la Computación (DCIC)  
**Fecha:** Agosto de 2026  
**Autor:** Manuel T.  
**Estado:** Documento de Definición Técnica

---

## 1. Introducción y Contexto del Proyecto

El proyecto ViMARA surge en el marco de la beca BENTRE25 con el propósito de facilitar la visualización e inspección espacial de maquetas arquitectónicas mediante tecnologías de Realidad Aumentada (AR).

En la práctica arquitectónica y académica, la evaluación de proyectos suele depender de maquetas físicas (costosas y difíciles de transportar) o de representaciones en pantallas bidimensionales que limitan la comprensión de escala y proporciones. Si bien existen soluciones de Realidad Aumentada, la gran mayoría exige la instalación de aplicaciones nativas pesadas en las tiendas móviles, lo cual genera una fricción considerable para usuarios esporádicos, docentes o alumnos.

El objetivo central planteado para ViMARA fue diseñar una solución accesible directamente desde el navegador web, eliminando el requisito de instalación previa y garantizando tiempos de respuesta adecuados en dispositivos móviles.

---

## 2. Recorrido Tecnológico y Evaluación de Motores AR

Durante las primeras etapas del proyecto, se evaluaron diferentes enfoques para el desarrollo de la experiencia de Realidad Aumentada:

### 2.1. Pruebas Iniciales en Entornos Nativos (Unity, AR Foundation y Vuforia)

La primera aproximación se desarrolló sobre el motor **Unity**, explorando dos alternativas clásicas de la industria:

- **AR Foundation (ARKit / ARCore):** Presenta un rendimiento óptimo en seguimiento de superficies planas (_Plane Tracking_), pero está restringido al empaquetado nativo para iOS y Android. No cuenta con soporte directo para despliegues web estándar.

- **Vuforia Engine:** Resulta eficaz para el seguimiento basado en marcadores impresos (_Image Tracking_), ideal para superponer modelos sobre planos en papel. No obstante, además de exigir una aplicación instalable, su esquema de licenciamiento comercial impone barreras para un proyecto de desarrollo abierto o académico.

Estas limitaciones confirmaron la necesidad de orientar la arquitectura hacia una WebApp, evitando forzar al usuario a descargar aplicaciones desde las tiendas de software.

### 2.2. Consideraciones de la Migración a WebApp (React y Three.js)

El paso hacia el navegador implicó definir un stack tecnológico capaz de ofrecer una interfaz fluida:

- **Interfaz de Usuario:** Se seleccionó **React** junto con **Vite** como entorno de construcción. Esto permitió reemplazar el sistema de interfaz gráfica de Unity (cuyos elementos _Canvas_ resultan rígidos y pesados en la web) por componentes web nativos, adaptables a pantallas táctiles (_mobile-first_) y con tiempos de carga prácticamente instantáneos.

- **Renderizado 3D en Navegador:** Se evaluó el uso de **Three.js** y el componente **`<model-viewer>`** de Google para gestionar la carga de modelos directamente sobre el DOM.

### 2.3. Dificultades de Implementación en WebAR Puro y Selección de Zapworks

Al profundizar en el diseño técnico del visor, se observó que desarrollar toda la lógica espacial, la calibración de cámara y el anclaje de Realidad Aumentada únicamente con código JavaScript y librerías abiertas (como MindAR.js o Three.js puro) implicaba una complejidad matemática y de desarrollo excesiva. Implementar transformaciones de matrices, iluminación dinámica y manejo de mallas a mano aumentaba los tiempos de desarrollo y el riesgo de inestabilidad en el seguimiento.

A partir de consultas con la dirección del proyecto, se evaluó la integración de **Zapworks** mediante su **Universal AR SDK**. Zapworks proporciona algoritmos de visión por computadora compilados a WebAssembly (Wasm), lo que permite ejecutar seguimiento de imágenes y de planos con alta precisión en el navegador. Principalmente, su SDK para Unity permite mantener la lógica de AR y el visor dentro del entorno de Unity (programando en C#), facilitando una exportación WebGL optimizada que puede integrarse en la web.

---

## 3. Manejo y Estandarización de Archivos 3D

Para asegurar que los modelos generados en software de diseño (como SketchUp, Revit, Rhino o Blender) puedan visualizarse sin degradar el rendimiento del navegador móvil, se definieron los siguientes formatos de soporte:

1. **`.glb` / `.gltf` (Estándar Principal):** Es el formato de referencia para el proyecto. Almacena la geometría de la malla junto con texturas y materiales PBR en un único contenedor binario comprimido, minimizando el consumo de memoria en el teléfono.
2. **`.stl` (Maquetas Volumétricas):** Formato estándar en impresión 3D. Contiene únicamente geometría sin información de color o textura, lo que permite cargas muy rápidas para el análisis de volumetrías o "maquetas blancas".
3. **`.obj` (Compatibilidad Básica):** Se mantiene soporte básico por su amplia difusión, aunque se desaconseja para modelos de alta densidad debido a su mayor peso relativo y la fragmentación de sus archivos de materiales (`.mtl`).

---

## 4. Comparativa de Alternativas Arquitectónicas

Con los requerimientos y herramientas analizadas, se compararon tres caminos posibles para la estructura del sistema:

| Alternativa                        | Descripción                                                                                                               | Ventajas                                                                                                 | Desventajas                                                                                         |
| :--------------------------------- | :------------------------------------------------------------------------------------------------------------------------ | :------------------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------- |
| **1. 100% Unity WebGL**            | Toda la aplicación (menús, botones y visor AR) se construye en Unity y se exporta a WebGL.                                | Entorno de desarrollo unificado en C# y herramientas visuales integradas.                                | Interfaz poco responsive, estética poco adaptada a móviles y mayor peso inicial de descarga.        |
| **2. 100% Web (React + Three.js)** | Toda la interfaz y la lógica de renderizado AR se programan en JavaScript/React.                                          | Carga extremadamente rápida, interfaz moderna y bajo consumo de recursos.                                | Alta complejidad para implementar la matemática de tracking y renderizado AR sin un motor dedicado. |
| **3. Arquitectura Híbrida**        | React gestiona el flujo de usuario y la interfaz; Unity WebGL (vía Zapworks) actúa como motor embebido para la escena AR. | Combina una interfaz web rápida y responsive con la robustez gráfica y facilidad de desarrollo de Unity. | Requiere configurar un puente de comunicación entre JavaScript y C#.                                |

---

## 5. Arquitectura Seleccionada: Enfoque Híbrido

Se resolvió adoptar la **Alternativa 3 (Enfoque Híbrido)** como la solución que mejor equilibra calidad de experiencia de usuario y viabilidad de desarrollo.

```
[ Usuario en Dispositivo Móvil ]
            │
            ▼
┌────────────────────────────────────────────────────────┐
│  Capa de Interfaz y Navegación (React + Vite)          │
│  - Pantalla Principal / Bienvenida                     │
│  - Selector de Modo AR (Plano / Marcador)              │
│  - Gestor de Archivos Locales (.glb, .stl, .obj)       │
└──────────────────────────┬─────────────────────────────┘
                           │
            [ react-unity-webgl Bridge ]
                           │
                           ▼
┌────────────────────────────────────────────────────────┐
│  Módulo de Visualización y AR (Unity WebGL + Zapworks) │
│  - Renderizado de Escena 3D e Iluminación              │
│  - Algoritmos de Visión y Tracking (Zapworks Wasm)     │
│  - Manipulación espacial del modelo arquitectónico     │
└────────────────────────────────────────────────────────┘
```

### Componentes del Sistema:

1. **Frontend Web (React + Vite):** Se encarga de la navegación, la selección de modos de seguimiento y la carga de archivos. Al ejecutarse directamente sobre el DOM, garantiza una interfaz limpia, adaptable a distintas pantallas y con controles táctiles naturales.
2. **Contenedor WebGL (Unity + Zapworks):** En la vista de visualización, se instancia el visor compilado de Unity. Este componente se activa únicamente cuando se inicia la experiencia AR, aprovechando la aceleración por hardware para renderizar el modelo y procesar el tracking de cámara provisto por Zapworks.
3. **Comunicación Bidireccional:** La interoperabilidad se resuelve mediante `react-unity-webgl`, permitiendo que React envíe los datos del modelo cargado (o parámetros de configuración) hacia las funciones internas de Unity mediante llamadas asíncronas.
4. **Operación Local (Client-Side):** Para esta etapa, la aplicación opera de forma completamente local en el navegador del dispositivo. No se requiere un backend para procesar o almacenar los modelos, lo que reduce costos de infraestructura a cero y preserva la privacidad de los proyectos de los estudiantes.

---

## 6. Conclusiones y Estado Actual

La adopción de una arquitectura híbrida permite resolver los dos requisitos centrales de ViMARA: brindar un acceso inmediato y estéticamente cuidado desde la web, sin renunciar a la capacidad de procesamiento 3D y estabilidad de seguimiento que ofrece un motor como Unity con el soporte de Zapworks.

**Estado y tareas inmediatas:**

1. Concluir el diseño de la interfaz web en React con soporte responsive.
2. Configurar la escena de tracking en Unity utilizando el paquete _Universal AR for Unity_ de Zapworks.
3. Integrar la compilación WebGL dentro de la aplicación React y validar el paso de parámetros y modelos 3D en dispositivos móviles reales sobre el despliegue en Vercel.
