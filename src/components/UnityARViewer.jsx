import React, { useRef, useState, useEffect, useCallback } from 'react';
import { Maximize2, Minimize2, RotateCcw, Camera } from 'lucide-react';
import { DEFAULT_MODEL, TRACKING_MODES } from '../constants/arConstants';

function getNativeFullscreenElement() {
  return (
    document.fullscreenElement ||
    document.webkitFullscreenElement ||
    document.mozFullScreenElement ||
    document.msFullscreenElement ||
    null
  );
}

async function requestElementFullscreen(el) {
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
  } catch {
    return false;
  }
  return false;
}

async function exitNativeFullscreen() {
  if (!getNativeFullscreenElement()) return;
  try {
    if (document.exitFullscreen) {
      await document.exitFullscreen();
    } else if (document.webkitExitFullscreen) {
      await document.webkitExitFullscreen();
    } else if (document.mozCancelFullScreen) {
      await document.mozCancelFullScreen();
    } else if (document.msExitFullscreen) {
      await document.msExitFullscreen();
    }
  } catch {
    // Ignorar si el navegador ya cerró el estado fullscreen
  }
}

export default function UnityARViewer({
  trackingMode = TRACKING_MODES.PLANE,
  currentModel = DEFAULT_MODEL,
}) {
  const iframeRef = useRef(null);
  const containerRef = useRef(null);
  const [isFullscreen, setIsFullscreen] = useState(false);

  const initialModelUrl = currentModel?.url || DEFAULT_MODEL.url;
  const initialModelFormat = currentModel?.format || DEFAULT_MODEL.format;
  const initialPayload = `${initialModelUrl}|${initialModelFormat}`;

  const lastSentPayloadRef = useRef(initialPayload);
  const lastSentModeRef = useRef(trackingMode);

  const initialIframeSrc = useRef(
    `/unity_ar/index.html?mode=${encodeURIComponent(trackingMode)}&model=${encodeURIComponent(
      initialModelUrl
    )}&format=${encodeURIComponent(initialModelFormat)}`
  );

  const postToUnity = useCallback((message) => {
    const win = iframeRef.current?.contentWindow;
    if (win) {
      win.postMessage(message, '*');
    }
  }, []);

  // Sincronizar estado si el usuario sale mediante gestos del sistema o tecla Escape
  const handleFullscreenChange = useCallback(() => {
    if (!getNativeFullscreenElement() && isFullscreen) {
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

  // Bloquear scroll del body y ocultar navbar en modo pantalla completa
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

  // Enviar nuevo modelo a Unity únicamente cuando cambie respecto al modelo cargado inicialmente por URL
  useEffect(() => {
    if (!currentModel?.url) return;
    const format = currentModel.format || 'glb';
    const payload = `${currentModel.url}|${format}`;
    if (payload === lastSentPayloadRef.current) return;

    lastSentPayloadRef.current = payload;
    postToUnity({ type: 'LOAD_MODEL', payload });
  }, [currentModel, postToUnity]);

  // Sincronizar cambios dinámicos en el modo de tracking
  useEffect(() => {
    if (!trackingMode || trackingMode === lastSentModeRef.current) return;
    lastSentModeRef.current = trackingMode;
    postToUnity({ type: 'SET_MODE', mode: trackingMode });
  }, [trackingMode, postToUnity]);

  const toggleFullscreen = async () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      setIsFullscreen(true);
      let entered = await requestElementFullscreen(container);
      if (!entered) {
        await requestElementFullscreen(document.documentElement);
      }
      postToUnity({ type: 'RESIZE_VIEWPORT' });
    } else {
      setIsFullscreen(false);
      await exitNativeFullscreen();
      postToUnity({ type: 'RESIZE_VIEWPORT' });
    }
  };

  const reloadViewer = () => {
    if (!iframeRef.current) return;
    const modelUrl = currentModel?.url || DEFAULT_MODEL.url;
    const modelFormat = currentModel?.format || DEFAULT_MODEL.format;
    lastSentPayloadRef.current = `${modelUrl}|${modelFormat}`;
    lastSentModeRef.current = trackingMode;
    iframeRef.current.src = `/unity_ar/index.html?mode=${encodeURIComponent(
      trackingMode
    )}&model=${encodeURIComponent(modelUrl)}&format=${encodeURIComponent(modelFormat)}`;
  };

  const isMarkerMode = trackingMode === TRACKING_MODES.MARKER;

  return (
    <div
      ref={containerRef}
      className={isFullscreen ? '' : 'glass-panel'}
      style={
        isFullscreen
          ? {
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
              touchAction: 'none',
            }
          : {
              padding: '16px',
              display: 'flex',
              flexDirection: 'column',
              gap: '12px',
              width: '100%',
              position: 'relative',
              background: 'var(--bg-card, #1e293b)',
            }
      }
    >
      {/* Barra de control superior */}
      <div
        style={{
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
          background: isFullscreen
            ? 'linear-gradient(to bottom, rgba(0,0,0,0.82), rgba(0,0,0,0))'
            : 'transparent',
          pointerEvents: isFullscreen ? 'none' : 'auto',
        }}
      >
        <div
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            minWidth: 0,
            background: isFullscreen ? 'rgba(15, 23, 42, 0.78)' : 'transparent',
            padding: isFullscreen ? '6px 12px' : '0',
            borderRadius: isFullscreen ? '20px' : '0',
            border: isFullscreen ? '1px solid rgba(255,255,255,0.15)' : 'none',
            pointerEvents: 'auto',
          }}
        >
          <Camera size={18} color="var(--accent-primary, #3b82f6)" style={{ flexShrink: 0 }} />
          <span
            style={{
              fontSize: '0.85rem',
              fontWeight: '600',
              color: isFullscreen ? '#fff' : 'var(--text-primary)',
              whiteSpace: 'nowrap',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
            }}
          >
            {currentModel?.name || `Modo ${isMarkerMode ? 'Marcador' : 'Plano'}`}
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
              color: isFullscreen ? '#fff' : undefined,
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
              boxShadow: isFullscreen ? '0 0 15px rgba(59, 130, 246, 0.5)' : undefined,
            }}
          >
            {isFullscreen ? <Minimize2 size={14} /> : <Maximize2 size={14} />}
            <span>{isFullscreen ? 'Salir' : 'Maximizar'}</span>
          </button>
        </div>
      </div>

      {/* Contenedor Iframe con permisos para Cámara y Sensores de Movimiento */}
      <div
        style={{
          width: '100%',
          height: isFullscreen ? '100%' : 'clamp(380px, 60vh, 600px)',
          borderRadius: isFullscreen ? '0' : '12px',
          overflow: 'hidden',
          background: '#000',
          position: isFullscreen ? 'absolute' : 'relative',
          inset: 0,
          zIndex: 1,
        }}
      >
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
            display: 'block',
          }}
        />
      </div>
    </div>
  );
}
