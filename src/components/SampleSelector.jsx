import React from 'react';
import { Building2, Box, Home } from 'lucide-react';

const SAMPLES = [
  {
    id: 'sample-helmet',
    name: 'Casco Futurista (PBR / Texturas)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb',
    description: 'Demostración de iluminación y texturas de alta calidad.'
  },
  {
    id: 'sample-chair',
    name: 'Mueble de Diseño (Interiorismo)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/SheenChair/glTF-Binary/SheenChair.glb',
    description: 'Ejemplo de escala realista para arquitectura de interiores.'
  },
  {
    id: 'sample-astronaut',
    name: 'Figura de Prueba 3D',
    format: 'glb',
    url: 'https://modelviewer.dev/shared-assets/models/Astronaut.glb',
    description: 'Modelo clásico de prueba para WebAR en exteriores.'
  }
];

export default function SampleSelector({ onSelectSample, activeSampleId }) {
  return (
    <div className="glass-panel" style={{ padding: '24px' }}>
      <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '4px' }}>
        Maquetas de Prueba Instantáneas
      </h3>
      <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginBottom: '16px' }}>
        Toca cualquier ejemplo para probar la Realidad Aumentada sin subir un archivo:
      </p>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
        {SAMPLES.map((sample) => {
          const isActive = activeSampleId === sample.id;
          return (
            <div
              key={sample.id}
              onClick={() => onSelectSample(sample)}
              className="glass-card"
              style={{
                padding: '14px 18px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                borderColor: isActive ? 'var(--accent-primary)' : 'rgba(255,255,255,0.08)',
                background: isActive ? 'rgba(59, 130, 246, 0.12)' : 'rgba(255,255,255,0.03)'
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
                <div style={{
                  width: '36px',
                  height: '36px',
                  borderRadius: '8px',
                  background: isActive ? 'var(--accent-gradient)' : 'rgba(255,255,255,0.06)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: isActive ? '#fff' : 'var(--text-secondary)'
                }}>
                  <Building2 size={18} />
                </div>
                <div>
                  <h4 style={{ fontSize: '0.9rem', fontWeight: '600', color: isActive ? '#fff' : 'var(--text-primary)' }}>
                    {sample.name}
                  </h4>
                  <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                    {sample.description}
                  </p>
                </div>
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
}
