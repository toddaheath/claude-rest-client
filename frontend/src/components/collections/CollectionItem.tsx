interface Props {
  name: string;
  isShared: boolean;
  expanded: boolean;
  onToggle: () => void;
  onDelete: () => void;
}

export function CollectionItem({ name, isShared, expanded, onToggle, onDelete }: Props) {
  return (
    <div
      style={{
        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
        padding: '6px 12px', cursor: 'pointer', fontSize: 13,
      }}
      onMouseEnter={(e) => (e.currentTarget.style.background = 'var(--color-bg-hover)')}
      onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
    >
      <div onClick={onToggle} style={{ display: 'flex', alignItems: 'center', gap: 6, flex: 1 }}>
        <span style={{ color: 'var(--color-text-secondary)', fontSize: 10 }}>{expanded ? '▼' : '▶'}</span>
        <span style={{ color: 'var(--color-text-primary)' }}>{name}</span>
        {isShared && <span style={{ color: 'var(--color-text-link)', fontSize: 10 }}>shared</span>}
      </div>
      <button
        onClick={(e) => { e.stopPropagation(); onDelete(); }}
        style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 14, padding: '0 4px' }}
        title="Delete collection"
      >
        ×
      </button>
    </div>
  );
}
