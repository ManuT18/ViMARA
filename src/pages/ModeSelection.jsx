import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import { TRACKING_MODES } from '../constants/arConstants';
import InfoModal from '../components/InfoModal';
import { ArrowLeft, Target, Layers, Info, CheckCircle2, ChevronRight } from 'lucide-react';

const MODE_OPTIONS = [
  {
    id: TRACKING_MODES.MARKER,
    title: 'Seguimiento por Marcador',
    description:
      'Proyecta el modelo anclado sobre un plano impreso, imagen 2D o código QR.\nIdeal para presentaciones de planos.',
    icon: Target,
    iconBg: 'rgba(59, 130, 246, 0.2)',
    accentColor: 'var(--accent-primary)',
    selectedClass: 'selected-marker',
  },
  {
    id: TRACKING_MODES.PLANE,
    title: 'Seguimiento por Plano',
    description:
      'Detecta automáticamente superficies planas y coloca el modelo 3D sin marcador.\nIdeal para entornos amplios.',
    icon: Layers,
    iconBg: 'rgba(139, 92, 246, 0.2)',
    accentColor: '#a855f7',
    selectedClass: 'selected-plane',
  },
];

export default function ModeSelection() {
  const navigate = useNavigate();
  const { trackingMode, setTrackingMode } = useApp();
  const [isInfoModalOpen, setIsInfoModalOpen] = useState(false);

  const handleSelectMode = (mode) => {
    setTrackingMode(mode);
    navigate('/model-import');
  };

  return (
    <div className="mode-selection-page animate-fade-in">
      <div className="page-top-bar">
        <button className="page-back-btn" onClick={() => navigate('/')}>
          <ArrowLeft size={16} />
          <span>Volver al Menú</span>
        </button>

        <button
          className="page-back-btn help-btn"
          onClick={() => setIsInfoModalOpen(true)}
        >
          <Info size={16} />
          <span>¿Necesitás Ayuda?</span>
        </button>
      </div>

      <div className="step-header">
        <span className="step-badge-text">Paso 1 de 3 · Configuración</span>
        <h2 className="step-title">Selecciona el Modo de Realidad Aumentada</h2>
        <p className="step-subtitle">
          Elige cómo deseas posicionar la maqueta 3D en tu espacio físico.
        </p>
      </div>

      <div className="mode-cards-container">
        {MODE_OPTIONS.map((option) => {
          const Icon = option.icon;
          const isSelected = trackingMode === option.id;

          return (
            <div
              key={option.id}
              onClick={() => handleSelectMode(option.id)}
              className={`mode-card ${isSelected ? option.selectedClass : ''}`}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <div
                  style={{
                    width: '48px',
                    height: '48px',
                    borderRadius: '14px',
                    background: option.iconBg,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    color: option.accentColor,
                  }}
                >
                  <Icon size={26} />
                </div>

                {isSelected && (
                  <span style={{ display: 'flex', alignItems: 'center', gap: '4px', color: '#4ade80', fontSize: '0.85rem', fontWeight: '600' }}>
                    <CheckCircle2 size={16} /> Seleccionado
                  </span>
                )}
              </div>

              <div>
                <h3 style={{ fontSize: '1.2rem', fontWeight: '600', marginBottom: '8px' }}>
                  {option.title}
                </h3>
                <p style={{ fontSize: '0.88rem', color: 'var(--text-secondary)', lineHeight: 1.5, whiteSpace: 'pre-line' }}>
                  {option.description}
                </p>
              </div>

              <div
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '6px',
                  color: option.accentColor,
                  fontSize: '0.9rem',
                  fontWeight: '600',
                  marginTop: 'auto',
                }}
              >
                <span>Seleccionar este modo</span>
                <ChevronRight size={18} />
              </div>
            </div>
          );
        })}
      </div>

      <InfoModal
        isOpen={isInfoModalOpen}
        onClose={() => setIsInfoModalOpen(false)}
      />
    </div>
  );
}
