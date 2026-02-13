import { useState } from 'react';
import { useStore } from '../../store';
import { EnvironmentEditor } from './EnvironmentEditor';

export function EnvironmentSelector() {
  const { environments, activeEnvironmentId, setActiveEnvironmentId } = useStore();
  const [showEditor, setShowEditor] = useState(false);

  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
      <select
        value={activeEnvironmentId || ''}
        onChange={(e) => setActiveEnvironmentId(e.target.value || null)}
        style={{
          padding: '4px 8px', background: '#2d2d2d', color: '#d4d4d4',
          border: '1px solid #444', borderRadius: 4, fontSize: 12, outline: 'none',
        }}
      >
        <option value="">No Environment</option>
        {environments.map((env) => (
          <option key={env.id} value={env.id}>{env.name}</option>
        ))}
      </select>
      <button
        onClick={() => setShowEditor(true)}
        style={{
          background: 'none', border: '1px solid #444', color: '#d4d4d4',
          borderRadius: 4, padding: '4px 8px', cursor: 'pointer', fontSize: 12,
        }}
      >
        Manage
      </button>
      {showEditor && <EnvironmentEditor onClose={() => setShowEditor(false)} />}
    </div>
  );
}
