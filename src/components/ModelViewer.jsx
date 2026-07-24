import React, { useRef, useState, useEffect } from 'react';
import { Smartphone, RotateCw, Eye, Sparkles } from 'lucide-react';

export default function ModelViewer({ modelUrl, modelName, format }) {
  const viewerRef = useRef(null);
  const [autoRotate, setAutoRotate] = useState(true);
  const [whiteModelMode, setWhiteModelMode] = useState(false);

  // Toggle white-model mode (Fase 1 - Geometry only look)
  useEffect(() => {
    const viewer = viewerRef.current;
    if (!viewer) return;

    if (whiteModelMode) {
      viewer.style.setProperty('--poster-color', '#ffffff');
    }
  }, [whiteModelMode]);

  return (
    <div className="glass-panel" style={{
      padding: '20px',
      display: 'flex',
      flexDirection: 'column',
      gap: '16px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Top Controls Bar */}
      <div style={{
        display: 'flex',
        flexDirection: 'row',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: '12px'
      }}>
        <div>
          <h2 style={{ fontSize: '1.2rem', fontWeight: '700', color: 'var(--text-primary)' }}>
            {modelName || 'Maqueta Arquitectónica'}
          </h2>
          <span style={{ fontSize: '0.82rem', color: 'var(--text-muted)', fontWeight: '500' }}>
            Formato: <strong style={{ color: 'var(--accent-primary)', background: 'var(--accent-light)', padding: '2px 8px', borderRadius: '6px' }}>{format?.toUpperCase() || 'GLB'}</strong>
          </span>
        </div>

        <div style={{ display: 'flex', gap: '8px', flexWrap: 'wrap' }}>
          <button 
            className="btn-secondary"
            onClick={() => setAutoRotate(!autoRotate)}
            title="Conmutar Rotación Automática"
            style={{ 
              minHeight: '48px', 
              padding: '10px 16px',
              fontSize: '0.88rem'
            }}
          >
            <RotateCw size={18} className={autoRotate ? 'spin' : ''} />
            <span>{autoRotate ? 'Giro ON' : 'Giro OFF'}</span>
          </button>

          <button 
            className="btn-secondary"
            onClick={() => setWhiteModelMode(!whiteModelMode)}
            title="Fase 1: Modo Maqueta Blanca"
            style={{
              minHeight: '48px',
              padding: '10px 16px',
              fontSize: '0.88rem',
              borderColor: whiteModelMode ? 'var(--accent-primary)' : 'var(--border-light)',
              background: whiteModelMode ? 'var(--accent-light)' : '#ffffff',
              color: whiteModelMode ? 'var(--accent-primary)' : 'var(--text-primary)'
            }}
          >
            <Eye size={18} />
            <span>{whiteModelMode ? 'Modo Blanco' : 'Texturas'}</span>
          </button>
        </div>
      </div>

      {/* 3D Container using Google <model-viewer> with Light Theme Studio Styling */}
      <div style={{
        height: 'clamp(320px, 50vh, 460px)',
        width: '100%',
        borderRadius: 'var(--radius-xl)',
        background: 'radial-gradient(circle at center, #ffffff 0%, #f1f5f9 100%)',
        position: 'relative',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        border: '1px solid var(--border-light)',
        boxShadow: 'inset 0 2px 6px rgba(15, 23, 42, 0.03)'
      }}>
        <model-viewer
          ref={viewerRef}
          src={modelUrl}
          alt={modelName || 'Maqueta 3D ViMARA'}
          ar
          ar-modes="webxr scene-viewer quick-look"
          camera-controls
          touch-action="pan-y"
          auto-rotate={autoRotate ? "" : undefined}
          shadow-intensity="1.2"
          shadow-softness="0.8"
          exposure="1.0"
          style={{ width: '100%', height: '100%', borderRadius: 'var(--radius-xl)' }}
        >
          {/* Custom AR Trigger Button inside <model-viewer> slot */}
          <button
            slot="ar-button"
            className="btn-primary"
            style={{
              position: 'absolute',
              bottom: '16px',
              right: '16px',
              zIndex: 10,
              minHeight: '48px',
              padding: '12px 20px',
              borderRadius: 'var(--radius-lg)',
              boxShadow: 'var(--shadow-lg)'
            }}
          >
            <Smartphone size={20} />
            <span>Ver en Realidad Aumentada</span>
          </button>
        </model-viewer>
      </div>

      {/* Instructions Footer */}
      <div style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        fontSize: '0.85rem',
        color: 'var(--text-secondary)',
        padding: '4px 6px',
        fontWeight: '500'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <Sparkles size={18} color="var(--accent-primary)" />
          <span>Arrastra para rotar · Pellizca para zoom · Toca AR desde tu móvil</span>
        </div>
      </div>
    </div>
  );
}
