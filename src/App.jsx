import React, { useState } from 'react';
import Navbar from './components/Navbar';
import ModelViewer from './components/ModelViewer';
import FileUploader from './components/FileUploader';
import SampleSelector from './components/SampleSelector';
import { Sparkles, Layers, Box, CheckCircle } from 'lucide-react';
import './App.css';

export default function App() {
  const [currentModel, setCurrentModel] = useState({
    id: 'sample-helmet',
    name: 'Casco Futurista (PBR / Texturas)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb'
  });

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

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <Navbar />

      <main style={{
        maxWidth: '1280px',
        margin: '0 auto',
        padding: '24px',
        width: '100%',
        display: 'grid',
        gridTemplateColumns: '1fr 380px',
        gap: '24px',
        alignItems: 'start'
      }} className="animate-fade-in">
        {/* Main 3D / WebAR View */}
        <section style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          <ModelViewer 
            modelUrl={currentModel.url}
            modelName={currentModel.name}
            format={currentModel.format}
          />
        </section>

        {/* Sidebar Controls */}
        <aside style={{ display: 'flex', flexDirection: 'column', gap: '20px' }}>
          <FileUploader 
            onFileSelect={handleFileSelect}
            activeFileName={currentModel.id.startsWith('custom-') ? currentModel.name : null}
          />

          <SampleSelector 
            onSelectSample={handleSelectSample}
            activeSampleId={currentModel.id}
          />

          {/* Quick Info Box */}
          <div className="glass-panel" style={{ padding: '20px' }}>
            <h4 style={{ fontSize: '0.95rem', fontWeight: '600', marginBottom: '8px', display: 'flex', alignItems: 'center', gap: '8px' }}>
              <Layers size={18} color="var(--accent-primary)" />
               Compatibilidad Móvil
            </h4>
            <p style={{ fontSize: '0.82rem', color: 'var(--text-secondary)' }}>
              - <strong>iOS (iPhone / iPad):</strong> Usa AR Quick Look nativo de Apple.<br />
              - <strong>Android:</strong> Usa Google Scene Viewer.<br />
              - <strong>Formatos:</strong> Soporta <code>.glb</code> (Fase 1 y 2), <code>.stl</code> y <code>.obj</code>.
            </p>
          </div>
        </aside>
      </main>
    </div>
  );
}
