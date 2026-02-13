import { useStore } from '../../store';
import { useRequest } from '../../hooks/useRequest';
import { MethodSelector } from './MethodSelector';
import type { HttpMethod } from '../../types';

export function UrlBar() {
  const { activeRequest, setActiveRequest, isLoading } = useStore();
  const { sendRequest } = useRequest();

  return (
    <div style={{ display: 'flex', padding: '12px 16px', gap: 0 }}>
      <MethodSelector
        value={activeRequest.method}
        onChange={(method: HttpMethod) => setActiveRequest({ method })}
      />
      <input
        type="text"
        value={activeRequest.url}
        onChange={(e) => setActiveRequest({ url: e.target.value })}
        onKeyDown={(e) => e.key === 'Enter' && sendRequest()}
        placeholder="Enter request URL"
        style={{
          flex: 1, padding: '8px 12px', background: '#2d2d2d', color: '#fff',
          border: '1px solid #555', borderLeft: 'none', fontSize: 14, outline: 'none',
        }}
      />
      <button
        onClick={sendRequest}
        disabled={isLoading}
        style={{
          padding: '8px 24px', background: isLoading ? '#555' : '#0078d4',
          color: '#fff', border: 'none', borderRadius: '0 6px 6px 0',
          cursor: isLoading ? 'wait' : 'pointer', fontSize: 14, fontWeight: 600,
        }}
      >
        {isLoading ? 'Sending...' : 'Send'}
      </button>
    </div>
  );
}
