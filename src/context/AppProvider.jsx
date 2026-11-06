import React, { useState, useCallback, useEffect, useRef, useMemo } from 'react';
import { AppContext } from './AppContext';

function isBlobUrl(url) {
  return typeof url === 'string' && url.startsWith('blob:');
}

export function AppProvider({ children }) {
  const [trackingMode, setTrackingMode] = useState(null);
  const [currentModel, setCurrentModelState] = useState(null);
  const previousBlobUrlRef = useRef(null);

  const revokePreviousBlobIfNeeded = useCallback((nextUrl) => {
    const prevUrl = previousBlobUrlRef.current;
    if (prevUrl && prevUrl !== nextUrl && isBlobUrl(prevUrl)) {
      URL.revokeObjectURL(prevUrl);
    }
    previousBlobUrlRef.current = isBlobUrl(nextUrl) ? nextUrl : null;
  }, []);

  const setCurrentModel = useCallback(
    (nextModel) => {
      revokePreviousBlobIfNeeded(nextModel?.url);
      setCurrentModelState(nextModel);
    },
    [revokePreviousBlobIfNeeded]
  );

  const resetSelection = useCallback(() => {
    revokePreviousBlobIfNeeded(null);
    setTrackingMode(null);
    setCurrentModelState(null);
  }, [revokePreviousBlobIfNeeded]);

  useEffect(() => {
    return () => {
      if (isBlobUrl(previousBlobUrlRef.current)) {
        URL.revokeObjectURL(previousBlobUrlRef.current);
      }
    };
  }, []);

  const contextValue = useMemo(
    () => ({
      trackingMode,
      setTrackingMode,
      currentModel,
      setCurrentModel,
      resetSelection,
    }),
    [trackingMode, currentModel, setCurrentModel, resetSelection]
  );

  return (
    <AppContext.Provider value={contextValue}>
      {children}
    </AppContext.Provider>
  );
}
