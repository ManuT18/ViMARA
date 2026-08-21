import React from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, Box, Smartphone, Layers, Sparkles } from 'lucide-react';

export default function MainMenu() {
  const navigate = useNavigate();

  const handleEnterApp = () => {
    navigate('/mode-selection');
  };

  return (
    <div 
      className="animate-fade-in"
      style={{
        maxWidth: '1100px',
        margin: '0 auto',
        padding: '40px 24px',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: '40px',
        textAlign: 'center'
      }}
    >
      {/* Hero Badge */}
      <div style={{
        display: 'inline-flex',
        alignItems: 'center',
        gap: '8px',
        padding: '8px 18px',
        borderRadius: '30px',
        background: 'rgba(59, 130, 246, 0.12)',
        border: '1px solid rgba(59, 130, 246, 0.3)',
        color: '#60a5fa',
        fontSize: '0.88rem',
        fontWeight: '500'
      }}>
        <Sparkles size={16} />
        <span>Plataforma WebAR para Arquitectura</span>
      </div>

      {/* Main Title & Hero Tagline */}
      <div style={{ maxWidth: '850px' }}>
        <h1 style={{
          fontSize: '3.2rem',
          fontWeight: '800',
          lineHeight: 1.15,
          letterSpacing: '-0.03em',
          marginBottom: '20px'
        }}>
          Visualizador de Maquetas de Arquitectura en <br />
          <span className="gradient-text">Realidad Aumentada</span>
        </h1>

        <p style={{
          fontSize: '1.18rem',
          color: 'var(--text-secondary)',
          lineHeight: 1.6,
          fontWeight: '400'
        }}>
          Proyecta modelos tridimensionales arquitectónicos a escala real directamente desde tu navegador web. Sin instalaciones pesadas ni plugins externos.
        </p>
      </div>

      {/* Action Button: "Entrar a la App" */}
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
          <span>Entrar a la App</span>
          <ArrowRight size={22} />
        </button>
      </div>

      {/* Value Proposition Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(280px, 1fr))',
        gap: '24px',
        width: '100%',
        marginTop: '20px'
      }}>
        <div className="glass-panel" style={{ padding: '28px', textAlign: 'left' }}>
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
            <Layers size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            Dos Modos de AR
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Soporta seguimiento por marcador para planos físicos y detección de planos horizontales (suelo/mesas).
          </p>
        </div>

        <div className="glass-panel" style={{ padding: '28px', textAlign: 'left' }}>
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
            <Box size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            Formatos 3D Estándar
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Carga fácilmente maquetas en formatos <code>.glb</code>, <code>.gltf</code>, <code>.stl</code> y <code>.obj</code>, exportando facilmente desde SketchUp o Revit.
          </p>
        </div>

        <div className="glass-panel" style={{ padding: '28px', textAlign: 'left' }}>
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
            <Smartphone size={24} />
          </div>
          <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '8px' }}>
            Nativo en iOS y Android
          </h3>
          <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)' }}>
            Compatible con varios sistemas operativos móviles como Android e iOS. Utiliza librerías de AR como Apple AR Quick Look y Google Scene Viewer para una experiencia de Realidad Aumentada fluida.
          </p>
        </div>
      </div>
    </div>
  );
}
