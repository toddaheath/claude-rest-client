import { useCallback } from 'react';
import { api } from '../api/client';
import { useStore } from '../store';
import type { Environment, EnvironmentVariable } from '../types';

export function useEnvironments() {
  const { setEnvironments, setActiveEnvironmentId } = useStore();

  const fetchEnvironments = useCallback(async () => {
    const envs = await api.get<Environment[]>('/environments');
    setEnvironments(envs);
  }, [setEnvironments]);

  const createEnvironment = useCallback(
    async (name: string, variables: EnvironmentVariable[] = [], isShared = false) => {
      const env = await api.post<Environment>('/environments', {
        name,
        is_shared: isShared,
        variables,
      });
      await fetchEnvironments();
      return env;
    },
    [fetchEnvironments]
  );

  const updateEnvironment = useCallback(
    async (id: string, data: { name?: string; variables?: EnvironmentVariable[]; is_shared?: boolean }) => {
      await api.put(`/environments/${id}`, data);
      await fetchEnvironments();
    },
    [fetchEnvironments]
  );

  const deleteEnvironment = useCallback(
    async (id: string) => {
      await api.delete(`/environments/${id}`);
      const { activeEnvironmentId } = useStore.getState();
      if (activeEnvironmentId === id) setActiveEnvironmentId(null);
      await fetchEnvironments();
    },
    [fetchEnvironments, setActiveEnvironmentId]
  );

  return { fetchEnvironments, createEnvironment, updateEnvironment, deleteEnvironment };
}
