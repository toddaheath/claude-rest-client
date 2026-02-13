import { useEffect, useState } from 'react';
import { useStore } from '../../store';
import { getApiKey, setApiKey } from '../../api/client';
import { useCollections } from '../../hooks/useCollections';
import { useEnvironments } from '../../hooks/useEnvironments';
import { Sidebar } from './Sidebar';
import { TopBar } from './TopBar';
import { RequestPanel } from '../request/RequestPanel';
import { ResponsePanel } from '../response/ResponsePanel';

export function AppLayout() {
  const { sidebarOpen } = useStore();
  const { fetchCollections } = useCollections();
  const { fetchEnvironments } = useEnvironments();
  const [authenticated, setAuthenticated] = useState(!!getApiKey());
  const [keyInput, setKeyInput] = useState('');

  useEffect(() => {
    if (authenticated) {
      fetchCollections().catch(console.error);
      fetchEnvironments().catch(console.error);
    }
  }, [authenticated, fetchCollections, fetchEnvironments]);

  if (!authenticated) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100vh', background: '#1e1e1e', color: '#fff' }}>
        <div style={{ textAlign: 'center' }}>
          <h1 style={{ marginBottom: 24, fontSize: 28, fontWeight: 600 }}>Restward</h1>
          <p style={{ color: '#999', marginBottom: 16 }}>Enter your API key to continue</p>
          <input
            type="password"
            value={keyInput}
            onChange={(e) => setKeyInput(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter' && keyInput.trim()) {
                setApiKey(keyInput.trim());
                setAuthenticated(true);
              }
            }}
            placeholder="API Key"
            style={{
              padding: '10px 16px', width: 320, background: '#2d2d2d', border: '1px solid #444',
              borderRadius: 6, color: '#fff', fontSize: 14, outline: 'none',
            }}
          />
          <br />
          <button
            onClick={() => { setApiKey(keyInput.trim()); setAuthenticated(true); }}
            style={{
              marginTop: 12, padding: '10px 32px', background: '#0078d4', color: '#fff',
              border: 'none', borderRadius: 6, cursor: 'pointer', fontSize: 14,
            }}
          >
            Connect
          </button>
        </div>
      </div>
    );
  }

  return (
    <div style={{ display: 'flex', height: '100vh', background: '#1e1e1e', color: '#d4d4d4' }}>
      {sidebarOpen && <Sidebar />}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
        <TopBar />
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
          <div style={{ flex: 1, overflow: 'auto', borderBottom: '1px solid #333' }}>
            <RequestPanel />
          </div>
          <div style={{ flex: 1, overflow: 'auto' }}>
            <ResponsePanel />
          </div>
        </div>
      </div>
    </div>
  );
}
