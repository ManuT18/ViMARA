import React, { useRef, useState, useEffect, useCallback } from 'react';
import { Maximize2, Minimize2, RotateCcw, Camera } from 'lucide-react';

export default function UnityARViewer({ trackingMode = 'surface' }) {
  const iframeRef = useRef(null);
  const containerRef = useRef(null);
  const [isFullscreen, setIsFullscreen] = useState(false);

  // Sincronizar estado si el usuario sale mediante gestos del navegador o tecla Escape
  const handleFullscreenChange = useCallback(() => {
    const isCurrentlyNativeFullscreen = !!(
      document.fullscreenElement ||
      document.webkitFullscreenElement ||
      document.mozFullScreenElement ||
      document.msFullscreenElement
    );
    if (!isCurrentlyNativeFullscreen && isFullscreen) {
      setIsFullscreen(false);
    }
  }, [isFullscreen]);

  useEffect(() => {
    document.addEventListener('fullscreenchange', handleFullscreenChange);
    document.addEventListener('webkitfullscreenchange', handleFullscreenChange);
    document.addEventListener('mozfullscreenchange', handleFullscreenChange);
    document.addEventListener('MSFullscreenChange', handleFullscreenChange);

    return () => {
      document.removeEventListener('fullscreenchange', handleFullscreenChange);
      document.removeEventListener('webkitfullscreenchange', handleFullscreenChange);
      document.removeEventListener('mozfullscreenchange', handleFullscreenChange);
      document.removeEventListener('MSFullscreenChange', handleFullscreenChange);
    };
  }, [handleFullscreenChange]);

  // Bloquear el scroll del body cuando está en pantalla completa
  useEffect(() => {
    if (isFullscreen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = '';
    }
    return () => {
      document.body.style.overflow = '';
    };
  }, [isFullscreen]);

  const toggleFullscreen = async () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      setIsFullscreen(true);
      // Intentar activar API nativa si el navegador lo soporta (Android Chrome, Desktop)
      try {
        if (container.requestFullscreen) {
          await container.requestFullscreen();
        } else if (container.webkitRequestFullscreen) {
          await container.webkitRequestFullscreen();
        } else if (container.mozRequestFullScreen) {
          await container.mozRequestFullScreen();
        } else if (container.msRequestFullscreen) {
          await container.msRequestFullscreen();
        }
      } catch (err) {
        // En iOS Safari el requestFullscreen en divs no está soportado, el modo CSS se encarga transparentemente
        console.log('[ViMARA] Modo pantalla completa virtual activado (CSS fallback).');
      }
    } else {
      setIsFullscreen(false);
      try {
        if (document.fullscreenElement || document.webkitFullscreenElement) {
          if (document.exitFullscreen) {
            await document.exitFullscreen();
          } else if (document.webkitExitFullscreen) {
            await document.webkitExitFullscreen();
          } else if (document.mozCancelFullScreen) {
            await document.mozCancelFullScreen();
          } else if (document.msExitFullscreen) {
            await document.msExitFullscreen();
          }
        }
      } catch (err) {
        console.log('[ViMARA] Salida de pantalla completa.');
      }
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
      className={isFullscreen ? '' : 'glass-panel'}
      style={isFullscreen ? {
        position: 'fixed',
        top: 0,
        left: 0,
        width: '100vw',
        height: '100dvh',
        zIndex: 9999,
        background: '#000',
        display: 'flex',
        flexDirection: 'column',
        margin: 0,
        padding: 0,
        borderRadius: 0,
        overflow: 'hidden'
      } : {
        padding: '16px',
        display: 'flex',
        flexDirection: 'column',
        gap: '12px',
        width: '100%',
        position: 'relative',
        background: 'var(--bg-card, #1e293b)'
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
        zIndex: 10000,
        background: isFullscreen ? 'linear-gradient(to bottom, rgba(0,0,0,0.8), rgba(0,0,0,0))' : 'transparent',
        pointerEvents: isFullscreen ? 'none' : 'auto'
      }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '6px', 
          minWidth: 0,
          background: isFullscreen ? 'rgba(15, 23, 42, 0.75)' : 'transparent',
          padding: isFullscreen ? '6px 12px' : '0',
          borderRadius: isFullscreen ? '20px' : '0',
          border: isFullscreen ? '1px solid rgba(255,255,255,0.15)' : 'none',
          pointerEvents: 'auto'
        }}>
          <Camera size={18} color="var(--accent-primary, #3b82f6)" style={{ flexShrink: 0 }} />
          <span style={{ fontSize: '0.85rem', fontWeight: '600', color: '#fff', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            Visor WebAR · Modo {trackingMode === 'marker' ? 'Marcador' : 'Plano'}
          </span>
        </div>

        <div style={{ display: 'flex', gap: '6px', flexShrink: 0, pointerEvents: 'auto' }}>
          <button 
            className="btn-secondary" 
            onClick={reloadViewer}
            title="Reiniciar cámara y visor"
            style={{ 
              padding: '6px 10px', 
              fontSize: '0.8rem', 
              height: '34px', 
              minHeight: '34px',
              background: isFullscreen ? 'rgba(15, 23, 42, 0.85)' : undefined,
              borderColor: isFullscreen ? 'rgba(255,255,255,0.2)' : undefined
            }}
          >
            <RotateCcw size={14} />
            <span>Reiniciar</span>
          </button>

          <button 
            className="btn-primary" 
            onClick={toggleFullscreen}
            title={isFullscreen ? 'Salir de pantalla completa' : 'Pantalla completa'}
            style={{ 
              padding: '6px 10px', 
              fontSize: '0.8rem', 
              height: '34px', 
              minHeight: '34px',
              boxShadow: isFullscreen ? '0 0 15px rgba(59, 130, 246, 0.5)' : undefined
            }}
          >
            {isFullscreen ? <Minimize2 size={14} /> : <Maximize2 size={14} />}
            <span>{isFullscreen ? 'Salir' : 'Maximizar'}</span>
          </button>
        </div>
      </div>

      {/* Contenedor Iframe con permisos para Cámara y Sensores de Movimiento */}
      <div style={{
        width: '100%',
        height: isFullscreen ? '100dvh' : 'clamp(380px, 60vh, 600px)',
        borderRadius: isFullscreen ? '0' : '12px',
        overflow: 'hidden',
        background: '#000',
        position: isFullscreen ? 'absolute' : 'relative',
        top: 0,
        left: 0,
        right: 0,
        bottom: 0,
        zIndex: 1
      }}>
        <iframe
          ref={iframeRef}
          src={`/unity_ar/index.html?mode=${trackingMode}`}
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
