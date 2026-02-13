import Editor from '@monaco-editor/react';
import { useStore } from '../../store';

const CONTENT_TYPES = [
  { label: 'JSON', value: 'application/json' },
  { label: 'XML', value: 'application/xml' },
  { label: 'HTML', value: 'text/html' },
  { label: 'Plain Text', value: 'text/plain' },
];

function getLanguage(contentType: string): string {
  if (contentType.includes('json')) return 'json';
  if (contentType.includes('xml')) return 'xml';
  if (contentType.includes('html')) return 'html';
  return 'plaintext';
}

export function BodyEditor() {
  const { activeRequest, setActiveRequest } = useStore();

  return (
    <div style={{ padding: '8px 16px', height: '100%', display: 'flex', flexDirection: 'column' }}>
      <div style={{ marginBottom: 8 }}>
        <select
          value={activeRequest.bodyContentType}
          onChange={(e) => setActiveRequest({ bodyContentType: e.target.value })}
          style={{
            padding: '4px 8px', background: '#2d2d2d', color: '#d4d4d4',
            border: '1px solid #444', borderRadius: 4, fontSize: 12, outline: 'none',
          }}
        >
          {CONTENT_TYPES.map((ct) => (
            <option key={ct.value} value={ct.value}>{ct.label}</option>
          ))}
        </select>
      </div>
      <div style={{ flex: 1, minHeight: 150, border: '1px solid #333', borderRadius: 4, overflow: 'hidden' }}>
        <Editor
          height="100%"
          language={getLanguage(activeRequest.bodyContentType)}
          value={activeRequest.body}
          onChange={(value) => setActiveRequest({ body: value || '' })}
          theme="vs-dark"
          options={{
            minimap: { enabled: false },
            fontSize: 13,
            lineNumbers: 'off',
            scrollBeyondLastLine: false,
            wordWrap: 'on',
            tabSize: 2,
          }}
        />
      </div>
    </div>
  );
}
