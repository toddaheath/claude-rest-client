import { useState } from 'react';
import { useStore } from '../../store';
import { ResponseBody } from './ResponseBody';
import { ResponseHeaders } from './ResponseHeaders';

type Tab = 'body' | 'headers';

function statusColor(code: number): string {
  if (code === 0) return '#f93e3e';
  if (code < 300) return '#49cc90';
  if (code < 400) return '#fca130';
  return '#f93e3e';
}

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function ResponsePanel() {
  const { response, isLoading } = useStore();
  const [tab, setTab] = useState<Tab>('body');

  if (isLoading) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', color: 'var(--color-text-secondary)' }}>
        Sending request...
      </div>
    );
  }

  if (!response) {
    return (
      <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', color: 'var(--color-text-muted)' }}>
        Send a request to see the response
      </div>
    );
  }

  const headerCount = Object.keys(response.headers).length;

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <div style={{
        display: 'flex', alignItems: 'center', gap: 16, padding: '8px 16px',
        borderBottom: '1px solid var(--color-border-primary)', fontSize: 13,
      }}>
        <span style={{ color: statusColor(response.status_code), fontWeight: 700 }}>
          {response.status_code} {response.status_text}
        </span>
        <span style={{ color: 'var(--color-text-secondary)' }}>{response.elapsed_ms} ms</span>
        <span style={{ color: 'var(--color-text-secondary)' }}>{formatBytes(response.size_bytes)}</span>

        <div style={{ marginLeft: 'auto', display: 'flex', gap: 0 }}>
          {(['body', 'headers'] as Tab[]).map((t) => (
            <button
              key={t}
              onClick={() => setTab(t)}
              style={{
                padding: '4px 12px', background: 'none', border: 'none',
                borderBottom: tab === t ? '2px solid var(--color-accent)' : '2px solid transparent',
                color: tab === t ? 'var(--color-text-bright)' : 'var(--color-text-secondary)', cursor: 'pointer', fontSize: 13,
              }}
            >
              {t === 'body' ? 'Body' : `Headers (${headerCount})`}
            </button>
          ))}
        </div>
      </div>
      <div style={{ flex: 1, overflow: 'auto' }}>
        {tab === 'body' && <ResponseBody body={response.body} headers={response.headers} />}
        {tab === 'headers' && <ResponseHeaders headers={response.headers} />}
      </div>
    </div>
  );
}
