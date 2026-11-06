import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { Box, Home, Target, FolderUp, Eye } from 'lucide-react';

const NAV_STEPS = [
  { path: '/', label: 'Inicio', icon: Home },
  { path: '/mode-selection', label: 'Modo AR', icon: Target },
  { path: '/model-import', label: 'Cargar Modelo', icon: FolderUp },
  { path: '/ar-view', label: 'Visor 3D/AR', icon: Eye },
];

export default function Navbar() {
  const navigate = useNavigate();
  const location = useLocation();

  const currentStepIndex = Math.max(
    0,
    NAV_STEPS.findIndex((step) => step.path === location.pathname)
  );

  return (
    <nav className="glass-panel navbar-container">
      <div className="navbar-brand" onClick={() => navigate('/')}>
        <div className="navbar-logo">
          <Box size={22} color="#ffffff" />
        </div>
        <div className="navbar-title-group">
          <h1 className="navbar-title">ViMARA</h1>
        </div>
      </div>

      <div className="navbar-steps">
        {NAV_STEPS.map((step, idx) => {
          const Icon = step.icon;
          const isActive = location.pathname === step.path;
          const isPassed = currentStepIndex > idx;

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
              {idx < NAV_STEPS.length - 1 && (
                <span className="navbar-step-divider">/</span>
              )}
            </React.Fragment>
          );
        })}
      </div>
    </nav>
  );
}
