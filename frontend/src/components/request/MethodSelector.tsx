import type { HttpMethod } from '../../types';

const METHODS: HttpMethod[] = ['GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'HEAD', 'OPTIONS'];

const METHOD_COLORS: Record<string, string> = {
  GET: '#61affe',
  POST: '#49cc90',
  PUT: '#fca130',
  PATCH: '#50e3c2',
  DELETE: '#f93e3e',
  HEAD: '#9012fe',
  OPTIONS: '#0d5aa7',
};

interface Props {
  value: HttpMethod;
  onChange: (method: HttpMethod) => void;
}

export function MethodSelector({ value, onChange }: Props) {
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value as HttpMethod)}
      style={{
        padding: '8px 12px', background: '#2d2d2d', color: METHOD_COLORS[value] || '#fff',
        border: '1px solid #555', borderRadius: '6px 0 0 6px', fontSize: 14,
        fontWeight: 700, cursor: 'pointer', outline: 'none', minWidth: 100,
      }}
    >
      {METHODS.map((m) => (
        <option key={m} value={m}>{m}</option>
      ))}
    </select>
  );
}
