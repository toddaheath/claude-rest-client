import { useCallback } from 'react';
import { api } from '../api/client';
import { useStore } from '../store';
import type { Collection, RequestItem } from '../types';

export function useCollections() {
  const { setCollections, setActiveRequest } = useStore();

  const fetchCollections = useCallback(async () => {
    const collections = await api.get<Collection[]>('/collections');
    setCollections(collections);
  }, [setCollections]);

  const createCollection = useCallback(
    async (name: string, isShared = false) => {
      await api.post('/collections', { name, is_shared: isShared });
      await fetchCollections();
    },
    [fetchCollections]
  );

  const deleteCollection = useCallback(
    async (id: string) => {
      await api.delete(`/collections/${id}`);
      await fetchCollections();
    },
    [fetchCollections]
  );

  const loadRequest = useCallback(
    async (id: string) => {
      const req = await api.get<RequestItem>(`/requests/${id}`);
      setActiveRequest({
        name: req.name,
        method: req.method as 'GET',
        url: req.url,
        headers: req.headers.length > 0 ? req.headers : [{ id: crypto.randomUUID(), key: '', value: '', enabled: true }],
        parameters: req.parameters.length > 0 ? req.parameters : [{ id: crypto.randomUUID(), key: '', value: '', enabled: true }],
        body: req.body || '',
        bodyContentType: req.body_content_type || 'application/json',
        collectionId: req.collection_id,
        folderId: req.folder_id,
        savedId: req.id,
      });
    },
    [setActiveRequest]
  );

  const saveRequest = useCallback(
    async (collectionId: string, folderId?: string) => {
      const { activeRequest } = useStore.getState();
      const payload = {
        name: activeRequest.name,
        method: activeRequest.method,
        url: activeRequest.url,
        body: activeRequest.body || undefined,
        body_content_type: activeRequest.bodyContentType || undefined,
        folder_id: folderId,
        headers: activeRequest.headers.filter((h) => h.key.trim()),
        parameters: activeRequest.parameters.filter((p) => p.key.trim()),
      };

      if (activeRequest.savedId) {
        await api.put(`/requests/${activeRequest.savedId}`, payload);
      } else {
        const created = await api.post<RequestItem>(`/collections/${collectionId}/requests`, payload);
        setActiveRequest({ savedId: created.id, collectionId, folderId });
      }

      await fetchCollections();
    },
    [fetchCollections, setActiveRequest]
  );

  return { fetchCollections, createCollection, deleteCollection, loadRequest, saveRequest };
}
