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
    <nav className="glass-panel navbar-container">
      {/* Brand Header */}
      <div 
        className="navbar-brand"
        onClick={() => navigate('/')}
      >
        <div className="navbar-logo">
          <Box size={22} color="#ffffff" />
        </div>
        <div className="navbar-title-group">
          <h1 className="navbar-title">
            ViMARA
          </h1>
          <p className="navbar-subtitle">
            Visualizador de Maquetas de Arquitectura en Realidad Aumentada
          </p>
        </div>
      </div>

      {/* Steps Progress Indicator (Mobile Friendly) */}
      <div className="navbar-steps">
        {steps.map((step, idx) => {
          const Icon = step.icon;
          const isActive = location.pathname === step.path;
          const isPassed = currentStep > idx;

          return (
            <React.Fragment key={step.path}>
              <button
                onClick={() => navigate(step.path)}
                className={`navbar-step-btn ${isActive ? 'active' : ''} ${isPassed ? 'passed' : ''}`}
                title={step.label}
              >
                <Icon size={16} />
                <span className="navbar-step-label">{step.label}</span>
              </button>
              {idx < steps.length - 1 && (
                <span className="navbar-step-divider">/</span>
              )}
            </React.Fragment>
          );
        })}
      </div>
    </nav>
  );
}
