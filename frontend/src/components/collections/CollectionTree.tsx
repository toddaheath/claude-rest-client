import { useState, useEffect } from 'react';
import { api } from '../../api/client';
import { useCollections } from '../../hooks/useCollections';
import type { Collection, RequestItem } from '../../types';
import { CollectionItem } from './CollectionItem';
import { FolderItem } from './FolderItem';

interface Props {
  collection: Collection;
}

export function CollectionTree({ collection }: Props) {
  const [expanded, setExpanded] = useState(false);
  const [detail, setDetail] = useState<Collection | null>(null);
  const { loadRequest, deleteCollection } = useCollections();

  useEffect(() => {
    if (expanded && !detail) {
      api.get<Collection>(`/collections/${collection.id}`).then(setDetail).catch(console.error);
    }
  }, [expanded, detail, collection.id]);

  return (
    <div>
      <CollectionItem
        name={collection.name}
        isShared={collection.is_shared}
        expanded={expanded}
        onToggle={() => setExpanded(!expanded)}
        onDelete={() => deleteCollection(collection.id)}
      />
      {expanded && detail && (
        <div style={{ paddingLeft: 16 }}>
          {detail.folders?.map((folder) => (
            <FolderItem key={folder.id} folder={folder} onSelectRequest={loadRequest} />
          ))}
          {detail.requests?.map((req) => (
            <RequestEntry key={req.id} request={req} onSelect={loadRequest} />
          ))}
        </div>
      )}
    </div>
  );
}

function RequestEntry({ request, onSelect }: { request: RequestItem; onSelect: (id: string) => void }) {
  const methodColors: Record<string, string> = {
    GET: '#61affe', POST: '#49cc90', PUT: '#fca130', PATCH: '#50e3c2',
    DELETE: '#f93e3e', HEAD: '#9012fe', OPTIONS: '#0d5aa7',
  };

  return (
    <div
      onClick={() => onSelect(request.id)}
      style={{
        display: 'flex', alignItems: 'center', gap: 8, padding: '4px 12px',
        cursor: 'pointer', fontSize: 13, borderRadius: 4,
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
