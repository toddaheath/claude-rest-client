import { mockApi } from './mockClient';

const API_BASE = `${import.meta.env.VITE_API_URL || ''}/api`;

let apiKey = localStorage.getItem('restward_api_key') || '';
let authToken = localStorage.getItem('restward_auth_token') || '';

export const isDemoMode = import.meta.env.VITE_DEMO_MODE === 'true';

export function setApiKey(key: string) {
  apiKey = key;
  localStorage.setItem('restward_api_key', key);
}

export function getApiKey(): string {
  return apiKey;
}

export function setAuthToken(token: string) {
  authToken = token;
  localStorage.setItem('restward_auth_token', token);
}

export function getAuthToken(): string {
  return authToken;
}

export function clearAuth() {
  apiKey = '';
  authToken = '';
  localStorage.removeItem('restward_api_key');
  localStorage.removeItem('restward_auth_token');
}

export function isAuthenticated(): boolean {
  return isDemoMode || !!authToken || !!apiKey;
}

async function request<T>(path: string, options: RequestInit = {}): Promise<T> {
  const headers: Record<string, string> = {
    ...(options.headers as Record<string, string> || {}),
  };

  if (authToken) {
    headers['Authorization'] = `Bearer ${authToken}`;
  } else if (apiKey) {
    headers['X-Api-Key'] = apiKey;
  }

  if (options.body && typeof options.body === 'string') {
    headers['Content-Type'] = headers['Content-Type'] || 'application/json';
  }

  const res = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  });

  if (!res.ok) {
    const error = await res.json().catch(() => ({ error: res.statusText }));
    throw new Error(error.error || `HTTP ${res.status}`);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

const realApi = {
  get: <T>(path: string) => request<T>(path),
  post: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'POST', body: body ? JSON.stringify(body) : undefined }),
  put: <T>(path: string, body?: unknown) =>
    request<T>(path, { method: 'PUT', body: body ? JSON.stringify(body) : undefined }),
  delete: (path: string) => request(path, { method: 'DELETE' }),
  postRaw: <T>(path: string, body: string, contentType = 'application/json') =>
    request<T>(path, {
      method: 'POST',
      body,
      headers: { 'Content-Type': contentType },
    }),
};

export const api = isDemoMode ? mockApi : realApi;
