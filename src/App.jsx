import React from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { AppProvider } from './context/AppProvider';
import Navbar from './components/Navbar';
import MainMenu from './pages/MainMenu';
import ModeSelection from './pages/ModeSelection';
import ModelImport from './pages/ModelImport';
import ARVisualization from './pages/ARVisualization';
import './App.css';

export default function App() {
  return (
    <AppProvider>
      <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
        <Navbar />

        <Routes>
          <Route path="/" element={<MainMenu />} />
          <Route path="/mode-selection" element={<ModeSelection />} />
          <Route path="/model-import" element={<ModelImport />} />
          <Route path="/ar-view" element={<ARVisualization />} />
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>
    </AppProvider>
  );
}
