import React, { useState } from 'react';
import { AppContext } from './AppContext';

export function AppProvider({ children }) {
  const [trackingMode, setTrackingMode] = useState('plane'); // 'plane' | 'marker'
  const [currentModel, setCurrentModel] = useState({
    id: 'sample-house',
    name: 'Casa Moderna con Piscina (Maqueta)',
    format: 'preset',
    url: 'preset:house'
  });

  const resetSelection = () => {
    setTrackingMode('plane');
    setCurrentModel({
      id: 'sample-house',
      name: 'Casa Moderna con Piscina (Maqueta)',
      format: 'preset',
      url: 'preset:house'
    });
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
