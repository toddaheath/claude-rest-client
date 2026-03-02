import { useState } from 'react';
import { api } from '../../api/client';
import { useCollections } from '../../hooks/useCollections';

interface Props {
  onClose: () => void;
}

export function ImportDialog({ onClose }: Props) {
  const [format, setFormat] = useState<'postman' | 'insomnia'>('postman');
  const [file, setFile] = useState<File | null>(null);
  const [importing, setImporting] = useState(false);
  const [error, setError] = useState('');
  const { fetchCollections } = useCollections();

  const handleImport = async () => {
    if (!file) return;
    setImporting(true);
    setError('');
    try {
      const text = await file.text();
      await api.postRaw(`/import/${format}`, text);
      await fetchCollections();
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Import failed');
    } finally {
      setImporting(false);
    }
  };

  return (
    <div
      style={{
        position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
        background: 'var(--color-overlay)', display: 'flex', justifyContent: 'center', alignItems: 'center',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          background: 'var(--color-bg-secondary)', borderRadius: 8, padding: 24, width: 400,
          border: '1px solid var(--color-border-secondary)',
        }}
      >
        <h3 style={{ margin: '0 0 16px', fontSize: 16 }}>Import Collection</h3>

        <div style={{ marginBottom: 16 }}>
          <label style={{ fontSize: 13, color: 'var(--color-text-secondary)', display: 'block', marginBottom: 4 }}>Format</label>
          <select
            value={format}
            onChange={(e) => setFormat(e.target.value as 'postman' | 'insomnia')}
            style={{
              width: '100%', padding: '8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
              border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
            }}
          >
            <option value="postman">Postman (v2.1)</option>
            <option value="insomnia">Insomnia (v4)</option>
          </select>
        </div>

        <div style={{ marginBottom: 16 }}>
          <label style={{ fontSize: 13, color: 'var(--color-text-secondary)', display: 'block', marginBottom: 4 }}>File</label>
          <input
            type="file"
            accept=".json"
            onChange={(e) => setFile(e.target.files?.[0] || null)}
            style={{ fontSize: 13, color: 'var(--color-text-primary)' }}
          />
        </div>

        {error && <p style={{ color: 'var(--color-error)', fontSize: 13, marginBottom: 12 }}>{error}</p>}

        <div style={{ display: 'flex', gap: 8, justifyContent: 'flex-end' }}>
          <button
            onClick={onClose}
            style={{
              padding: '8px 16px', background: 'var(--color-border-primary)', color: 'var(--color-text-bright)',
              border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
            }}
          >Cancel</button>
          <button
            onClick={handleImport}
            disabled={!file || importing}
            style={{
              padding: '8px 16px', background: 'var(--color-accent)', color: '#fff',
              border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
              opacity: !file || importing ? 0.5 : 1,
            }}
          >{importing ? 'Importing...' : 'Import'}</button>
        </div>
      </div>
    </div>
  );
}
