import { DEMO_COLLECTIONS, DEMO_ENVIRONMENTS, DEMO_PROXY_RESPONSE } from './mockData';

function delay(ms = 100): Promise<void> {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

async function mockGet<T>(path: string): Promise<T> {
  await delay();

  if (path === '/collections') return DEMO_COLLECTIONS as T;
  if (path.startsWith('/collections/')) return DEMO_COLLECTIONS[0] as T;
  if (path === '/environments') return DEMO_ENVIRONMENTS as T;
  if (path.startsWith('/environments/')) return DEMO_ENVIRONMENTS[0] as T;

  return {} as T;
}

async function mockPost<T>(path: string): Promise<T> {
  await delay();

  if (path === '/proxy') return DEMO_PROXY_RESPONSE as T;

  if (path.startsWith('/collections')) return DEMO_COLLECTIONS[0] as T;
  if (path.startsWith('/environments')) return DEMO_ENVIRONMENTS[0] as T;

  return {} as T;
}

async function mockMutate<T>(): Promise<T> {
  await delay();
  return {} as T;
}

async function mockDelete(): Promise<void> {
  await delay();
}

export const mockApi = {
  get: <T>(path: string) => mockGet<T>(path),
  post: <T>(path: string) => mockPost<T>(path),
  put: <T>() => mockMutate<T>(),
  delete: () => mockDelete(),
  postRaw: <T>(path: string) => mockPost<T>(path) as Promise<T>,
};
