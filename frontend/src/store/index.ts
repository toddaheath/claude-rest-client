import { create } from 'zustand';
import type {
  Collection,
  Environment,
  KeyValuePair,
  ProxyResponse,
  HttpMethod,
} from '../types';

interface ActiveRequest {
  name: string;
  method: HttpMethod;
  url: string;
  headers: KeyValuePair[];
  parameters: KeyValuePair[];
  body: string;
  bodyContentType: string;
  collectionId?: string;
  folderId?: string;
  savedId?: string;
}

function newKeyValuePair(): KeyValuePair {
  return { id: crypto.randomUUID(), key: '', value: '', enabled: true };
}

function defaultRequest(): ActiveRequest {
  return {
    name: 'New Request',
    method: 'GET',
    url: '',
    headers: [newKeyValuePair()],
    parameters: [newKeyValuePair()],
    body: '',
    bodyContentType: 'application/json',
  };
}

interface AppState {
  activeRequest: ActiveRequest;
  response: ProxyResponse | null;
  isLoading: boolean;
  collections: Collection[];
  environments: Environment[];
  activeEnvironmentId: string | null;
  sidebarOpen: boolean;

  setActiveRequest: (req: Partial<ActiveRequest>) => void;
  resetRequest: () => void;
  setResponse: (res: ProxyResponse | null) => void;
  setLoading: (loading: boolean) => void;
  setCollections: (collections: Collection[]) => void;
  setEnvironments: (environments: Environment[]) => void;
  setActiveEnvironmentId: (id: string | null) => void;
  toggleSidebar: () => void;
}

export const useStore = create<AppState>((set) => ({
  activeRequest: defaultRequest(),
  response: null,
  isLoading: false,
  collections: [],
  environments: [],
  activeEnvironmentId: null,
  sidebarOpen: true,

  setActiveRequest: (req) =>
    set((state) => ({ activeRequest: { ...state.activeRequest, ...req } })),
  resetRequest: () => set({ activeRequest: defaultRequest(), response: null }),
  setResponse: (response) => set({ response }),
  setLoading: (isLoading) => set({ isLoading }),
  setCollections: (collections) => set({ collections }),
  setEnvironments: (environments) => set({ environments }),
  setActiveEnvironmentId: (activeEnvironmentId) => set({ activeEnvironmentId }),
  toggleSidebar: () => set((state) => ({ sidebarOpen: !state.sidebarOpen })),
}));

export { newKeyValuePair };
export type { ActiveRequest };
