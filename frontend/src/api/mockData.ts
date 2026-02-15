import type { Collection, Environment, ProxyResponse } from '../types';

export const DEMO_COLLECTIONS: Collection[] = [
  {
    id: 'demo-collection-1',
    name: 'Sample API',
    description: 'A demo collection showcasing Restward features',
    is_shared: false,
    owner_id: 'demo-user',
    created_at: '2025-01-01T00:00:00Z',
    updated_at: '2025-01-01T00:00:00Z',
    folders: [
      {
        id: 'demo-folder-1',
        name: 'Users',
        collection_id: 'demo-collection-1',
        sort_order: 0,
        requests: [
          {
            id: 'demo-req-2',
            name: 'Create User',
            method: 'POST',
            url: '{{base_url}}/users',
            body: JSON.stringify({ name: 'Jane Doe', email: 'jane@example.com' }, null, 2),
            body_content_type: 'application/json',
            collection_id: 'demo-collection-1',
            folder_id: 'demo-folder-1',
            sort_order: 0,
            created_at: '2025-01-01T00:00:00Z',
            updated_at: '2025-01-01T00:00:00Z',
            headers: [{ id: 'h2', key: 'Content-Type', value: 'application/json', enabled: true }],
            parameters: [],
          },
        ],
      },
    ],
    requests: [
      {
        id: 'demo-req-1',
        name: 'List Users',
        method: 'GET',
        url: '{{base_url}}/users',
        collection_id: 'demo-collection-1',
        sort_order: 0,
        created_at: '2025-01-01T00:00:00Z',
        updated_at: '2025-01-01T00:00:00Z',
        headers: [{ id: 'h1', key: 'Accept', value: 'application/json', enabled: true }],
        parameters: [{ id: 'p1', key: 'page', value: '1', enabled: true }],
      },
      {
        id: 'demo-req-3',
        name: 'Health Check',
        method: 'GET',
        url: '{{base_url}}/health',
        collection_id: 'demo-collection-1',
        sort_order: 1,
        created_at: '2025-01-01T00:00:00Z',
        updated_at: '2025-01-01T00:00:00Z',
        headers: [],
        parameters: [],
      },
    ],
  },
];

export const DEMO_ENVIRONMENTS: Environment[] = [
  {
    id: 'demo-env-1',
    name: 'Demo Environment',
    is_shared: false,
    owner_id: 'demo-user',
    created_at: '2025-01-01T00:00:00Z',
    updated_at: '2025-01-01T00:00:00Z',
    variables: [
      { id: 'v1', key: 'base_url', value: 'https://api.example.com', enabled: true },
      { id: 'v2', key: 'api_key', value: 'demo-key-12345', enabled: true },
    ],
  },
];

export const DEMO_PROXY_RESPONSE: ProxyResponse = {
  status_code: 200,
  status_text: 'OK',
  headers: {
    'content-type': 'application/json',
    'x-request-id': 'demo-abc123',
  },
  body: JSON.stringify(
    {
      data: [
        { id: 1, name: 'Alice', email: 'alice@example.com' },
        { id: 2, name: 'Bob', email: 'bob@example.com' },
      ],
      total: 2,
      page: 1,
    },
    null,
    2,
  ),
  elapsed_ms: 42,
  size_bytes: 156,
};
