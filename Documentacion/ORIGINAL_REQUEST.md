# Original User Request

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
