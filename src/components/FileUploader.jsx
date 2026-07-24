import React, { useRef } from 'react';
import { UploadCloud, CheckCircle2 } from 'lucide-react';

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
    <div className="glass-panel" style={{ padding: '20px' }}>
      <h3 style={{ fontSize: '1.05rem', fontWeight: '700', marginBottom: '10px', color: 'var(--text-primary)' }}>
        Cargar Maqueta desde tu Dispositivo
      </h3>
      
      <div 
        onClick={() => fileInputRef.current?.click()}
        className="glass-card touch-card"
        style={{
          border: '2px dashed #cbd5e1',
          padding: '24px 16px',
          textAlign: 'center',
          cursor: 'pointer',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '12px',
          borderRadius: 'var(--radius-lg)',
          userSelect: 'none'
        }}
      >
        <div style={{
          width: '56px',
          height: '56px',
          borderRadius: '50%',
          background: 'var(--accent-light)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--accent-primary)',
          boxShadow: '0 4px 12px rgba(37, 99, 235, 0.12)'
        }}>
          <UploadCloud size={28} />
        </div>

        <div>
          <p style={{ fontWeight: '600', fontSize: '0.95rem', color: 'var(--text-primary)' }}>
            Toca aquí para seleccionar tu archivo 3D
          </p>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '4px', lineHeight: 1.4 }}>
            Formatos admitidos: <strong style={{ color: 'var(--text-secondary)' }}>.GLB</strong> (Recomendado), <strong>.STL</strong>, <strong>.OBJ</strong>
          </p>
        </div>

        {activeFileName && (
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '8px',
            marginTop: '6px',
            padding: '8px 14px',
            minHeight: '38px',
            borderRadius: 'var(--radius-md)',
            background: 'var(--accent-light)',
            color: '#1d4ed8',
            fontSize: '0.85rem',
            fontWeight: '600',
            border: '1px solid #bfdbfe'
          }}>
            <CheckCircle2 size={18} color="#1d4ed8" />
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
