import React from 'react';
import { Building2, CheckCircle2 } from 'lucide-react';

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
    <div className="glass-panel" style={{ padding: '20px' }}>
      <h3 style={{ fontSize: '1.05rem', fontWeight: '700', marginBottom: '4px', color: 'var(--text-primary)' }}>
        Maquetas de Prueba Instantáneas
      </h3>
      <p style={{ fontSize: '0.82rem', color: 'var(--text-muted)', marginBottom: '14px', lineHeight: 1.4 }}>
        Toca cualquier ejemplo para probar la Realidad Aumentada sin subir un archivo:
      </p>

      <div style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
        {SAMPLES.map((sample) => {
          const isActive = activeSampleId === sample.id;
          return (
            <div
              key={sample.id}
              onClick={() => onSelectSample(sample)}
              className="glass-card touch-card"
              style={{
                padding: '12px 16px',
                minHeight: '56px',
                cursor: 'pointer',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'space-between',
                borderColor: isActive ? 'var(--accent-primary)' : 'var(--border-light)',
                background: isActive ? 'rgba(37, 99, 235, 0.06)' : '#ffffff',
                boxShadow: isActive ? '0 4px 12px rgba(37, 99, 235, 0.12)' : 'var(--shadow-sm)',
                borderRadius: 'var(--radius-lg)',
                userSelect: 'none'
              }}
            >
              <div style={{ display: 'flex', alignItems: 'center', gap: '12px', flex: 1 }}>
                <div style={{
                  width: '40px',
                  height: '40px',
                  borderRadius: 'var(--radius-md)',
                  background: isActive ? 'var(--accent-gradient)' : 'var(--bg-tertiary)',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: isActive ? '#ffffff' : 'var(--text-secondary)',
                  flexShrink: 0
                }}>
                  <Building2 size={20} />
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
                    <h4 style={{
                      fontSize: '0.92rem',
                      fontWeight: '700',
                      color: isActive ? 'var(--accent-primary)' : 'var(--text-primary)'
                    }}>
                      {sample.name}
                    </h4>
                    {isActive && <CheckCircle2 size={16} color="var(--accent-primary)" />}
                  </div>
                  <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)', lineHeight: 1.3 }}>
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
