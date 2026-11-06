import React, { useRef, useState } from 'react';
import { UploadCloud, CheckCircle2, AlertCircle } from 'lucide-react';
import { SUPPORTED_EXTENSIONS } from '../constants/arConstants';

export default function FileUploader({ onFileSelect, activeFileName }) {
  const fileInputRef = useRef(null);
  const [errorMsg, setErrorMsg] = useState('');

  const handleFileChange = (e) => {
    setErrorMsg('');
    const file = e.target.files?.[0];
    if (!file) return;

    const parts = file.name.split('.');
    const extension = parts.length > 1 ? parts.pop().toLowerCase().trim() : '';

    if (SUPPORTED_EXTENSIONS.includes(extension)) {
      const objectUrl = URL.createObjectURL(file);
      onFileSelect({
        name: file.name,
        url: objectUrl,
        format: extension,
      });
    } else {
      setErrorMsg(
        `El archivo "${file.name}" no tiene un formato 3D compatible. Selecciona un archivo .GLB, .GLTF, .STL u .OBJ.`
      );
    }

    // Permitir volver a seleccionar el mismo archivo si el usuario lo desea
    e.target.value = '';
  };

  return (
    <div className="glass-panel" style={{ padding: '20px' }}>
      <h3 style={{ fontSize: '1.05rem', fontWeight: '700', marginBottom: '10px', color: 'var(--text-primary)' }}>
        Cargar Modelo desde tu Dispositivo
      </h3>

      <div
        onClick={() => fileInputRef.current?.click()}
        className="glass-card touch-card upload-dropzone"
      >
        <div className="upload-icon-circle">
          <UploadCloud size={28} />
        </div>

        <div>
          <p style={{ fontWeight: '600', fontSize: '0.95rem', color: 'var(--text-primary)' }}>
            Toca aquí para seleccionar tu archivo 3D
          </p>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '4px', lineHeight: 1.4 }}>
            Formatos admitidos: <strong style={{ color: 'var(--text-secondary)' }}>.GLB</strong>, <strong>.STL</strong>, <strong>.OBJ</strong>, <strong>.GLTF</strong>
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

        {errorMsg && (
          <div style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '8px',
            marginTop: '6px',
            padding: '8px 14px',
            borderRadius: 'var(--radius-md)',
            background: 'rgba(239, 68, 68, 0.1)',
            color: '#dc2626',
            fontSize: '0.8rem',
            fontWeight: '600',
            border: '1px solid rgba(239, 68, 68, 0.3)'
          }}>
            <AlertCircle size={16} />
            <span>{errorMsg}</span>
          </div>
        )}

        {/* IMPORTANTE PARA iOS / iPhone: No usar accept=".stl,.obj,.glb" porque iOS Safari 
            carece de mapeo UTI para .stl/.glb y los deshabilita (en gris) en la app Archivos. */}
        <input
          ref={fileInputRef}
          type="file"
          style={{ display: 'none' }}
          onChange={handleFileChange}
        />
      </div>
    </div>
  );
}
