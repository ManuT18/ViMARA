import React, { useEffect } from 'react';
import { createPortal } from 'react-dom';
import { X, Info, Target, Layers, Check } from 'lucide-react';

export default function InfoModal({ isOpen, onClose }) {
  useEffect(() => {
    if (!isOpen) return undefined;
    const handleKeyDown = (e) => {
      if (e.key === 'Escape') onClose();
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, onClose]);

  if (!isOpen) return null;

  return createPortal(
    <div className="modal-overlay" onClick={onClose}>
      <div
        className="glass-panel modal-card animate-fade-in"
        onClick={(e) => e.stopPropagation()}
      >
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

        <div style={{ marginTop: '24px', display: 'flex', justifyContent: 'flex-end' }}>
          <button className="btn-primary" onClick={onClose}>
            <Check size={18} /> Entendido / Cerrar
          </button>
        </div>
      </div>
    </div>,
    document.body
  );
}
