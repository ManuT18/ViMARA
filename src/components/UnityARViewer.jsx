import React, { useRef, useState, useEffect, useCallback } from 'react';
import { Maximize2, Minimize2, RotateCcw, Camera, Box } from 'lucide-react';
import { SAMPLES } from './SampleSelector';
import { useApp } from '../context/useApp';

export default function UnityARViewer({ trackingMode = 'surface', currentModel }) {
  const iframeRef = useRef(null);
  const containerRef = useRef(null);
  const [isFullscreen, setIsFullscreen] = useState(false);
  const { setCurrentModel } = useApp();

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

  // Bloquear el scroll del body y ocultar navbar en pantalla completa
  useEffect(() => {
    if (isFullscreen) {
      document.body.style.overflow = 'hidden';
      document.body.classList.add('ar-fullscreen-active');
    } else {
      document.body.style.overflow = '';
      document.body.classList.remove('ar-fullscreen-active');
    }
    return () => {
      document.body.style.overflow = '';
      document.body.classList.remove('ar-fullscreen-active');
    };
  }, [isFullscreen]);

  // Enviar modelo en vivo a Unity sin recargar el iframe cuando el usuario cambie de maqueta
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

  const toggleFullscreen = async () => {
    const container = containerRef.current;
    if (!container) return;

    if (!isFullscreen) {
      setIsFullscreen(true);
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

  // Construir URL inicial del iframe con modo y modelo codificados
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
        flexWrap: 'wrap',
        gap: '8px',
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
          gap: '8px', 
          minWidth: 0,
          background: isFullscreen ? 'rgba(15, 23, 42, 0.75)' : 'transparent',
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

        <div style={{ display: 'flex', gap: '6px', alignItems: 'center', flexShrink: 0, pointerEvents: 'auto' }}>
          {/* Selector rápido de maqueta en vivo sin reiniciar cámara */}
          <div style={{ position: 'relative', display: 'flex', alignItems: 'center' }}>
            <Box size={14} style={{ position: 'absolute', left: '10px', pointerEvents: 'none', color: isFullscreen ? '#93c5fd' : 'var(--accent-primary)' }} />
            <select
              aria-label="Cambiar modelo 3D en vivo"
              value={SAMPLES.some(s => s.id === currentModel?.id) ? currentModel.id : 'custom'}
              onChange={(e) => {
                const found = SAMPLES.find(s => s.id === e.target.value);
                if (found) setCurrentModel(found);
              }}
              style={{
                padding: '6px 10px 6px 28px',
                fontSize: '0.78rem',
                fontWeight: '600',
                height: '34px',
                borderRadius: '8px',
                border: isFullscreen ? '1px solid rgba(255,255,255,0.2)' : '1px solid var(--border-light)',
                background: isFullscreen ? 'rgba(15, 23, 42, 0.85)' : 'var(--bg-secondary)',
                color: isFullscreen ? '#fff' : 'var(--text-primary)',
                cursor: 'pointer',
                maxWidth: '175px'
              }}
            >
              {!SAMPLES.some(s => s.id === currentModel?.id) && currentModel && (
                <option value="custom">📁 {currentModel.name}</option>
              )}
              {SAMPLES.map((s) => (
                <option key={s.id} value={s.id}>
                  {s.name}
                </option>
              ))}
            </select>
          </div>

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
              borderColor: isFullscreen ? 'rgba(255,255,255,0.2)' : undefined,
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
          src={initialIframeSrc.current}
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
    </div>
  );
}
