import { useEffect, useRef } from 'react';
import { useStore } from '../store';
import { api } from '../api/client';

export function useAutoSave() {
  const timeoutRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const prevRequestRef = useRef<string>('');

  useEffect(() => {
    const unsubscribe = useStore.subscribe((state, prevState) => {
      const req = state.activeRequest;
      const prevReq = prevState.activeRequest;

      // Only auto-save if the request has been saved to a collection
      if (!req.savedId) return;

      // Skip if nothing changed on the active request
      if (req === prevReq) return;

      // Skip if only the savedId/collectionId/folderId changed (i.e., just loaded)
      const snapshot = JSON.stringify({
        name: req.name, method: req.method, url: req.url,
        headers: req.headers, parameters: req.parameters,
        body: req.body, bodyContentType: req.bodyContentType,
      });
      if (snapshot === prevRequestRef.current) return;
      prevRequestRef.current = snapshot;

      // Debounce the save
      if (timeoutRef.current) clearTimeout(timeoutRef.current);

      timeoutRef.current = setTimeout(async () => {
        const { setSaveStatus } = useStore.getState();
        setSaveStatus('saving');

        try {
          const current = useStore.getState().activeRequest;
          if (!current.savedId) return;

          await api.put(`/requests/${current.savedId}`, {
            name: current.name,
            method: current.method,
            url: current.url,
            body: current.body || undefined,
            body_content_type: current.bodyContentType || undefined,
            headers: current.headers.filter((h) => h.key.trim()),
            parameters: current.parameters.filter((p) => p.key.trim()),
          });

          setSaveStatus('saved');
          setTimeout(() => {
            const { saveStatus } = useStore.getState();
            if (saveStatus === 'saved') setSaveStatus('idle');
          }, 2000);
        } catch {
          useStore.getState().setSaveStatus('error');
        }
      }, 1500);
    });

    return () => {
      unsubscribe();
      if (timeoutRef.current) clearTimeout(timeoutRef.current);
    };
  }, []);
}
