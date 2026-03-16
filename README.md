# ViMARA
**Visualizador de Maquetas de Arquitectura en Realidad Aumentada**

![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Android](https://img.shields.io/badge/Android-3DDC84?style=for-the-badge&logo=android&logoColor=white)
![AR Foundation](https://img.shields.io/badge/AR_Foundation-000000?style=for-the-badge&logo=unity&logoColor=white)

ViMARA es una aplicación móvil desarrollada en Unity, diseñada para la visualización interactiva de modelos y maquetas de arquitectura en tamaño real o a escala utilizando Realidad Aumentada (AR). La aplicación busca facilitar la previsualización de proyectos arquitectónicos en entornos del mundo real y ayudar al alumnado de carreras universitarias en preentregas y/o entregas finales de proyectos de las diferentes materias de la carrera.

## 🚀 Características Principales (En Desarrollo)
- **Modo Plano Libre**: Permite al usuario detectar superficies en el mundo real (como mesas o el suelo) para posicionar modelos arquitectónicos a escala 1:1.
- **Modo Marcador Fijo**: Posicionamiento anclado a imágenes físicas (Image Tracking) para superponer maquetas sobre marcadores fijos.
  - **Idea actual:** Único marcador fijo descargable desde la aplicación.
  - **Idea futura 1:** Carga de marcadores desde el almacenamiento del dispositivo móvil.
  - **Idea futura 2:** Detección de marcadores (plano de la maqueta) en tiempo real (Image Tracking).
- **Importador de Modelos**: Capacidad de buscar y cargar modelos 3D (`.gltf`, `.glb`) directamente desde el almacenamiento del dispositivo móvil. Se intentará limitar el sistema a este tipo de archivos para evitar problemas con los materiales y las texturas, pero se dará una guia de qué ofrece cada archivo y se permitirá la subida de archivos .obj o .stl.
- **Interfaz Moderna**: Navegación fluida y responsiva utilizando el sistema UI Toolkit de Unity.

## 🛠️ Tecnologías Utilizadas
- **Motor Gráfico**: [Unity 3D](https://unity.com/) 
- **Realidad Aumentada**: AR Foundation, XR Interaction Toolkit
- **Interfaz de Usuario (UI)**: UI Toolkit (`.uxml`, `.uss`)
- **Lenguaje de Programación**: C#

## 📱 Plataformas Soportadas
- Desarrollado principalmente para dispositivos móviles (Android / iOS). 
*(Nota: Actualmente configurado con el Mobile AR Template para Android y resolución en orientación Portrait. Aún se analiza la posibilidad de integracion en iOS)*

## 📋 Fases actuales del Proyecto (Roadmap)
1. **Fase 1:** Interfaz de usuario básica y flujos de navegación (Menú principal, Selección de modo).
2. **Fase 2:** Visualización AR en Plano (Plane Tracker) e interacción espacial (Mover, Rotar, Escalar).
3. **Fase 3:** Visualización AR con Marcador (Image Tracker).
4. **Fase 4:** Integración de un cargador dinámico de archivos 3D (`.gltf`/`.glb`) desde el dispositivo.

---
*Proyecto desarrollado con el apoyo de la beca universitaria BENTRE25.*
