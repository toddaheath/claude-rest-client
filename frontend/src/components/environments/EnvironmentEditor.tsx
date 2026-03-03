import { useState, useRef, useEffect, useCallback } from 'react';
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

  const [envSaveStatus, setEnvSaveStatus] = useState<'idle' | 'saving' | 'saved' | 'error'>('idle');
  const autoSaveTimerRef = useRef<ReturnType<typeof setTimeout>>(undefined);
  const savedIndicatorTimerRef = useRef<ReturnType<typeof setTimeout>>(undefined);

  const doAutoSave = useCallback(async (id: string, name: string, vars: EnvironmentVariable[]) => {
    setEnvSaveStatus('saving');
    try {
      await updateEnvironment(id, { name, variables: vars });
      setEnvSaveStatus('saved');
      if (savedIndicatorTimerRef.current) clearTimeout(savedIndicatorTimerRef.current);
      savedIndicatorTimerRef.current = setTimeout(() => setEnvSaveStatus('idle'), 2000);
    } catch {
      setEnvSaveStatus('error');
    }
  }, [updateEnvironment]);

  useEffect(() => {
    // Auto-save when editVars or editName changes (only if an env is selected)
    if (!selectedId || !editName) return;

    if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    autoSaveTimerRef.current = setTimeout(() => {
      doAutoSave(selectedId, editName, editVars);
    }, 1500);

    return () => {
      if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
    };
  }, [editVars, editName, selectedId, doAutoSave]);

  useEffect(() => {
    return () => {
      if (autoSaveTimerRef.current) clearTimeout(autoSaveTimerRef.current);
      if (savedIndicatorTimerRef.current) clearTimeout(savedIndicatorTimerRef.current);
    };
  }, []);

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
        background: 'var(--color-overlay)', display: 'flex', justifyContent: 'center', alignItems: 'center',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          background: 'var(--color-bg-secondary)', borderRadius: 8, width: 700, maxHeight: '80vh',
          display: 'flex', overflow: 'hidden', border: '1px solid var(--color-border-secondary)',
        }}
      >
        <div style={{ width: 200, borderRight: '1px solid var(--color-border-primary)', display: 'flex', flexDirection: 'column' }}>
          <div style={{ padding: '12px', borderBottom: '1px solid var(--color-border-primary)', fontSize: 14, fontWeight: 600 }}>
            Environments
          </div>
          <div style={{ flex: 1, overflow: 'auto' }}>
            {environments.map((env) => (
              <div
                key={env.id}
                onClick={() => handleSelect(env)}
                style={{
                  padding: '8px 12px', cursor: 'pointer', fontSize: 13,
                  background: env.id === selectedId ? 'var(--color-bg-active)' : 'transparent',
                  display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                }}
              >
                <span>{env.name}</span>
                <button
                  onClick={(e) => { e.stopPropagation(); deleteEnvironment(env.id); }}
                  style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 14 }}
                >×</button>
              </div>
            ))}
          </div>
          <div style={{ padding: 8, borderTop: '1px solid var(--color-border-primary)', display: 'flex', gap: 4 }}>
            <input
              value={newName}
              onChange={(e) => setNewName(e.target.value)}
              onKeyDown={(e) => e.key === 'Enter' && handleCreate()}
              placeholder="New env"
              style={{
                flex: 1, padding: '4px 8px', background: 'var(--color-bg-elevated)', border: '1px solid var(--color-border-tertiary)',
                borderRadius: 4, color: 'var(--color-text-bright)', fontSize: 12, outline: 'none',
              }}
            />
            <button
              onClick={handleCreate}
              style={{
                padding: '4px 8px', background: 'var(--color-accent)', color: '#fff',
                border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12,
              }}
            >+</button>
          </div>
        </div>

        <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
          {selected ? (
            <>
              <div style={{ padding: 12, borderBottom: '1px solid var(--color-border-primary)', display: 'flex', gap: 8, alignItems: 'center' }}>
                <input
                  value={editName}
                  onChange={(e) => setEditName(e.target.value)}
                  style={{
                    flex: 1, padding: '6px 8px', background: 'var(--color-bg-elevated)', border: '1px solid var(--color-border-tertiary)',
                    borderRadius: 4, color: 'var(--color-text-bright)', fontSize: 14, outline: 'none',
                  }}
                />
                <button
                  onClick={handleSave}
                  style={{
                    padding: '6px 16px', background: 'var(--color-accent)', color: '#fff',
                    border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
                  }}
                >Save</button>
                {envSaveStatus === 'saving' && (
                  <span style={{ fontSize: 12, color: 'var(--color-text-muted)' }}>Saving...</span>
                )}
                {envSaveStatus === 'saved' && (
                  <span style={{ fontSize: 12, color: 'var(--color-success, #4caf50)' }}>Saved</span>
                )}
                {envSaveStatus === 'error' && (
                  <span style={{ fontSize: 12, color: 'var(--color-error)' }}>Save failed</span>
                )}
                <button
                  onClick={onClose}
                  style={{
                    padding: '6px 16px', background: 'var(--color-border-primary)', color: 'var(--color-text-bright)',
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
                      style={{ accentColor: 'var(--color-accent)' }}
                    />
                    <input
                      value={v.key}
                      onChange={(e) => updateVar(i, { key: e.target.value })}
                      placeholder="Variable"
                      style={{
                        flex: 1, padding: '6px 8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
                        border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
                      }}
                    />
                    <input
                      value={v.value}
                      onChange={(e) => updateVar(i, { value: e.target.value })}
                      placeholder="Value"
                      style={{
                        flex: 1, padding: '6px 8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
                        border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
                      }}
                    />
                    <button
                      onClick={() => removeVar(i)}
                      style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 16 }}
                    >×</button>
                  </div>
                ))}
                <button
                  onClick={addVariable}
                  style={{
                    background: 'none', border: 'none', color: 'var(--color-accent)',
                    cursor: 'pointer', fontSize: 13, padding: '4px 0', marginTop: 4,
                  }}
                >+ Add variable</button>
              </div>
            </>
          ) : (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flex: 1, color: 'var(--color-text-muted)' }}>
              Select or create an environment
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
