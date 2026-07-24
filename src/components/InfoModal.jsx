import React from 'react';
import { X, Info, Target, Layers, Check } from 'lucide-react';

export default function InfoModal({ isOpen, onClose }) {
  if (!isOpen) return null;

  return (
    <div
      style={{
        position: 'fixed',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(8px)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
        padding: '20px'
      }}
      onClick={onClose}
    >
      <div
        className="glass-panel animate-fade-in"
        style={{
          maxWidth: '560px',
          width: '100%',
          padding: '28px',
          background: 'var(--bg-secondary)',
          border: '1px solid var(--border-glass)',
          borderRadius: 'var(--radius-lg)',
          boxShadow: '0 20px 50px rgba(0,0,0,0.6)',
          position: 'relative'
        }}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
            <div style={{
              width: '36px',
              height: '36px',
              borderRadius: '50%',
              background: 'rgba(59, 130, 246, 0.15)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              color: 'var(--accent-primary)'
            }}>
              <Info size={20} />
            </div>
            <h3 style={{ fontSize: '1.25rem', fontWeight: '600' }}>Información de Selección</h3>
          </div>
          <button
            onClick={onClose}
            style={{
              background: 'transparent',
              border: 'none',
              color: 'var(--text-muted)',
              cursor: 'pointer',
              padding: '6px',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center'
            }}
            title="Cerrar"
          >
            <X size={20} />
          </button>
        </div>

        {/* Body Content */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px', fontSize: '0.9rem', color: 'var(--text-secondary)' }}>
          <p>
            En <strong>ViMARA WebAR</strong> puedes seleccionar dos métodos de seguimiento para proyectar maquetas 3D en el entorno real:
          </p>

          <div className="glass-card" style={{ padding: '16px', borderLeft: '4px solid #3b82f6' }}>
            <h4 style={{ color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
              <Target size={18} color="#3b82f6" /> Seguimiento por Marcador (Image Tracking)
            </h4>
            <p style={{ fontSize: '0.85rem' }}>
              Utiliza una imagen de referencia, plano impreso o código QR. La maqueta se anclará exactamente sobre el marcador detectado por la cámara.
            </p>
          </div>

          <div className="glass-card" style={{ padding: '16px', borderLeft: '4px solid #8b5cf6' }}>
            <h4 style={{ color: 'var(--text-primary)', display: 'flex', alignItems: 'center', gap: '8px', marginBottom: '6px' }}>
              <Layers size={18} color="#8b5cf6" /> Seguimiento por Plano (Surface Detection)
            </h4>
            <p style={{ fontSize: '0.85rem' }}>
              Detecta automáticamente pisos, mesas o cualquier superficie plana utilizando los sensores del dispositivo (SLAM). Ideal para colocar maquetas a escala 1:1 en espacios reales.
            </p>
          </div>
        </div>

        {/* Footer */}
        <div style={{ marginTop: '24px', display: 'flex', justifyContent: 'flex-end' }}>
          <button className="btn-primary" onClick={onClose}>
            <Check size={18} /> Entendido / Cerrar
          </button>
        </div>
      </div>
    </div>
  );
}
