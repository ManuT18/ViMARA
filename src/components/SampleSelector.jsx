import React, { useState } from 'react';
import { Building2, CheckCircle2, ChevronDown, ChevronUp, Sparkles, Box } from 'lucide-react';

export const SAMPLES = [
  {
    id: 'sample-house',
    name: 'Casa Moderna con Piscina (Maqueta)',
    format: 'preset',
    url: 'preset:house',
    badge: 'Instantáneo',
    description: 'Vivienda contemporánea de dos plantas con voladizo, terraza de madera y piscina.'
  },
  {
    id: 'sample-tower',
    name: 'Torre Residencial Urbana (Maqueta)',
    format: 'preset',
    url: 'preset:tower',
    badge: 'Instantáneo',
    description: 'Edificio en altura con basamento comercial, núcleo acristalado y balcones alternados.'
  },
  {
    id: 'sample-pavilion',
    name: 'Pabellón de Exposición y Espejo de Agua',
    format: 'preset',
    url: 'preset:pavilion',
    badge: 'Instantáneo',
    description: 'Espacio arquitectónico abierto con columnata perimetral y pérgola de madera.'
  },
  {
    id: 'sample-chair',
    name: 'Silla de Diseño Texturizada (.GLB)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/SheenChair/glTF-Binary/SheenChair.glb',
    badge: 'GLB PBR',
    description: 'Mobiliario de interiorismo con materiales PBR realistas.'
  },
  {
    id: 'sample-helmet',
    name: 'Casco Sci-Fi PBR (.GLB)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb',
    badge: 'GLB PBR',
    description: 'Modelo de prueba de alta fidelidad para reflejos y texturas metálicas.'
  }
];

export default function SampleSelector({ onSelectSample, activeSampleId }) {
  const [isOpen, setIsOpen] = useState(true);

  return (
    <div className="glass-panel" style={{ padding: '20px' }}>
      <div 
        style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', cursor: 'pointer', userSelect: 'none' }}
        onClick={() => setIsOpen(!isOpen)}
      >
        <div>
          <h3 style={{ fontSize: '1.05rem', fontWeight: '700', marginBottom: '2px', color: 'var(--text-primary)' }}>
            Modelos y Maquetas de Prueba
          </h3>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
            Selecciona un prototipo arquitectónico o modelo 3D listo para proyectar
          </p>
        </div>
        <div style={{ color: 'var(--text-secondary)' }}>
          {isOpen ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
        </div>
      </div>

      {isOpen && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginTop: '16px' }}>
          {SAMPLES.map((sample) => {
            const isActive = activeSampleId === sample.id;
            const isPreset = sample.format === 'preset';
            return (
              <div
                key={sample.id}
                onClick={() => onSelectSample(sample)}
                className="glass-card touch-card"
                style={{
                  padding: '12px 16px',
                  minHeight: '60px',
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
                    {isPreset ? <Building2 size={20} /> : <Box size={20} />}
                  </div>
                  <div style={{ flex: 1 }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '6px', flexWrap: 'wrap' }}>
                      <h4 style={{
                        fontSize: '0.92rem',
                        fontWeight: '700',
                        color: isActive ? 'var(--accent-primary)' : 'var(--text-primary)'
                      }}>
                        {sample.name}
                      </h4>
                      {sample.badge && (
                        <span style={{
                          fontSize: '0.68rem',
                          fontWeight: '600',
                          padding: '2px 7px',
                          borderRadius: '999px',
                          background: isPreset ? 'rgba(16, 185, 129, 0.12)' : 'rgba(59, 130, 246, 0.12)',
                          color: isPreset ? '#059669' : '#2563eb',
                          display: 'inline-flex',
                          alignItems: 'center',
                          gap: '3px'
                        }}>
                          {isPreset && <Sparkles size={10} />}
                          {sample.badge}
                        </span>
                      )}
                      {isActive && <CheckCircle2 size={16} color="var(--accent-primary)" />}
                    </div>
                    <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)', lineHeight: 1.3, marginTop: '2px' }}>
                      {sample.description}
                    </p>
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
