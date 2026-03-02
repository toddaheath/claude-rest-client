import type { KeyValuePair } from '../../types';
import { newKeyValuePair } from '../../store';

interface Props {
  items: KeyValuePair[];
  onChange: (items: KeyValuePair[]) => void;
  keyPlaceholder?: string;
  valuePlaceholder?: string;
}

export function KeyValueEditor({ items, onChange, keyPlaceholder = 'Key', valuePlaceholder = 'Value' }: Props) {
  const update = (index: number, field: Partial<KeyValuePair>) => {
    const updated = items.map((item, i) => (i === index ? { ...item, ...field } : item));
    onChange(updated);
  };

  const remove = (index: number) => {
    const updated = items.filter((_, i) => i !== index);
    onChange(updated.length > 0 ? updated : [newKeyValuePair()]);
  };

  const addRow = () => {
    onChange([...items, newKeyValuePair()]);
  };

  return (
    <div style={{ padding: '8px 16px' }}>
      {items.map((item, index) => (
        <div key={item.id} style={{ display: 'flex', gap: 8, marginBottom: 4, alignItems: 'center' }}>
          <input
            type="checkbox"
            checked={item.enabled}
            onChange={(e) => update(index, { enabled: e.target.checked })}
            style={{ accentColor: 'var(--color-accent)' }}
          />
          <input
            value={item.key}
            onChange={(e) => update(index, { key: e.target.value })}
            placeholder={keyPlaceholder}
            style={{
              flex: 1, padding: '6px 8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
              border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
            }}
          />
          <input
            value={item.value}
            onChange={(e) => update(index, { value: e.target.value })}
            placeholder={valuePlaceholder}
            style={{
              flex: 1, padding: '6px 8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
              border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
            }}
          />
          <button
            onClick={() => remove(index)}
            style={{
              background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer',
              fontSize: 16, padding: '2px 6px',
            }}
          >
            ×
          </button>
        </div>
      ))}
      <button
        onClick={addRow}
        style={{
          background: 'none', border: 'none', color: 'var(--color-accent)', cursor: 'pointer',
          fontSize: 13, padding: '4px 0', marginTop: 4,
        }}
      >
        + Add row
      </button>
    </div>
  );
}
