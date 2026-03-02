interface Props {
  headers: Record<string, string>;
}

export function ResponseHeaders({ headers }: Props) {
  const entries = Object.entries(headers);

  return (
    <div style={{ padding: 16 }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: 13 }}>
        <thead>
          <tr>
            <th style={{ textAlign: 'left', padding: '6px 8px', color: 'var(--color-text-secondary)', borderBottom: '1px solid var(--color-border-primary)' }}>Header</th>
            <th style={{ textAlign: 'left', padding: '6px 8px', color: 'var(--color-text-secondary)', borderBottom: '1px solid var(--color-border-primary)' }}>Value</th>
          </tr>
        </thead>
        <tbody>
          {entries.map(([key, value]) => (
            <tr key={key}>
              <td style={{ padding: '6px 8px', color: 'var(--color-text-link)', borderBottom: '1px solid var(--color-bg-input)' }}>{key}</td>
              <td style={{ padding: '6px 8px', color: 'var(--color-text-primary)', borderBottom: '1px solid var(--color-bg-input)', wordBreak: 'break-all' }}>{value}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
