import { useCallback } from 'react';
import { api } from '../api/client';
import { useStore } from '../store';
import type { ProxyRequest, ProxyResponse } from '../types';

export function useRequest() {
  const { activeRequest, setResponse, setLoading, environments, activeEnvironmentId } = useStore();

  const resolveVariables = useCallback(
    (text: string): string => {
      if (!activeEnvironmentId) return text;
      const env = environments.find((e) => e.id === activeEnvironmentId);
      if (!env) return text;

      let resolved = text;
      for (const v of env.variables) {
        if (v.enabled) {
          resolved = resolved.replaceAll(`{{${v.key}}}`, v.value);
        }
      }
      return resolved;
    },
    [environments, activeEnvironmentId]
  );

  const sendRequest = useCallback(async () => {
    setLoading(true);
    setResponse(null);

    try {
      const headers: Record<string, string> = {};
      for (const h of activeRequest.headers) {
        if (h.enabled && h.key.trim()) {
          headers[resolveVariables(h.key)] = resolveVariables(h.value);
        }
      }

      const proxyReq: ProxyRequest = {
        method: activeRequest.method,
        url: resolveVariables(activeRequest.url),
        headers,
        body: activeRequest.body ? resolveVariables(activeRequest.body) : undefined,
        body_content_type: activeRequest.bodyContentType || undefined,
      };

      const response = await api.post<ProxyResponse>('/proxy', proxyReq);
      setResponse(response);
    } catch (err) {
      setResponse({
        status_code: 0,
        status_text: err instanceof Error ? err.message : 'Request failed',
        headers: {},
        body: '',
        elapsed_ms: 0,
        size_bytes: 0,
      });
    } finally {
      setLoading(false);
    }
  }, [activeRequest, setResponse, setLoading, resolveVariables]);

  return { sendRequest, resolveVariables };
}
