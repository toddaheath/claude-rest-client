import { useState } from 'react';
import type { Folder, RequestItem } from '../../types';

interface Props {
  folder: Folder;
  onSelectRequest: (id: string) => void;
}

const methodColors: Record<string, string> = {
  GET: '#61affe', POST: '#49cc90', PUT: '#fca130', PATCH: '#50e3c2',
  DELETE: '#f93e3e', HEAD: '#9012fe', OPTIONS: '#0d5aa7',
};

export function FolderItem({ folder, onSelectRequest }: Props) {
  const [expanded, setExpanded] = useState(false);

  return (
    <div>
      <div
        onClick={() => setExpanded(!expanded)}
        style={{
          display: 'flex', alignItems: 'center', gap: 6, padding: '4px 12px',
          cursor: 'pointer', fontSize: 13,
        }}
        onMouseEnter={(e) => (e.currentTarget.style.background = '#2a2d2e')}
        onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
      >
        <span style={{ color: '#888', fontSize: 10 }}>{expanded ? '▼' : '▶'}</span>
        <span style={{ color: '#ccb563' }}>{folder.name}</span>
      </div>
      {expanded && (
        <div style={{ paddingLeft: 16 }}>
          {folder.sub_folders?.map((sub) => (
            <FolderItem key={sub.id} folder={sub} onSelectRequest={onSelectRequest} />
          ))}
          {folder.requests?.map((req) => (
            <RequestRow key={req.id} request={req} onSelect={onSelectRequest} />
          ))}
        </div>
      )}
    </div>
  );
}

function RequestRow({ request, onSelect }: { request: RequestItem; onSelect: (id: string) => void }) {
  return (
    <div
      onClick={() => onSelect(request.id)}
      style={{
        display: 'flex', alignItems: 'center', gap: 8, padding: '4px 12px',
        cursor: 'pointer', fontSize: 13,
      }}
      onMouseEnter={(e) => (e.currentTarget.style.background = '#2a2d2e')}
      onMouseLeave={(e) => (e.currentTarget.style.background = 'transparent')}
    >
      <span style={{ color: methodColors[request.method] || '#888', fontSize: 10, fontWeight: 700, minWidth: 36 }}>
        {request.method}
      </span>
      <span style={{ color: '#d4d4d4', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
        {request.name}
      </span>
    </div>
  );
}
