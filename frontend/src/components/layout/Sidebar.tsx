import { useState } from 'react';
import { useStore } from '../../store';
import { useCollections } from '../../hooks/useCollections';
import { CollectionTree } from '../collections/CollectionTree';
import { ImportDialog } from '../import-export/ImportDialog';
import { ExportDialog } from '../import-export/ExportDialog';

export function Sidebar() {
  const { collections } = useStore();
  const { createCollection } = useCollections();
  const [showImport, setShowImport] = useState(false);
  const [showExport, setShowExport] = useState(false);
  const [newName, setNewName] = useState('');
  const [showNewInput, setShowNewInput] = useState(false);

  const handleCreate = async () => {
    if (newName.trim()) {
      await createCollection(newName.trim());
      setNewName('');
      setShowNewInput(false);
    }
  };

  return (
    <div style={{
      width: 280, background: 'var(--color-bg-secondary)', borderRight: '1px solid var(--color-border-primary)',
      display: 'flex', flexDirection: 'column', overflow: 'hidden',
    }}>
      <div style={{
        padding: '12px 16px', borderBottom: '1px solid var(--color-border-primary)',
        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      }}>
        <span style={{ fontWeight: 600, fontSize: 13, textTransform: 'uppercase', color: 'var(--color-text-secondary)' }}>
          Collections
        </span>
        <div style={{ display: 'flex', gap: 4 }}>
          <button
            onClick={() => setShowNewInput(!showNewInput)}
            title="New Collection"
            style={{ background: 'none', border: 'none', color: 'var(--color-text-primary)', cursor: 'pointer', fontSize: 16 }}
          >+</button>
          <button
            onClick={() => setShowImport(true)}
            title="Import"
            style={{ background: 'none', border: 'none', color: 'var(--color-text-primary)', cursor: 'pointer', fontSize: 12 }}
          >Import</button>
          <button
            onClick={() => setShowExport(true)}
            title="Export"
            style={{ background: 'none', border: 'none', color: 'var(--color-text-primary)', cursor: 'pointer', fontSize: 12 }}
          >Export</button>
        </div>
      </div>

      {showNewInput && (
        <div style={{ padding: '8px 16px', borderBottom: '1px solid var(--color-border-primary)' }}>
          <input
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleCreate()}
            placeholder="Collection name"
            autoFocus
            style={{
              width: '100%', padding: '6px 8px', background: 'var(--color-bg-elevated)', border: '1px solid var(--color-border-tertiary)',
              borderRadius: 4, color: 'var(--color-text-bright)', fontSize: 13, outline: 'none', boxSizing: 'border-box',
            }}
          />
        </div>
      )}

      <div style={{ flex: 1, overflow: 'auto', padding: '8px 0' }}>
        {collections.map((c) => (
          <CollectionTree key={c.id} collection={c} />
        ))}
        {collections.length === 0 && (
          <p style={{ padding: '16px', color: 'var(--color-text-muted)', fontSize: 13, textAlign: 'center' }}>
            No collections yet
          </p>
        )}
      </div>

      {showImport && <ImportDialog onClose={() => setShowImport(false)} />}
      {showExport && <ExportDialog onClose={() => setShowExport(false)} />}
    </div>
  );
}
