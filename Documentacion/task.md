# Task List: Plan de Desarrollo Funcional ViMARA

**Objetivo:** Desarrollar e integrar de forma incremental el núcleo funcional de Realidad Aumentada (WebAR) utilizando Unity, el SDK de Zapworks y la interfaz web en React.

---

## Fase 1: "Hello World" de AR con Zapworks en Unity (Local)
> **Meta:** Comprobar que el tracking de Zapworks funciona en Unity con un modelo estático de prueba.

- [ ] **1.1.** Crear una cuenta gratuita en [Zapworks](https://zap.works) y obtener Workspace/Project para WebAR.
- [ ] **1.2.** Instalar el paquete **Universal AR SDK for Unity** en el proyecto `Legacy_Unity` (vía Unity Package Manager con la URL git de Zappar).
- [ ] **1.3.** Crear una escena limpia en Unity (`Zapworks_AR_Scene`).
- [ ] **1.4.** Configurar una cámara Zappar (`Zappar Camera`) y un objetivo de prueba simple:
  - *Opción A (Image Tracking):* Un marcador impreso (plano o imagen de prueba) con un cubo 3D anclado.
  - *Opción B (Instant/Plane Tracking):* Colocar un cubo 3D sobre una superficie plana detectada.
- [ ] **1.5.** Probar la escena dentro del Unity Editor con la webcam de la PC.

---

## Fase 2: Primera Exportación WebGL y Validación en Celular
> **Meta:** Lograr que la escena de Unity corra dentro de un navegador móvil real y active la cámara.

- [ ] **2.1.** Cambiar la plataforma de Unity a **WebGL** en *Build Settings*.
- [ ] **2.2.** Seleccionar el *WebGL Template* oficial de Zappar en `Project Settings > Player`.
- [ ] **2.3.** Realizar la primera compilación WebGL a una carpeta local de prueba (`Build_Test`).
- [ ] **2.4.** Probar el build en local o servirlo con soporte HTTPS (requerido para acceder a la cámara en móviles).
- [ ] **2.5.** Abrir el enlace en el teléfono móvil, conceder permisos de cámara y verificar el anclaje del objeto 3D en el mundo real.

---

## Fase 3: Carga Dinámica de Modelos 3D en Runtime
> **Meta:** Permitir que Unity descargue, procese e instancie dinámicamente un archivo 3D (`.glb`) en lugar de un objeto fijo.

- [ ] **3.1.** Incorporar en Unity una librería de importación glTF en tiempo de ejecución (ej. `glTFast`).
- [ ] **3.2.** Crear un script en C# (`ModelLoaderBridge.cs`) con un método receptor:
  `public void LoadModelFromURL(string url)`.
- [ ] **3.3.** Probar en Unity que, dada una URL de una maqueta `.glb`, el modelo se descargue, se instancie y se coloque en el ancla de AR correspondiente.

---

## Fase 4: Conexión con la WebApp de React (El Puente)
> **Meta:** Conectar la interfaz web de React con el visor funcional de Unity WebGL.

- [ ] **4.1.** Instalar la dependencia `react-unity-webgl` en el proyecto React (`npm install react-unity-webgl`).
- [ ] **4.2.** Mover los archivos de salida de Unity WebGL (`.data`, `.wasm`, `.framework.js`) a la carpeta pública de React (`public/unity-build/`).
- [ ] **4.3.** Actualizar `src/pages/ARVisualization.jsx` para instanciar el canvas WebGL de Unity embebido.
- [ ] **4.4.** Conectar la interacción: cuando el usuario seleccione un modelo en `ModelImport.jsx`, React envía la orden a Unity vía `sendMessage("ModelLoaderBridge", "LoadModelFromURL", modelUrl)`.
- [ ] **4.5.** Desplegar en Vercel y validar el flujo completo de punta a punta desde el navegador del celular.
