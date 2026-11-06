import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Globe, Layers, Building2 } from 'lucide-react';

const FEATURES = [
  {
    icon: Globe,
    iconBg: 'rgba(59, 130, 246, 0.15)',
    iconColor: 'var(--accent-primary)',
    title: 'WebAR Sin Instalaciones',
    description:
      'Visualización inmersiva directa en el navegador (Safari / Chrome) sin requerir descargas ni instalación de apps externas.',
  },
  {
    icon: Layers,
    iconBg: 'rgba(139, 92, 246, 0.15)',
    iconColor: '#a855f7',
    title: 'Seguimiento Instantáneo',
    description:
      'Algoritmos de visión computacional para fijar maquetas tridimensionales en superficies reales con alta estabilidad.',
  },
  {
    icon: Building2,
    iconBg: 'rgba(236, 72, 153, 0.15)',
    iconColor: '#ec4899',
    title: 'Formatos 3D Estándar',
    description:
      'Preparado para inspeccionar volumetrías, proporciones espaciales y modelos exportados desde Revit, SketchUp, Rhino o Blender.',
  },
];

export default function MainMenu() {
  const navigate = useNavigate();

  return (
    <div className="main-menu-page animate-fade-in">
      <div style={{ maxWidth: '850px' }}>
        <h1 className="hero-title">
          Visualizador de Maquetas de Arquitectura en <br />
          <span className="gradient-text">Realidad Aumentada</span>
        </h1>

        <p className="hero-tagline">
          Proyecta modelos tridimensionales arquitectónicos a escala real directamente desde tu navegador web
        </p>
      </div>

      <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap', justifyContent: 'center' }}>
        <button
          className="btn-primary"
          onClick={() => navigate('/mode-selection')}
          style={{
            fontSize: '1.1rem',
            padding: '16px 36px',
            borderRadius: '14px',
          }}
        >
          <span>Iniciar App</span>
          <ArrowRight size={22} />
        </button>
      </div>

      <div className="main-features-grid">
        {FEATURES.map((feat) => {
          const Icon = feat.icon;
          return (
            <div key={feat.title} className="main-feature-card">
              <div
                className="feature-icon-box"
                style={{ background: feat.iconBg, color: feat.iconColor }}
              >
                <Icon size={24} />
              </div>
              <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
                {feat.title}
              </h3>
              <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
                {feat.description}
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
}
