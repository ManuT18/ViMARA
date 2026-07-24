import React, { useState } from 'react';
import { AppContext } from './AppContext';

export function AppProvider({ children }) {
  const [trackingMode, setTrackingMode] = useState('plane'); // 'plane' | 'marker'
  const [currentModel, setCurrentModel] = useState({
    id: 'sample-helmet',
    name: 'Casco Futurista (PBR / Texturas)',
    format: 'glb',
    url: 'https://raw.githubusercontent.com/KhronosGroup/glTF-Sample-Models/main/2.0/DamagedHelmet/glTF-Binary/DamagedHelmet.glb'
  });

  const resetSelection = () => {
    setTrackingMode('plane');
    setCurrentModel(null);
  };

  return (
    <AppContext.Provider
      value={{
        trackingMode,
        setTrackingMode,
        currentModel,
        setCurrentModel,
        resetSelection
      }}
    >
      {children}
    </AppContext.Provider>
  );
}
