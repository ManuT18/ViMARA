import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import ModelViewer from '../components/ModelViewer';
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
    <div
      className="animate-fade-in"
      style={{
        maxWidth: '1280px',
        margin: '0 auto',
        padding: '24px',
        display: 'flex',
        flexDirection: 'column',
        gap: '20px'
      }}
    >
      {/* Top Header & Navigation Bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '12px' }}>
        <div style={{ display: 'flex', gap: '12px', alignItems: 'center' }}>
          <button className="btn-secondary" onClick={handleBackToImport}>
            <ArrowLeft size={18} />
            <span>Volver a Importar Modelo</span>
          </button>

          {/* Model Info Badge */}
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '8px',
            padding: '6px 14px',
            borderRadius: '20px',
            background: 'rgba(255, 255, 255, 0.05)',
            border: '1px solid var(--border-glass)',
            fontSize: '0.85rem',
            color: 'var(--text-primary)'
          }}>
            {trackingMode === 'marker' ? <Target size={15} color="#60a5fa" /> : <Layers size={15} color="#c084fc" />}
            <span>Modo: <strong>{modeLabel}</strong></span>
          </div>
        </div>

        {/* Exit Button */}
        <button 
          className="btn-secondary" 
          onClick={handleExitApp}
          style={{ borderColor: 'rgba(239, 68, 68, 0.4)', color: '#f87171' }}
        >
          <LogOut size={18} />
          <span>Salir de la App</span>
        </button>
      </div>

      {/* Main 3D / WebAR View Component */}
      <main style={{ width: '100%' }}>
        <ModelViewer 
          modelUrl={currentModel.url}
          modelName={currentModel.name}
          format={currentModel.format}
        />
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
