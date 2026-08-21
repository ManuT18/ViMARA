# Informe de Evolución Arquitectónica y Tecnológica: Proyecto ViMARA

**Proyecto:** ViMARA (Visualizador de Maquetas de Arquitectura en Realidad Aumentada)  
**Marco:** Beca BENTRE25 / Departamento de Ciencias e Ingeniería de la Computación (DCIC)  
**Fecha de Actualización:** Agosto 2026  
**Estado:** Documento de Definición Final  

---

## 1. Introducción y Objetivos del Proyecto

El proyecto **ViMARA** nace bajo el marco de la beca BENTRE25 y el DCIC con un objetivo fundamental: **democratizar la visualización de modelos 3D y maquetas arquitectónicas mediante Realidad Aumentada (AR)**. 

El problema central identificado es que la presentación tradicional de maquetas (físicas o en pantallas 2D limitadas) restringe la comprensión espacial del diseño. Al mismo tiempo, las soluciones actuales de Realidad Aumentada en el mercado suelen requerir la instalación de aplicaciones nativas pesadas, creando una "barrera de fricción" alta para docentes, estudiantes y clientes finales.

Por lo tanto, el objetivo arquitectónico principal de ViMARA se definió como la creación de una plataforma **Zero-Friction**: una experiencia que se ejecute directamente desde el navegador web de cualquier teléfono inteligente (WebAR), combinando fluidez estética con un motor de renderizado 3D robusto capaz de interpretar modelos arquitectónicos a escala real.

---

## 2. Evaluación de Motores de Realidad Aumentada (El recorrido AR)

La búsqueda del motor de Realidad Aumentada adecuado requirió atravesar distintas etapas y evaluaciones tecnológicas:

### 2.1. El Enfoque Nativo (Vuforia y AR Foundation)
Inicialmente, el proyecto se concibió bajo el ecosistema tradicional de desarrollo de videojuegos usando el motor **Unity**. Se evaluaron las dos herramientas líderes:
- **AR Foundation (ARCore para Android / ARKit para iOS):** Excelente rendimiento nativo y detección de planos (suelos y mesas). Sin embargo, exportar esto a la web es técnicamente inviable, forzando la creación de dos aplicaciones nativas (una para la App Store y otra para la Play Store).
- **Vuforia:** Reconocido por su poderoso seguimiento de imágenes (Image Tracking, ideal para apuntar el teléfono a un plano impreso y levantar la maqueta). El problema de Vuforia radica en sus altos costos de licenciamiento comercial y la restricción a aplicaciones instalables.

**Conclusión de la Etapa 1:** Desarrollar una App Nativa limitaría el alcance de ViMARA. Se tomó la decisión estratégica de migrar hacia **WebAR** (Realidad Aumentada en el Navegador).

### 2.2. El Salto a WebAR y la Llegada a Zapworks
En el ecosistema web, las opciones open-source (como MindAR.js o AR.js) ofrecían una puerta de entrada gratuita, pero su rendimiento de seguimiento, estabilidad de cámara y capacidad de procesamiento de mallas 3D complejas en celulares de gama media presentaban deficiencias notorias.

Tras la recomendación de los directores del proyecto, se analizó el ecosistema **Zapworks** y su **Universal AR SDK**. Esta tecnología resultó ser un punto de inflexión. Zappar desarrolló motores de visión por computadora en C++ de alta eficiencia, compilados a WebAssembly. Esto permite que el seguimiento de imágenes (marcadores) y de planos físicos dentro del navegador web funcione con una estabilidad que compite directamente con las aplicaciones nativas. 

---

## 3. Estandarización de Formatos de Archivos 3D

Dado que el flujo de trabajo de los usuarios proviene de software arquitectónico (como SketchUp, Revit o AutoCAD), se definió una política estricta de estandarización de formatos para garantizar que la WebApp no colapse por problemas de memoria gráfica:

1. **`.glb` / `.gltf` (Formato Principal):** Designado como el formato oficial de ViMARA. Es un estándar binario diseñado específicamente para la web. Comprime mallas eficientemente y encapsula las texturas y materiales (PBR) en un único archivo ligero.
2. **`.stl` (Geometría / Maqueta Blanca):** Un formato común en impresión 3D. Se soporta en ViMARA para cargar estudios volumétricos y maquetas conceptuales "en blanco", ya que contiene únicamente la geometría de la estructura, logrando tiempos de carga instantáneos.
3. **`.obj` (Formato Tradicional):** Soportado como medida de retrocompatibilidad, aunque se desaconseja su uso frente al `.glb` debido a su naturaleza de texto plano (pesado de parsear) y el manejo fragmentado de materiales (archivos `.mtl`).

---

## 4. La Encrucijada Arquitectónica: Las Tres Alternativas

Al establecer que WebAR y Zapworks eran el camino a seguir, el equipo se enfrentó a un dilema de desarrollo y diseño sobre cómo construir la aplicación final. Se evaluaron tres alternativas reales:

### Alternativa 1: 100% Unity (Web Build Tradicional)
Consistía en desarrollar tanto la interfaz de usuario (menús, botones) como el visor 3D dentro del editor Unity y exportar todo como un único bloque WebGL.
*   **Ventaja:** Alta familiaridad para el desarrollador de videojuegos, uso del editor visual (drag-and-drop).
*   **Desventaja:** La interfaz de usuario (UI) creada en Unity WebGL suele sentirse pesada, lenta de cargar, no se adapta orgánicamente a pantallas móviles (responsive) y carece del estilo moderno y limpio de una aplicación web real.

### Alternativa 2: 100% Desarrollo Web (React Puro)
Consistía en abandonar Unity por completo y programar toda la aplicación (desde el diseño hasta el renderizado de gráficos 3D) en código JavaScript utilizando **React** y bibliotecas como **Three.js**.
*   **Ventaja:** Una aplicación extremadamente rápida, de estética "Premium" (mobile-first), con pesos de carga menores a 2 MB y latencia cero.
*   **Desventaja:** Requiere programar la lógica matemática del renderizado 3D y la iluminación a mano, perdiendo las facilidades visuales del entorno de Unity.

### Alternativa 3: El Enfoque Híbrido (React + Unity Embebido)
Esta alternativa plantea la separación de responsabilidades: usar **React** exclusivamente para construir la Interfaz de Usuario (la "Cáscara"), y utilizar **Unity WebGL** exclusivamente como el visor de Realidad Aumentada (el "Motor Interno").

---

## 5. La Solución Adoptada: El Enfoque Híbrido con Zapworks

Tras sopesar la estética de la aplicación, el rendimiento en dispositivos móviles y el tiempo de desarrollo, **se determinó avanzar formalmente con la Alternativa 3 (El Enfoque Híbrido)**.

### ¿Cómo funciona la arquitectura adoptada?
1. **La "Cáscara" Premium (React + Vite):** Todo el flujo de navegación previo a la Realidad Aumentada (la Pantalla de Bienvenida, la Selección del Modo de Seguimiento y la Importación del Archivo 3D) fue desarrollado con React 19. Esto garantiza que el estudiante o cliente experimente una navegación fluida, diseño adaptable (mobile-first), temas claros modernos e interacciones instantáneas propias de una app de alto nivel.
2. **El "Motor Interno" (Unity WebGL + Zapworks):** La lógica de renderizado del modelo 3D y la interacción espacial de Realidad Aumentada permanecen en **Unity**, aprovechando el SDK `Universal AR for Unity` de Zapworks. La exportación WebGL de este módulo está altamente optimizada gracias a las plantillas de compresión de Zappar.
3. **Comunicación (El Puente):** Para integrar ambas partes, la WebApp de React utiliza una biblioteca puente (`react-unity-webgl`) que incrusta el motor de Unity en la página de visualización final. React toma los comandos del usuario (ej. "cargar archivo .glb") y se los transmite directamente al motor de Unity mediante comunicación bidireccional asíncrona, permitiendo que Unity renderice el entorno AR y el modelo arquitectónico seleccionado.

### Beneficios del Enfoque Híbrido:
*   **Lo mejor de ambos mundos:** Se mantienen las ventajas visuales del ecosistema web moderno (React) sin perder el poder gráfico de un motor de videojuegos líder en la industria (Unity).
*   **Sin servidor (Serverless / MVP):** Toda esta arquitectura opera directamente en el navegador del dispositivo del usuario (Client-Side). Para esta primera fase MVP no es necesario un servidor Backend costoso. Los archivos 3D seleccionados se manejan localmente en la memoria, asegurando cero consumo de datos innecesario y total privacidad de los proyectos.

---

## 6. Conclusión y Próximos Pasos

La evolución de la arquitectura de **ViMARA** demuestra una maduración técnica enfocada estrictamente en el usuario final. Al descartar la obligatoriedad de instalar aplicaciones nativas y adoptar un entorno **WebAR Híbrido con Zapworks**, el proyecto se asegura de brindar una herramienta académica y profesional altamente accesible.

El flujo de trabajo híbrido no solo facilita el proceso de programación gráfica mediante Unity, sino que preserva la estética, accesibilidad y rapidez de una aplicación web de vanguardia. 

**Próximos Pasos:**
1. Desarrollar y perfeccionar la escena de Realidad Aumentada dentro de Unity integrando el SDK de Zapworks.
2. Exportar el módulo WebGL optimizado desde Unity.
3. Incrustar el módulo exportado dentro del ecosistema React actual (`vimara-3d.vercel.app`) y establecer los puentes de comunicación JavaScript-C# para la carga dinámica de las maquetas.
