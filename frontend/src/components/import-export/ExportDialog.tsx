import { useState } from 'react';
import { useStore } from '../../store';
import { getApiKey } from '../../api/client';

interface Props {
  onClose: () => void;
}

export function ExportDialog({ onClose }: Props) {
  const { collections } = useStore();
  const [format, setFormat] = useState<'postman' | 'insomnia'>('postman');
  const [selectedId, setSelectedId] = useState(collections[0]?.id || '');

  const handleExport = () => {
    if (!selectedId) return;
    const apiBase = import.meta.env.VITE_API_URL || '/api';
    const url = `${apiBase}/export/${format}/${selectedId}`;

    fetch(url, { headers: { 'X-Api-Key': getApiKey() } })
      .then((res) => res.blob())
      .then((blob) => {
        const a = document.createElement('a');
        a.href = URL.createObjectURL(blob);
        a.download = `collection-${format}.json`;
        a.click();
        URL.revokeObjectURL(a.href);
        onClose();
      })
      .catch(console.error);
  };

  return (
    <div
      style={{
        position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
        background: 'rgba(0,0,0,0.6)', display: 'flex', justifyContent: 'center', alignItems: 'center',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          background: '#252526', borderRadius: 8, padding: 24, width: 400,
          border: '1px solid #444',
        }}
      >
        <h3 style={{ margin: '0 0 16px', fontSize: 16 }}>Export Collection</h3>

        <div style={{ marginBottom: 16 }}>
          <label style={{ fontSize: 13, color: '#888', display: 'block', marginBottom: 4 }}>Collection</label>
          <select
            value={selectedId}
            onChange={(e) => setSelectedId(e.target.value)}
            style={{
              width: '100%', padding: '8px', background: '#2d2d2d', color: '#fff',
              border: '1px solid #444', borderRadius: 4, fontSize: 13, outline: 'none',
            }}
          >
            {collections.map((c) => (
              <option key={c.id} value={c.id}>{c.name}</option>
            ))}
          </select>
        </div>

        <div style={{ marginBottom: 16 }}>
          <label style={{ fontSize: 13, color: '#888', display: 'block', marginBottom: 4 }}>Format</label>
          <select
            value={format}
            onChange={(e) => setFormat(e.target.value as 'postman' | 'insomnia')}
            style={{
              width: '100%', padding: '8px', background: '#2d2d2d', color: '#fff',
              border: '1px solid #444', borderRadius: 4, fontSize: 13, outline: 'none',
            }}
          >
            <option value="postman">Postman (v2.1)</option>
            <option value="insomnia">Insomnia (v4)</option>
          </select>
        </div>

        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button
            onClick={onClose}
            style={{
              padding: '8px 16px', background: '#333', color: '#fff',
              border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
            }}
          >Cancel</button>
          <button
            onClick={handleExport}
            disabled={!selectedId}
            style={{
              padding: '8px 16px', background: '#0078d4', color: '#fff',
              border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
            }}
          >Export</button>
        </div>
      </div>
    </div>
  );
}
