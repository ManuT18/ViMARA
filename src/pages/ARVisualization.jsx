import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import UnityARViewer from '../components/UnityARViewer';
import { ArrowLeft, LogOut, Target, Layers, Box, Info } from 'lucide-react';

export default function ARVisualization() {
  const navigate = useNavigate();
  const { currentModel, trackingMode, resetSelection } = useApp();

  const handleBackToImport = () => {
    navigate('/model-import');
  };

  const handleExitApp = () => {
    resetSelection();
    navigate('/');
  };

  const modeLabel = trackingMode === 'marker' 
    ? 'Seguimiento por Marcador' 
    : 'Seguimiento por Plano';

  // Fallback if accessed directly without model
  if (!currentModel || !currentModel.url) {
    return (
      <div 
        className="animate-fade-in"
        style={{
          maxWidth: '600px',
          margin: '60px auto',
          padding: '32px',
          textAlign: 'center',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '20px'
        }}
      >
        <div className="glass-panel" style={{ padding: '36px', width: '100%' }}>
          <Box size={48} color="var(--accent-primary)" style={{ marginBottom: '16px' }} />
          <h3 style={{ fontSize: '1.4rem', fontWeight: '600', marginBottom: '8px' }}>
            No hay modelo 3D seleccionado
          </h3>
          <p style={{ fontSize: '0.9rem', color: 'var(--text-secondary)', marginBottom: '24px' }}>
            Por favor regresa al paso de importación para seleccionar o cargar una maqueta 3D.
          </p>
          <button className="btn-primary" onClick={handleBackToImport}>
            <ArrowLeft size={18} />
            <span>Ir a Seleccionar Modelo</span>
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="ar-visualization-page animate-fade-in">
      {/* Top Header & Navigation Bar */}
      <div className="page-top-bar">
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
          <button className="page-back-btn" onClick={handleBackToImport}>
            <ArrowLeft size={16} />
            <span>Volver a Modelo</span>
          </button>

          {/* Model Info Badge */}
          <div className={`page-mode-badge ${trackingMode === 'marker' ? 'marker' : 'plane'}`}>
            {trackingMode === 'marker' ? <Target size={14} /> : <Layers size={14} />}
            <span>Modo: <strong>{trackingMode === 'marker' ? 'Marcador' : 'Plano'}</strong></span>
          </div>
        </div>

        {/* Exit Button */}
        <button 
          className="page-back-btn" 
          onClick={handleExitApp}
          style={{ borderColor: 'rgba(239, 68, 68, 0.3)', color: '#ef4444' }}
        >
          <LogOut size={16} />
          <span>Salir</span>
        </button>
      </div>

      {/* Main 3D / WebAR View Component */}
      <main style={{ width: '100%' }}>
        <UnityARViewer trackingMode={trackingMode} />
      </main>

      {/* Footer Info Badge */}
      <div className="glass-panel" style={{ padding: '16px 24px', display: 'flex', alignItems: 'center', justifyContent: 'space-between', fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Info size={18} color="var(--accent-primary)" />
          <span>Visualizando maqueta en visor tridimensional interactivo con soporte WebAR en tiempo real.</span>
        </div>
        <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
          ID Modelo: <code>{currentModel.id}</code>
        </span>
      </div>
    </div>
  );
}
