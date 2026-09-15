import React, { useRef, useState } from 'react';
import { Maximize2, Minimize2, RotateCcw, Camera } from 'lucide-react';

export default function UnityARViewer({ trackingMode = 'surface' }) {
  const iframeRef = useRef(null);
  const containerRef = useRef(null);
  const [isFullscreen, setIsFullscreen] = useState(false);

  const toggleFullscreen = () => {
    if (!containerRef.current) return;

    if (!document.fullscreenElement) {
      containerRef.current.requestFullscreen().then(() => {
        setIsFullscreen(true);
      }).catch((err) => {
        console.warn('Error al activar pantalla completa:', err);
      });
    } else {
      document.exitFullscreen().then(() => {
        setIsFullscreen(false);
      }).catch((err) => {
        console.warn('Error al salir de pantalla completa:', err);
      });
    }
  };

  const reloadViewer = () => {
    if (iframeRef.current) {
      iframeRef.current.src = iframeRef.current.src;
    }
  };

  return (
    <div 
      ref={containerRef}
      className="glass-panel" 
      style={{
        padding: isFullscreen ? '0' : '16px',
        display: 'flex',
        flexDirection: 'column',
        gap: '12px',
        width: '100%',
        height: isFullscreen ? '100vh' : 'auto',
        position: 'relative',
        background: isFullscreen ? '#000' : 'var(--bg-card, #1e293b)'
      }}
    >
      {/* Barra de control superior */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        padding: isFullscreen ? '12px 16px' : '0',
        position: isFullscreen ? 'absolute' : 'relative',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 20,
        background: isFullscreen ? 'rgba(0,0,0,0.6)' : 'transparent',
        backdropFilter: isFullscreen ? 'blur(8px)' : 'none'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '6px', minWidth: 0 }}>
          <Camera size={18} color="var(--accent-primary, #3b82f6)" style={{ flexShrink: 0 }} />
          <span style={{ fontSize: '0.85rem', fontWeight: '600', color: '#fff', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            Visor WebAR · Modo {trackingMode === 'marker' ? 'Marcador' : 'Plano'}
          </span>
        </div>

        <div style={{ display: 'flex', gap: '6px', flexShrink: 0 }}>
          <button 
            className="btn-secondary" 
            onClick={reloadViewer}
            title="Reiniciar cámara y visor"
            style={{ padding: '6px 10px', fontSize: '0.8rem', height: '34px', minHeight: '34px' }}
          >
            <RotateCcw size={14} />
            <span>Reiniciar</span>
          </button>

          <button 
            className="btn-primary" 
            onClick={toggleFullscreen}
            title={isFullscreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
            style={{ padding: '6px 10px', fontSize: '0.8rem', height: '34px', minHeight: '34px' }}
          >
            {isFullscreen ? <Minimize2 size={14} /> : <Maximize2 size={14} />}
            <span>{isFullscreen ? 'Salir' : 'Maximizar'}</span>
          </button>
        </div>
      </div>

      {/* Contenedor Iframe con permisos para Cámara y Sensores de Movimiento */}
      <div style={{
        width: '100%',
        height: isFullscreen ? '100%' : 'clamp(380px, 60vh, 600px)',
        borderRadius: isFullscreen ? '0' : '12px',
        overflow: 'hidden',
        background: '#000',
        position: 'relative'
      }}>
        <iframe
          ref={iframeRef}
          src="/unity_ar/index.html"
          title="ViMARA WebAR Viewport"
          allow="camera; accelerometer; gyroscope; microphone; fullscreen; xr-spatial-tracking"
          style={{
            width: '100%',
            height: '100%',
            border: 'none',
            display: 'block'
          }}
        />
      </div>

      {!isFullscreen && (
        <div style={{
          display: 'flex',
          justifyContent: 'space-between',
          fontSize: '0.82rem',
          color: 'var(--text-secondary, #94a3b8)',
          padding: '2px 4px'
        }}>
          <span>👉 <strong>En celular</strong>: Apuntá al plano y tocá la pantalla para fijar o liberar el modelo.</span>
          <span>👉 <strong>En laptop</strong>: Hacé clic con el mouse o presioná <code>Espacio</code> para interactuar.</span>
        </div>
      )}
    </div>
  );
}
