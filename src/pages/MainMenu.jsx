import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Globe, Layers, Building2 } from 'lucide-react';

export default function MainMenu() {
  const navigate = useNavigate();

  const handleEnterApp = () => {
    navigate('/mode-selection');
  };

  return (
    <div className="main-menu-page animate-fade-in">

      {/* Main Title & Hero Tagline */}
      <div style={{ maxWidth: '850px' }}>
        <h1 className="hero-title">
          Visualizador de Maquetas de Arquitectura en <br />
          <span className="gradient-text">Realidad Aumentada</span>
        </h1>

        <p className="hero-tagline">
          Proyecta modelos tridimensionales arquitectónicos a escala real directamente desde tu navegador web
        </p>
      </div>

      {/* Action Button: "Iniciar App" */}
      <div style={{ display: 'flex', gap: '16px', flexWrap: 'wrap', justifyContent: 'center' }}>
        <button 
          className="btn-primary" 
          onClick={handleEnterApp}
          style={{
            fontSize: '1.1rem',
            padding: '16px 36px',
            borderRadius: '14px'
          }}
        >
          <span>Iniciar App</span>
          <ArrowRight size={22} />
        </button>
      </div>

      {/* Value Proposition Cards */}
      <div className="main-features-grid">
        <div className="main-feature-card">
          <div style={{
            width: '48px',
            height: '48px',
            borderRadius: '12px',
            background: 'rgba(59, 130, 246, 0.15)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: 'var(--accent-primary)',
            marginBottom: '16px'
          }}>
            <Globe size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            WebAR Sin Instalaciones
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Visualización inmersiva directa en el navegador (Safari / Chrome) sin requerir descargas ni instalación de apps externas.
          </p>
        </div>

        <div className="main-feature-card">
          <div style={{
            width: '48px',
            height: '48px',
            borderRadius: '12px',
            background: 'rgba(139, 92, 246, 0.15)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#a855f7',
            marginBottom: '16px'
          }}>
            <Layers size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            Seguimiento Instantáneo
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Algoritmos de visión computacional para fijar maquetas tridimensionales en superficies reales con alta estabilidad.
          </p>
        </div>

        <div className="main-feature-card">
          <div style={{
            width: '48px',
            height: '48px',
            borderRadius: '12px',
            background: 'rgba(236, 72, 153, 0.15)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#ec4899',
            marginBottom: '16px'
          }}>
            <Building2 size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            Formatos 3D estandar
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Preparado para inspeccionar volumetrías, proporciones espaciales y modelos exportados desde Revit, SketchUp, Rhino o Blender.
          </p>
        </div>
      </div>
    </div>
  );
}
