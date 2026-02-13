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
      onMouseEnter={(e) => (e.currentTarget.style.background = '#2a2d2e')}
      onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
    >
      <div onClick={onToggle} style={{ display: 'flex', alignItems: 'center', gap: 6, flex: 1 }}>
        <span style={{ color: '#888', fontSize: 10 }}>{expanded ? '▼' : '▶'}</span>
        <span style={{ color: '#d4d4d4' }}>{name}</span>
        {isShared && <span style={{ color: '#569cd6', fontSize: 10 }}>shared</span>}
      </div>
      <button
        onClick={(e) => { e.stopPropagation(); onDelete(); }}
        style={{ background: 'none', border: 'none', color: '#666', cursor: 'pointer', fontSize: 14, padding: '0 4px' }}
        title="Delete collection"
      >
        ×
      </button>
    </div>
  );
}
