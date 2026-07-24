import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import FileUploader from '../components/FileUploader';
import SampleSelector from '../components/SampleSelector';
import { ArrowLeft, Play, Target, Layers, FileCode, CheckCircle2, AlertCircle } from 'lucide-react';

export default function ModelImport() {
  const navigate = useNavigate();
  const { trackingMode, currentModel, setCurrentModel } = useApp();

  const handleSelectSample = (sample) => {
    setCurrentModel(sample);
  };

  const handleFileSelect = (uploadedFile) => {
    setCurrentModel({
      id: 'custom-' + Date.now(),
      name: uploadedFile.name,
      format: uploadedFile.format,
      url: uploadedFile.url
    });
  };

  const handleStartAR = () => {
    if (currentModel && currentModel.url) {
      navigate('/ar-view');
    }
  };

  const handleBackToMode = () => {
    navigate('/mode-selection');
  };

  const modeLabel = trackingMode === 'marker' 
    ? 'Seguimiento por Marcador' 
    : 'Seguimiento por Plano';

  return (
    <div
      className="animate-fade-in"
      style={{
        maxWidth: '1200px',
        margin: '0 auto',
        padding: '32px 24px',
        display: 'flex',
        flexDirection: 'column',
        gap: '28px'
      }}
    >
      {/* Top Header & Navigation */}
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <button className="btn-secondary" onClick={handleBackToMode}>
          <ArrowLeft size={18} />
          <span>Volver a Selección de Modo</span>
        </button>

        {/* Selected Mode Tag */}
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '8px',
          padding: '6px 16px',
          borderRadius: '20px',
          background: trackingMode === 'marker' ? 'rgba(59, 130, 246, 0.15)' : 'rgba(139, 92, 246, 0.15)',
          border: `1px solid ${trackingMode === 'marker' ? 'rgba(59, 130, 246, 0.3)' : 'rgba(139, 92, 246, 0.3)'}`,
          color: trackingMode === 'marker' ? '#60a5fa' : '#c084fc',
          fontSize: '0.88rem',
          fontWeight: '500'
        }}>
          {trackingMode === 'marker' ? <Target size={16} /> : <Layers size={16} />}
          <span>Modo Activo: <strong>{modeLabel}</strong></span>
        </div>
      </div>

      {/* Step Title */}
      <div style={{ textAlign: 'center' }}>
        <span style={{ fontSize: '0.85rem', color: 'var(--accent-primary)', fontWeight: '600', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          Paso 2 de 3 · Selección de Modelo 3D
        </span>
        <h2 style={{ fontSize: '2rem', fontWeight: '700', marginTop: '4px' }}>
          Importar o Seleccionar Maqueta 3D
        </h2>
        <p style={{ fontSize: '0.95rem', color: 'var(--text-secondary)', marginTop: '8px' }}>
          Sube tu propia maqueta o selecciona un ejemplo predefinido para comenzar.
        </p>
      </div>

      {/* Content Grid */}
      <div style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(360px, 1fr))',
        gap: '24px',
        alignItems: 'start'
      }}>
        {/* Left Column: FileUploader & Format Recommendations */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          <FileUploader
            onFileSelect={handleFileSelect}
            activeFileName={currentModel?.id?.startsWith('custom-') ? currentModel.name : null}
          />

          {/* Format Recommendations Card */}
          <div className="glass-panel" style={{ padding: '24px' }}>
            <h4 style={{ fontSize: '1rem', fontWeight: '600', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <FileCode size={18} color="var(--accent-primary)" />
              Recomendaciones de Formato 3D
            </h4>
            <ul style={{
              fontSize: '0.85rem',
              color: 'var(--text-secondary)',
              display: 'flex',
              flexDirection: 'column',
              gap: '8px',
              paddingLeft: '20px'
            }}>
              <li>
                <strong>.GLB / .GLTF:</strong> Formato recomendado para la mejor fidelidad en WebAR, materiales PBR y texturas integradas.
              </li>
              <li>
                <strong>.STL:</strong> Recomendado para prototipos de malla y maquetas físicas sin textura.
              </li>
              <li>
                <strong>.OBJ:</strong> Soporte para geometría básica exportada desde SketchUp o Blender.
              </li>
            </ul>
          </div>
        </div>

        {/* Right Column: SampleSelector & Action Confirmation */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          <SampleSelector
            onSelectSample={handleSelectSample}
            activeSampleId={currentModel?.id}
          />

          {/* Selected Model Confirmation Panel & "Iniciar AR" Action */}
          <div className="glass-panel" style={{ padding: '24px', display: 'flex', flexDirection: 'column', gap: '16px' }}>
            <h4 style={{ fontSize: '1rem', fontWeight: '600' }}>
              Estado del Modelo Seleccionado
            </h4>

            {currentModel ? (
              <div className="glass-card" style={{ padding: '16px', display: 'flex', alignItems: 'center', gap: '12px', borderColor: 'rgba(34, 197, 94, 0.4)' }}>
                <CheckCircle2 size={24} color="#4ade80" />
                <div>
                  <p style={{ fontWeight: '600', fontSize: '0.92rem' }}>{currentModel.name}</p>
                  <p style={{ fontSize: '0.78rem', color: 'var(--text-muted)' }}>
                    Formato: <strong style={{ color: 'var(--accent-primary)' }}>{currentModel.format?.toUpperCase()}</strong>
                  </p>
                </div>
              </div>
            ) : (
              <div className="glass-card" style={{ padding: '16px', display: 'flex', alignItems: 'center', gap: '12px', borderColor: 'rgba(234, 179, 8, 0.4)' }}>
                <AlertCircle size={24} color="#facc15" />
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Aún no se ha seleccionado ninguna maqueta 3D. Elige un ejemplo o sube un archivo.
                </p>
              </div>
            )}

            {/* "Iniciar AR" Action Button */}
            <button
              className="btn-primary"
              onClick={handleStartAR}
              disabled={!currentModel || !currentModel.url}
              style={{
                width: '100%',
                justifyContent: 'center',
                padding: '14px',
                fontSize: '1.05rem',
                opacity: (!currentModel || !currentModel.url) ? 0.5 : 1,
                cursor: (!currentModel || !currentModel.url) ? 'not-allowed' : 'pointer'
              }}
            >
              <Play size={20} />
              <span>Iniciar AR</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
