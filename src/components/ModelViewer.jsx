import React, { useRef, useState, useEffect } from 'react';
import { Smartphone, RotateCw, Eye, Sun, Layers, Sparkles } from 'lucide-react';

export default function ModelViewer({ modelUrl, modelName, format }) {
  const viewerRef = useRef(null);
  const [autoRotate, setAutoRotate] = useState(true);
  const [whiteModelMode, setWhiteModelMode] = useState(false);
  const [arSupported, setArSupported] = useState(true);

  // Toggle white-model mode (Fase 1 - Geometry only look)
  useEffect(() => {
    const viewer = viewerRef.current;
    if (!viewer) return;

    if (whiteModelMode) {
      viewer.style.setProperty('--poster-color', '#ffffff');
      // Apply white override material if needed
    }
  }, [whiteModelMode]);

  return (
    <div className="glass-panel" style={{
      padding: '24px',
      display: 'flex',
      flexDirection: 'column',
      gap: '16px',
      position: 'relative',
      overflow: 'hidden'
    }}>
      {/* Top Controls Bar */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h2 style={{ fontSize: '1.2rem', fontWeight: '600' }}>
            {modelName || 'Maqueta Arquitectónica'}
          </h2>
          <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>
            Formato: <strong style={{ color: 'var(--accent-primary)' }}>{format?.toUpperCase() || 'GLB'}</strong>
          </span>
        </div>

        <div style={{ display: 'flex', gap: '8px' }}>
          <button 
            className="btn-secondary"
            onClick={() => setAutoRotate(!autoRotate)}
            title="Conmutar Rotación Automática"
            style={{ padding: '8px 12px' }}
          >
            <RotateCw size={16} className={autoRotate ? 'spin' : ''} />
            <span>{autoRotate ? 'Giro ON' : 'Giro OFF'}</span>
          </button>

          <button 
            className="btn-secondary"
            onClick={() => setWhiteModelMode(!whiteModelMode)}
            title="Fase 1: Modo Maqueta Blanca"
            style={{
              padding: '8px 12px',
              borderColor: whiteModelMode ? 'var(--accent-primary)' : 'var(--border-glass)',
              color: whiteModelMode ? 'var(--accent-primary)' : 'var(--text-primary)'
            }}
          >
            <Eye size={16} />
            <span>{whiteModelMode ? 'Modo Blanco' : 'Texturas'}</span>
          </button>
        </div>
      </div>

      {/* 3D Container using Google <model-viewer> */}
      <div style={{
        height: '460px',
        width: '100%',
        borderRadius: 'var(--radius-md)',
        background: 'radial-gradient(circle at center, rgba(30, 41, 59, 0.8) 0%, rgba(15, 23, 42, 0.95) 100%)',
        position: 'relative',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        border: '1px solid rgba(255, 255, 255, 0.05)'
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
          shadow-intensity="1.5"
          exposure="1.0"
          style={{ width: '100%', height: '100%', borderRadius: 'var(--radius-md)' }}
        >
          {/* Custom AR Trigger Button inside <model-viewer> slot */}
          <button
            slot="ar-button"
            className="btn-primary"
            style={{
              position: 'absolute',
              bottom: '20px',
              right: '20px',
              zIndex: 10
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
        padding: '4px 8px'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
          <Sparkles size={16} color="var(--accent-primary)" />
          <span>Arrastra para rotar · Pellizca para zoom · Toca AR desde tu iPhone / Android</span>
        </div>
      </div>
    </div>
  );
}
