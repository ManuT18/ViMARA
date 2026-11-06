import React from 'react';
import { useNavigate } from 'react-router-dom';
import { useApp } from '../context/useApp';
import { FORMAT_RECOMMENDATIONS, TRACKING_MODES } from '../constants/arConstants';
import FileUploader from '../components/FileUploader';
import SampleSelector from '../components/SampleSelector';
import { ArrowLeft, Play, Target, Layers, FileCode, CheckCircle2, AlertCircle } from 'lucide-react';

export default function ModelImport() {
  const navigate = useNavigate();
  const { trackingMode, currentModel, setCurrentModel } = useApp();

  React.useEffect(() => {
    if (!trackingMode) {
      navigate('/mode-selection', { replace: true });
    }
  }, [trackingMode, navigate]);

  const isMarkerMode = trackingMode === TRACKING_MODES.MARKER;
  const hasValidModel = Boolean(currentModel?.url);

  const handleFileSelect = (uploadedFile) => {
    setCurrentModel({
      id: `custom-${Date.now()}`,
      name: uploadedFile.name,
      format: uploadedFile.format,
      url: uploadedFile.url,
    });
  };

  const handleStartAR = () => {
    if (hasValidModel) {
      navigate('/ar-view');
    }
  };

  return (
    <div className="model-import-page animate-fade-in">
      <div className="page-top-bar">
        <button className="page-back-btn" onClick={() => navigate('/mode-selection')}>
          <ArrowLeft size={16} />
          <span>Volver a Modo</span>
        </button>

        <div className={`page-mode-badge ${isMarkerMode ? 'marker' : 'plane'}`}>
          {isMarkerMode ? <Target size={14} /> : <Layers size={14} />}
          <span>
            Modo: <strong>{isMarkerMode ? 'Marcador' : 'Plano'}</strong>
          </span>
        </div>
      </div>

      <div className="step-header">
        <span className="step-badge-text">Paso 2 de 3 · Selección de Modelo 3D</span>
        <h2 className="step-title">Importar o Seleccionar Modelo 3D</h2>
      </div>

      <div className="model-import-grid">
        <div className="model-import-column">
          <FileUploader
            onFileSelect={handleFileSelect}
            activeFileName={currentModel?.id?.startsWith('custom-') ? currentModel.name : null}
          />

          <div className="glass-panel" style={{ padding: '24px' }}>
            <h4 style={{ fontSize: '1rem', fontWeight: '600', marginBottom: '12px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <FileCode size={18} color="var(--accent-primary)" />
              Recomendaciones de Formato 3D
            </h4>
            <ul
              style={{
                fontSize: '0.85rem',
                color: 'var(--text-secondary)',
                display: 'flex',
                flexDirection: 'column',
                gap: '8px',
                paddingLeft: '20px',
              }}
            >
              {FORMAT_RECOMMENDATIONS.map((item) => (
                <li key={item.ext}>
                  <strong>{item.ext}:</strong> {item.description}
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div className="model-import-column">
          <SampleSelector
            onSelectSample={setCurrentModel}
            activeSampleId={currentModel?.id}
          />

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
                    Tipo:{' '}
                    <strong style={{ color: 'var(--accent-primary)' }}>
                      {currentModel.format === 'preset'
                        ? 'MAQUETA INTEGRADA 3D'
                        : currentModel.format?.toUpperCase()}
                    </strong>
                  </p>
                </div>
              </div>
            ) : (
              <div className="glass-card" style={{ padding: '16px', display: 'flex', alignItems: 'center', gap: '12px', borderColor: 'rgba(234, 179, 8, 0.4)' }}>
                <AlertCircle size={24} color="#facc15" />
                <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
                  Aún no se ha seleccionado ningún modelo 3D. Elige un ejemplo o sube un archivo.
                </p>
              </div>
            )}

            <button
              className="btn-primary"
              onClick={handleStartAR}
              disabled={!hasValidModel}
              style={{
                width: '100%',
                justifyContent: 'center',
                padding: '14px',
                fontSize: '1.05rem',
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
