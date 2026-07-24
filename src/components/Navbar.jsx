import React from 'react';
import { Box, Sparkles, Layers, ShieldCheck } from 'lucide-react';

export default function Navbar() {
  return (
    <nav className="glass-panel" style={{
      margin: '16px 24px 0 24px',
      padding: '14px 28px',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      position: 'sticky',
      top: '16px',
      zIndex: 100
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: '14px' }}>
        <div style={{
          width: '42px',
          height: '42px',
          borderRadius: '12px',
          background: 'var(--accent-gradient)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '0 4px 15px var(--accent-glow)'
        }}>
          <Box size={24} color="#ffffff" />
        </div>
        <div>
          <h1 style={{ fontSize: '1.4rem', fontWeight: '700', letterSpacing: '-0.02em', lineHeight: 1.1 }}>
            ViMARA <span className="gradient-text">WebAR</span>
          </h1>
          <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
            Visualizador de Maquetas 3D para Arquitectura
          </p>
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'center', gap: '12px' }}>
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '6px',
          padding: '6px 12px',
          borderRadius: '20px',
          background: 'rgba(34, 197, 94, 0.1)',
          border: '1px solid rgba(34, 197, 94, 0.2)',
          color: '#4ade80',
          fontSize: '0.82rem',
          fontWeight: '500'
        }}>
          <ShieldCheck size={14} />
          <span>Vercel Live: vimara-3d</span>
        </div>
      </div>
    </nav>
  );
}
