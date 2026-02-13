import { useState } from 'react';
import { useStore } from '../../store';
import { useEnvironments } from '../../hooks/useEnvironments';
import type { Environment, EnvironmentVariable } from '../../types';

interface Props {
  onClose: () => void;
}

export function EnvironmentEditor({ onClose }: Props) {
  const { environments } = useStore();
  const { createEnvironment, updateEnvironment, deleteEnvironment } = useEnvironments();
  const [selectedId, setSelectedId] = useState<string | null>(environments[0]?.id || null);
  const [newName, setNewName] = useState('');
  const [editVars, setEditVars] = useState<EnvironmentVariable[]>([]);
  const [editName, setEditName] = useState('');

  const selected = environments.find((e) => e.id === selectedId);

  const handleSelect = (env: Environment) => {
    setSelectedId(env.id);
    setEditName(env.name);
    setEditVars([...env.variables]);
  };

  const handleCreate = async () => {
    if (newName.trim()) {
      const env = await createEnvironment(newName.trim());
      setNewName('');
      setSelectedId(env.id);
      setEditName(env.name);
      setEditVars([]);
    }
  };

  const handleSave = async () => {
    if (selectedId) {
      await updateEnvironment(selectedId, {
        name: editName,
        variables: editVars,
      });
    }
  };

  const addVariable = () => {
    setEditVars([...editVars, { id: crypto.randomUUID(), key: '', value: '', enabled: true }]);
  };

  const updateVar = (index: number, field: Partial<EnvironmentVariable>) => {
    setEditVars(editVars.map((v, i) => (i === index ? { ...v, ...field } : v)));
  };

  const removeVar = (index: number) => {
    setEditVars(editVars.filter((_, i) => i !== index));
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
          background: '#252526', borderRadius: 8, width: 700, maxHeight: '80vh',
          display: 'flex', overflow: 'hidden', border: '1px solid #444',
        }}
      >
        <div style={{ width: 200, borderRight: '1px solid #333', display: 'flex', flexDirection: 'column' }}>
          <div style={{ padding: '12px', borderBottom: '1px solid #333', fontSize: 14, fontWeight: 600 }}>
            Environments
          </div>
          <div style={{ flex: 1, overflow: 'auto' }}>
            {environments.map((env) => (
              <div
                key={env.id}
                onClick={() => handleSelect(env)}
                style={{
                  padding: '8px 12px', cursor: 'pointer', fontSize: 13,
                  background: env.id === selectedId ? '#37373d' : 'transparent',
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                }}
              >
                <span>{env.name}</span>
                <button
                  onClick={(e) => { e.stopPropagation(); deleteEnvironment(env.id); }}
                  style={{ background: 'none', border: 'none', color: '#666', cursor: 'pointer', fontSize: 14 }}
                >×</button>
              </div>
            ))}
          </div>
          <div style={{ padding: 8, borderTop: '1px solid #333', display: 'flex', gap: 4 }}>
            <input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleCreate()}
              placeholder="New env"
              style={{
                flex: 1, padding: '4px 8px', background: '#3c3c3c', border: '1px solid #555',
                borderRadius: 4, color: '#fff', fontSize: 12, outline: 'none',
              }}
            />
            <button
              onClick={handleCreate}
              style={{
                padding: '4px 8px', background: '#0078d4', color: '#fff',
                border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12,
              }}
            >+</button>
          </div>
        </div>

        <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
          {selected ? (
            <>
              <div style={{ padding: 12, borderBottom: '1px solid #333', display: 'flex', gap: 8, alignItems: 'center' }}>
                <input
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  style={{
                    flex: 1, padding: '6px 8px', background: '#3c3c3c', border: '1px solid #555',
                    borderRadius: 4, color: '#fff', fontSize: 14, outline: 'none',
                  }}
                />
                <button
                  onClick={handleSave}
                  style={{
                    padding: '6px 16px', background: '#0078d4', color: '#fff',
                    border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
                  }}
                >Save</button>
                <button
                  onClick={onClose}
                  style={{
                    padding: '6px 16px', background: '#333', color: '#fff',
                    border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
                  }}
                >Close</button>
              </div>
              <div style={{ flex: 1, overflow: 'auto', padding: 12 }}>
                {editVars.map((v, i) => (
                  <div key={v.id} style={{ display: 'flex', gap: 8, marginBottom: 4, alignItems: 'center' }}>
                    <input
                      type="checkbox"
                      checked={v.enabled}
                      onChange={(e) => updateVar(i, { enabled: e.target.checked })}
                      style={{ accentColor: '#0078d4' }}
                    />
                    <input
                      value={v.key}
                      onChange={(e) => updateVar(i, { key: e.target.value })}
                      placeholder="Variable"
                      style={{
                        flex: 1, padding: '6px 8px', background: '#2d2d2d', color: '#fff',
                        border: '1px solid #444', borderRadius: 4, fontSize: 13, outline: 'none',
                      }}
                    />
                    <input
                      value={v.value}
                      onChange={(e) => updateVar(i, { value: e.target.value })}
                      placeholder="Value"
                      style={{
                        flex: 1, padding: '6px 8px', background: '#2d2d2d', color: '#fff',
                        border: '1px solid #444', borderRadius: 4, fontSize: 13, outline: 'none',
                      }}
                    />
                    <button
                      onClick={() => removeVar(i)}
                      style={{ background: 'none', border: 'none', color: '#666', cursor: 'pointer', fontSize: 16 }}
                    >×</button>
                  </div>
                ))}
                <button
                  onClick={addVariable}
                  style={{
                    background: 'none', border: 'none', color: '#0078d4',
                    cursor: 'pointer', fontSize: 13, padding: '4px 0', marginTop: 4,
                  }}
                >+ Add variable</button>
              </div>
            </>
          ) : (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flex: 1, color: '#666' }}>
              Select or create an environment
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
