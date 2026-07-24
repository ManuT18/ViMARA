import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import InfoModal from '../components/InfoModal';
import { ArrowLeft, Target, Layers, Info, CheckCircle2, ChevronRight } from 'lucide-react';

export default function ModeSelection() {
  const navigate = useNavigate();
  const { trackingMode, setTrackingMode } = useApp();
  const [isInfoModalOpen, setIsInfoModalOpen] = useState(false);

  const handleSelectMode = (mode) => {
    setTrackingMode(mode);
    navigate('/model-import');
  };

  const handleBackToMenu = () => {
    navigate('/');
  };

  return (
    <div
      className="animate-fade-in"
      style={{
        maxWidth: '900px',
        margin: '0 auto',
        padding: '32px 24px',
        display: 'flex',
        flexDirection: 'column',
        gap: '32px'
      }}
    >
      {/* Top Header & Navigation */}
      <div className="mode-selection-header">
        <button className="btn-secondary nav-action-btn" onClick={handleBackToMenu}>
          <ArrowLeft size={18} />
          <span>Volver al Menú</span>
        </button>

        <button 
          className="btn-secondary nav-action-btn help-btn" 
          onClick={() => setIsInfoModalOpen(true)}
        >
          <Info size={18} />
          <span>¿Necesitás Ayuda?</span>
        </button>
      </div>

      {/* Step Title */}
      <div style={{ textAlign: 'center' }}>
        <span style={{ fontSize: '0.85rem', color: 'var(--accent-primary)', fontWeight: '600', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          Paso 1 de 3 · Configuración
        </span>
        <h2 style={{ fontSize: '2rem', fontWeight: '700', marginTop: '4px' }}>
          Selecciona el Modo de Realidad Aumentada
        </h2>
        <p style={{ fontSize: '0.95rem', color: 'var(--text-secondary)', marginTop: '8px' }}>
          Elige cómo deseas posicionar la maqueta 3D en tu espacio físico.
        </p>
      </div>

      {/* Mode Selection Cards */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
        gap: '24px'
      }}>
        {/* Option 1: Seguimiento por Marcador */}
        <div
          onClick={() => handleSelectMode('marker')}
          className="glass-card"
          style={{
            padding: '32px 24px',
            cursor: 'pointer',
            display: 'flex',
            flexDirection: 'column',
            gap: '16px',
            position: 'relative',
            border: trackingMode === 'marker' ? '2px solid var(--accent-primary)' : '1px solid var(--border-glass)',
            background: trackingMode === 'marker' ? 'rgba(59, 130, 246, 0.12)' : 'rgba(255, 255, 255, 0.03)'
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{
              width: '52px',
              height: '52px',
              borderRadius: '14px',
              background: 'rgba(59, 130, 246, 0.2)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--accent-primary)'
            }}>
              <Target size={28} />
            </div>

            {trackingMode === 'marker' && (
              <span style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#4ade80', fontSize: '0.85rem', fontWeight: '600' }}>
                <CheckCircle2 size={16} /> Seleccionado
              </span>
            )}
          </div>

          <div>
            <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '8px' }}>
              Seguimiento por Marcador
            </h3>
            <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)', lineHeight: 1.5 }}>
              Proyecta la maqueta anclada sobre un plano impreso, imagen 2D o código QR de referencia. Ideal para presentaciones de planos.
            </p>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: 'var(--accent-primary)', fontSize: '0.9rem', fontWeight: '600', marginTop: 'auto' }}>
            <span>Seleccionar este modo</span>
            <ChevronRight size={18} />
          </div>
        </div>

        {/* Option 2: Seguimiento por Plano */}
        <div
          onClick={() => handleSelectMode('plane')}
          className="glass-card"
          style={{
            padding: '32px 24px',
            cursor: 'pointer',
            display: 'flex',
            flexDirection: 'column',
            gap: '16px',
            position: 'relative',
            border: trackingMode === 'plane' ? '2px solid #8b5cf6' : '1px solid var(--border-glass)',
            background: trackingMode === 'plane' ? 'rgba(139, 92, 246, 0.12)' : 'rgba(255, 255, 255, 0.03)'
          }}
        >
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <div style={{
              width: '52px',
              height: '52px',
              borderRadius: '14px',
              background: 'rgba(139, 92, 246, 0.2)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: '#a855f7'
            }}>
              <Layers size={28} />
            </div>

            {trackingMode === 'plane' && (
              <span style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#4ade80', fontSize: '0.85rem', fontWeight: '600' }}>
                <CheckCircle2 size={16} /> Seleccionado
              </span>
            )}
          </div>

          <div>
            <h3 style={{ fontSize: '1.25rem', fontWeight: '600', marginBottom: '8px' }}>
              Seguimiento por Plano
            </h3>
            <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)', lineHeight: 1.5 }}>
              Detecta automáticamente superficies planas (suelos o mesas) y coloca la maqueta 3D en escala realista sin necesidad de marcador.
            </p>
          </div>

          <div style={{ display: 'flex', alignItems: 'center', gap: '6px', color: '#a855f7', fontSize: '0.9rem', fontWeight: '600', marginTop: 'auto' }}>
            <span>Seleccionar este modo</span>
            <ChevronRight size={18} />
          </div>
        </div>
      </div>

      {/* Info Modal Component */}
      <InfoModal 
        isOpen={isInfoModalOpen} 
        onClose={() => setIsInfoModalOpen(false)} 
      />
    </div>
  );
}
