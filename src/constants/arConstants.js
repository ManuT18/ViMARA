/**
 * Constantes globales y catálogo de modelos de prueba para ViMARA WebAR.
 */

export const TRACKING_MODES = Object.freeze({
  PLANE: 'plane',
  MARKER: 'marker',
});

export const SUPPORTED_EXTENSIONS = Object.freeze(['glb', 'gltf', 'stl', 'obj']);

export const DEFAULT_MODEL = Object.freeze({
  id: 'sample-house',
  name: 'Casa Moderna con Piscina (Maqueta)',
  format: 'preset',
  url: 'preset:house',
});

export const SAMPLES = Object.freeze([
  {
    ...DEFAULT_MODEL,
    badge: 'Instantáneo',
    description:
      'Vivienda contemporánea de dos plantas con voladizo, terraza de madera y piscina.',
  },
  {
    id: 'sample-tower',
    name: 'Torre Residencial Urbana (Maqueta)',
    format: 'preset',
    url: 'preset:tower',
    badge: 'Instantáneo',
    description:
      'Edificio en altura con basamento comercial, núcleo acristalado y balcones alternados.',
  },
  {
    id: 'sample-pavilion',
    name: 'Pabellón de Exposición y Espejo de Agua',
    format: 'preset',
    url: 'preset:pavilion',
    badge: 'Instantáneo',
    description:
      'Espacio arquitectónico abierto con columnata perimetral y pérgola de madera.',
  },
  {
    id: 'sample-chair',
    name: 'Silla de Diseño Texturizada (.GLB)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/SheenChair/glTF-Binary/SheenChair.glb',
    badge: 'GLB PBR',
    description: 'Mobiliario de interiorismo con materiales PBR realistas.',
  },
  {
    id: 'sample-helmet',
    name: 'Casco Sci-Fi PBR (.GLB)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb',
    badge: 'GLB PBR',
    description:
      'Modelo de prueba de alta fidelidad para reflejos y texturas metálicas.',
  },
]);

export const FORMAT_RECOMMENDATIONS = Object.freeze([
  {
    ext: '.GLB / .GLTF',
    description:
      'Formato recomendado para la mejor fidelidad en WebAR, materiales PBR y texturas integradas.',
  },
  {
    ext: '.STL',
    description:
      'Recomendado para prototipos de malla y maquetas físicas sin textura.',
  },
  {
    ext: '.OBJ',
    description:
      'Soporte para geometría básica exportada desde SketchUp o Blender.',
  },
]);
