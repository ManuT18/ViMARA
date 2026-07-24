# Contexto del Proyecto: ViMARA

Este archivo sirve como punto de partida para que cualquier agente de IA entienda el estado y propósito del proyecto al iniciar una nueva conversación.

## Descripción General
**ViMARA** (Visualizador de Maquetas de Arquitectura en Realidad Aumentada) es el proyecto final de carrera universitario. Consiste en una aplicación móvil desarrollada en Unity para visualizar modelos y maquetas arquitectónicas en tamaño real o a escala utilizando Realidad Aumentada (AR).

## Características Core
- **Modo Plano Libre:** Detección de superficies (Plane Tracking) para posicionar modelos 3D a escala 1:1 o personalizada en el entorno real.
- **Modo Marcador Fijo:** Image Tracking para anclar y superponer las maquetas sobre planos o marcadores físicos impresos.
- **Importación Dinámica de Modelos:** El sistema debe permitir cargar modelos `.gltf` y `.glb` directamente desde el almacenamiento del celular en tiempo de ejecución (limitado preferentemente a estos formatos para mantener texturas y materiales).

## Arquitectura y Tecnologías
- **Motor Engine:** Unity 3D
- **Framework de AR:** AR Foundation, XR Interaction Toolkit
- **Interfaz de Usuario (UI):** UI Toolkit de Unity (`.uxml`, `.uss`), para una experiencia fluida e integraciones modernas.
- **Plataforma Objetivo:** Android (Mobile AR Template configurado en Portrait). Futuro potencial: iOS.
- **Lenguaje:** C#

## Estado Actual (Roadmap)
El proyecto se divide en fases de madurez:
1. Interfaz básica y flujos de navegación (Menús UI Toolkit).
2. Interacción espacial (Mover, Rotar, Escalar en AR).
3. Visualización con Image Tracking.
4. Importador dinámico de archivos desde el almacenamiento.

**Directriz Principal:** Este es un proyecto universitario final (BENTRE25), por lo que el código debe ser estructurado, mantenible y bien documentado, prestando especial atención a la experiencia de usuario y al rendimiento gráfico en dispositivos móviles.
