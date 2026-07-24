import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Box, Home, Target, FolderUp, Eye } from 'lucide-react';

export default function Navbar() {
  const navigate = useNavigate();
  const location = useLocation();

  const steps = [
    { path: '/', label: 'Inicio', icon: Home },
    { path: '/mode-selection', label: 'Modo AR', icon: Target },
    { path: '/model-import', label: 'Cargar Modelo', icon: FolderUp },
    { path: '/ar-view', label: 'Visor 3D/AR', icon: Eye }
  ];

  const getStepIndex = (path) => {
    switch (path) {
      case '/': return 0;
      case '/mode-selection': return 1;
      case '/model-import': return 2;
      case '/ar-view': return 3;
      default: return 0;
    }
  };

  const currentStep = getStepIndex(location.pathname);

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
      {/* Brand Header */}
      <div 
        onClick={() => navigate('/')}
        style={{ display: 'flex', alignItems: 'center', gap: '14px', cursor: 'pointer' }}
      >
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

      {/* Navigation Breadcrumb / Flow Steps */}
      <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
        {steps.map((step, idx) => {
          const Icon = step.icon;
          const isActive = location.pathname === step.path;
          const isPassed = currentStep > idx;

          return (
            <div key={step.path} style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
              <button
                onClick={() => navigate(step.path)}
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '6px',
                  padding: '6px 14px',
                  borderRadius: '20px',
                  background: isActive 
                    ? 'rgba(59, 130, 246, 0.2)' 
                    : isPassed 
                      ? 'rgba(255, 255, 255, 0.05)' 
                      : 'transparent',
                  border: isActive 
                    ? '1px solid var(--accent-primary)' 
                    : '1px solid transparent',
                  color: isActive 
                    ? '#ffffff' 
                    : isPassed 
                      ? 'var(--text-primary)' 
                      : 'var(--text-muted)',
                  fontSize: '0.82rem',
                  fontWeight: isActive ? '600' : '400',
                  cursor: 'pointer',
                  transition: 'all 0.2s ease'
                }}
              >
                <Icon size={15} color={isActive ? 'var(--accent-primary)' : 'currentColor'} />
                <span>{step.label}</span>
              </button>
              {idx < steps.length - 1 && (
                <span style={{ color: 'var(--text-muted)', fontSize: '0.8rem' }}>/</span>
              )}
            </div>
          );
        })}
      </div>
    </nav>
  );
}
