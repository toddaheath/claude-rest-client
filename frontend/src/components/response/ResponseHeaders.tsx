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
            <th style={{ textAlign: 'left', padding: '6px 8px', color: '#888', borderBottom: '1px solid #333' }}>Header</th>
            <th style={{ textAlign: 'left', padding: '6px 8px', color: '#888', borderBottom: '1px solid #333' }}>Value</th>
          </tr>
        </thead>
        <tbody>
          {entries.map(([key, value]) => (
            <tr key={key}>
              <td style={{ padding: '6px 8px', color: '#569cd6', borderBottom: '1px solid #2d2d2d' }}>{key}</td>
              <td style={{ padding: '6px 8px', color: '#d4d4d4', borderBottom: '1px solid #2d2d2d', wordBreak: 'break-all' }}>{value}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
