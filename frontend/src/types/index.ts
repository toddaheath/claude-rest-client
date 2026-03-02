export interface KeyValuePair {
  id: string;
  key: string;
  value: string;
  enabled: boolean;
}

export interface RequestItem {
  id: string;
  name: string;
  method: string;
  url: string;
  body?: string;
  body_content_type?: string;
  collection_id: string;
  folder_id?: string;
  sort_order: number;
  created_at: string;
  updated_at: string;
  headers: KeyValuePair[];
  parameters: KeyValuePair[];
}

export interface Folder {
  id: string;
  name: string;
  collection_id: string;
  parent_folder_id?: string;
  sort_order: number;
  sub_folders?: Folder[];
  requests?: RequestItem[];
}

export interface Collection {
  id: string;
  name: string;
  description?: string;
  is_shared: boolean;
  owner_id: string;
  created_at: string;
  updated_at: string;
  folders?: Folder[];
  requests?: RequestItem[];
}

export interface EnvironmentVariable {
  id: string;
  key: string;
  value: string;
  enabled: boolean;
}

export interface Environment {
  id: string;
  name: string;
  is_shared: boolean;
  owner_id: string;
  created_at: string;
  updated_at: string;
  variables: EnvironmentVariable[];
}

export interface ProxyRequest {
  method: string;
  url: string;
  headers?: Record<string, string>;
  body?: string;
  body_content_type?: string;
}

export interface ProxyResponse {
  status_code: number;
  status_text: string;
  headers: Record<string, string>;
  body: string;
  elapsed_ms: number;
  size_bytes: number;
}

export interface User {
  id: string;
  name: string;
  email?: string;
  is_admin: boolean;
  created_at: string;
}

export interface AuthResponse {
  token: string;
  user: User;
  api_key: string;
}

export interface Team {
  id: string;
  name: string;
  owner_id: string;
  created_at: string;
  members?: TeamMember[];
}

export interface TeamMember {
  id: string;
  user_id: string;
  user_name: string;
  user_email?: string;
  role: string;
  joined_at: string;
}

export interface TeamInvitation {
  id: string;
  team_id: string;
  team_name: string;
  email: string;
  invited_by_name: string;
  status: string;
  created_at: string;
  expires_at: string;
}

export type HttpMethod = 'GET' | 'POST' | 'PUT' | 'PATCH' | 'DELETE' | 'HEAD' | 'OPTIONS';
