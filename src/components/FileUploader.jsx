import React, { useRef } from 'react';
import { UploadCloud, FileCode, CheckCircle2 } from 'lucide-react';

export default function FileUploader({ onFileSelect, activeFileName }) {
  const fileInputRef = useRef(null);

  const handleFileChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const extension = file.name.split('.').pop().toLowerCase();
      if (['glb', 'gltf', 'stl', 'obj'].includes(extension)) {
        const objectUrl = URL.createObjectURL(file);
        onFileSelect({
          name: file.name,
          url: objectUrl,
          format: extension
        });
      } else {
        alert('Por favor selecciona un archivo con extensión .glb, .stl u .obj (exportado de SketchUp)');
      }
    }
  };

  return (
    <div className="glass-panel" style={{ padding: '24px' }}>
      <h3 style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '12px' }}>
        Cargar Maqueta desde tu Dispositivo
      </h3>
      
      <div 
        onClick={() => fileInputRef.current?.click()}
        className="glass-card"
        style={{
          border: '2px dashed var(--border-glass)',
          padding: '32px 20px',
          textAlign: 'center',
          cursor: 'pointer',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '12px'
        }}
      >
        <div style={{
          width: '54px',
          height: '54px',
          borderRadius: '50%',
          background: 'rgba(59, 130, 246, 0.1)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--accent-primary)'
        }}>
          <UploadCloud size={28} />
        </div>

        <div>
          <p style={{ fontWeight: '600', fontSize: '0.95rem' }}>
            Toca aquí para seleccionar tu archivo 3D
          </p>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '4px' }}>
            Formatos admitidos: <strong>.GLB</strong> (Recomendado), <strong>.STL</strong>, <strong>.OBJ</strong>
          </p>
        </div>

        {activeFileName && (
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '6px',
            marginTop: '8px',
            padding: '6px 12px',
            borderRadius: 'var(--radius-sm)',
            background: 'rgba(59, 130, 246, 0.15)',
            color: 'var(--accent-primary)',
            fontSize: '0.85rem'
          }}>
            <CheckCircle2 size={16} />
            <span>Cargado: {activeFileName}</span>
          </div>
        )}

        <input 
          ref={fileInputRef}
          type="file" 
          accept=".glb,.gltf,.stl,.obj" 
          style={{ display: 'none' }}
          onChange={handleFileChange}
        />
      </div>
    </div>
  );
}
