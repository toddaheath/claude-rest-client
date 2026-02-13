import { useState } from 'react';
import { useStore } from '../../store';
import { UrlBar } from './UrlBar';
import { KeyValueEditor } from './KeyValueEditor';
import { BodyEditor } from './BodyEditor';

type Tab = 'params' | 'headers' | 'body';

export function RequestPanel() {
  const { activeRequest, setActiveRequest } = useStore();
  const [tab, setTab] = useState<Tab>('params');

  const tabs: { key: Tab; label: string }[] = [
    { key: 'params', label: `Params (${activeRequest.parameters.filter(p => p.key.trim()).length})` },
    { key: 'headers', label: `Headers (${activeRequest.headers.filter(h => h.key.trim()).length})` },
    { key: 'body', label: 'Body' },
  ];

  return (
    <div style={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <UrlBar />
      <div style={{ display: 'flex', borderBottom: '1px solid #333', paddingLeft: 16 }}>
        {tabs.map((t) => (
          <button
            key={t.key}
            onClick={() => setTab(t.key)}
            style={{
              padding: '8px 16px', background: 'none', border: 'none',
              borderBottom: tab === t.key ? '2px solid #0078d4' : '2px solid transparent',
              color: tab === t.key ? '#fff' : '#888', cursor: 'pointer', fontSize: 13,
            }}
          >
            {t.label}
          </button>
        ))}
      </div>
      <div style={{ flex: 1, overflow: 'auto' }}>
        {tab === 'params' && (
          <KeyValueEditor
            items={activeRequest.parameters}
            onChange={(parameters) => setActiveRequest({ parameters })}
            keyPlaceholder="Parameter"
            valuePlaceholder="Value"
          />
        )}
        {tab === 'headers' && (
          <KeyValueEditor
            items={activeRequest.headers}
            onChange={(headers) => setActiveRequest({ headers })}
            keyPlaceholder="Header"
            valuePlaceholder="Value"
          />
        )}
        {tab === 'body' && <BodyEditor />}
      </div>
    </div>
  );
}
