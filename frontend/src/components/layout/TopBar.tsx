import { useStore } from '../../store';
import { EnvironmentSelector } from '../environments/EnvironmentSelector';

export function TopBar() {
  const { toggleSidebar } = useStore();

  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '8px 16px', background: '#252526', borderBottom: '1px solid #333',
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <button
          onClick={toggleSidebar}
          style={{
            background: 'none', border: 'none', color: '#d4d4d4', cursor: 'pointer',
            fontSize: 18, padding: '4px 8px',
          }}
        >
          ☰
        </button>
        <span style={{ fontWeight: 600, fontSize: 16 }}>Restward</span>
      </div>
      <EnvironmentSelector />
    </div>
  );
}
