import React, { useRef, useState, useEffect, useCallback } from 'react';
import { Maximize2, Minimize2, RotateCcw, Camera } from 'lucide-react';

export default function UnityARViewer({ trackingMode = 'surface', currentModel }) {
  const iframeRef = useRef(null);
  const containerRef = useRef(null);
  const [isFullscreen, setIsFullscreen] = useState(false);

  // Sincronizar estado si el usuario sale mediante gestos del sistema o tecla Escape
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

  // Bloquear el scroll del body y ocultar navbar en pantalla completa
  useEffect(() => {
    if (isFullscreen) {
      document.body.style.overflow = 'hidden';
      document.body.classList.add('ar-fullscreen-active');
      window.scrollTo(0, 1);
    } else {
      document.body.style.overflow = '';
      document.body.classList.remove('ar-fullscreen-active');
    }
    return () => {
      document.body.style.overflow = '';
      document.body.classList.remove('ar-fullscreen-active');
    };
  }, [isFullscreen]);

  // Enviar modelo a Unity cuando se monte o cambie
  useEffect(() => {
    if (iframeRef.current && iframeRef.current.contentWindow && currentModel?.url) {
      const format = currentModel.format || 'glb';
      const payload = `${currentModel.url}|${format}`;
      iframeRef.current.contentWindow.postMessage(
        { type: 'LOAD_MODEL', payload },
        '*'
      );
    }
  }, [currentModel]);

  const requestElementFullscreen = async (el) => {
    if (!el) return false;
    try {
      if (el.requestFullscreen) {
        await el.requestFullscreen({ navigationUI: 'hide' });
        return true;
      }
      if (el.webkitRequestFullscreen) {
        await el.webkitRequestFullscreen();
        return true;
      }
      if (el.webkitEnterFullscreen) {
        el.webkitEnterFullscreen();
        return true;
      }
      if (el.mozRequestFullScreen) {
        await el.mozRequestFullScreen();
        return true;
      }
      if (el.msRequestFullscreen) {
        await el.msRequestFullscreen();
        return true;
      }
    } catch (_) {
      return false;
    }
    return false;
  };

  const toggleFullscreen = async () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      setIsFullscreen(true);
      // 1. Intentar sobre el contenedor del visor
      let entered = await requestElementFullscreen(container);
      // 2. En iPadOS / iOS 16+ Safari e instalaciones web, document.documentElement tiene mayor soporte nativo
      if (!entered) {
        entered = await requestElementFullscreen(document.documentElement);
      }
      // 3. Notificar a Unity para que recalcule las dimensiones del canvas al 100%
      if (iframeRef.current && iframeRef.current.contentWindow) {
        iframeRef.current.contentWindow.postMessage({ type: 'RESIZE_VIEWPORT' }, '*');
      }
    } else {
      setIsFullscreen(false);
      try {
        if (
          document.fullscreenElement ||
          document.webkitFullscreenElement ||
          document.mozFullScreenElement ||
          document.msFullscreenElement
        ) {
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

  const initialIframeSrc = useRef(
    `/unity_ar/index.html?mode=${encodeURIComponent(trackingMode)}&model=${encodeURIComponent(
      currentModel?.url || 'preset:house'
    )}&format=${encodeURIComponent(currentModel?.format || 'preset')}`
  );

  return (
    <div 
      ref={containerRef}
      className={isFullscreen ? '' : 'glass-panel'}
      style={isFullscreen ? {
        position: 'fixed',
        inset: 0,
        width: '100vw',
        height: '100dvh',
        minHeight: '-webkit-fill-available',
        zIndex: 2147483647,
        background: '#000',
        display: 'flex',
        flexDirection: 'column',
        margin: 0,
        padding: 0,
        borderRadius: 0,
        overflow: 'hidden',
        touchAction: 'none'
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
      {/* Barra de control superior: únicamente título a la izquierda y botones Reiniciar + Maximizar a la derecha */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        gap: '8px',
        padding: isFullscreen
          ? 'calc(env(safe-area-inset-top, 0px) + 12px) calc(env(safe-area-inset-right, 0px) + 16px) 12px calc(env(safe-area-inset-left, 0px) + 16px)'
          : '0',
        position: isFullscreen ? 'absolute' : 'relative',
        top: 0,
        left: 0,
        right: 0,
        zIndex: 2147483647,
        background: isFullscreen ? 'linear-gradient(to bottom, rgba(0,0,0,0.82), rgba(0,0,0,0))' : 'transparent',
        pointerEvents: isFullscreen ? 'none' : 'auto'
      }}>
        <div style={{ 
          display: 'flex', 
          alignItems: 'center', 
          gap: '8px', 
          minWidth: 0,
          background: isFullscreen ? 'rgba(15, 23, 42, 0.78)' : 'transparent',
          padding: isFullscreen ? '6px 12px' : '0',
          borderRadius: isFullscreen ? '20px' : '0',
          border: isFullscreen ? '1px solid rgba(255,255,255,0.15)' : 'none',
          pointerEvents: 'auto'
        }}>
          <Camera size={18} color="var(--accent-primary, #3b82f6)" style={{ flexShrink: 0 }} />
          <span style={{ fontSize: '0.85rem', fontWeight: '600', color: isFullscreen ? '#fff' : 'var(--text-primary)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
            {currentModel?.name || `Modo ${trackingMode === 'marker' ? 'Marcador' : 'Plano'}`}
          </span>
        </div>

        <div style={{ display: 'flex', gap: '8px', alignItems: 'center', flexShrink: 0, pointerEvents: 'auto' }}>
          <button 
            className="btn-secondary" 
            onClick={reloadViewer}
            title="Reiniciar cámara y visor"
            style={{ 
              padding: '6px 12px', 
              fontSize: '0.8rem', 
              height: '36px', 
              minHeight: '36px',
              background: isFullscreen ? 'rgba(15, 23, 42, 0.85)' : undefined,
              borderColor: isFullscreen ? 'rgba(255,255,255,0.22)' : undefined,
              color: isFullscreen ? '#fff' : undefined
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
              padding: '6px 12px', 
              fontSize: '0.8rem', 
              height: '36px', 
              minHeight: '36px',
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
        height: isFullscreen ? '100%' : 'clamp(380px, 60vh, 600px)',
        borderRadius: isFullscreen ? '0' : '12px',
        overflow: 'hidden',
        background: '#000',
        position: isFullscreen ? 'absolute' : 'relative',
        inset: 0,
        zIndex: 1
      }}>
        <iframe
          ref={iframeRef}
          src={initialIframeSrc.current}
          title="ViMARA WebAR Viewport"
          allow="camera; accelerometer; gyroscope; microphone; fullscreen; xr-spatial-tracking"
          allowFullScreen
          style={{
            width: '100%',
            height: '100%',
            border: 'none',
            display: 'block'
          }}
        />
      </div>
    </div>
  );
}
