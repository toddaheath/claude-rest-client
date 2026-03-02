import { useState } from 'react';
import { useStore } from '../../store';
import { useTheme } from '../../hooks/useTheme';
import { EnvironmentSelector } from '../environments/EnvironmentSelector';
import { TeamManager } from '../teams/TeamManager';

interface Props {
  onLogout: () => void;
}

export function TopBar({ onLogout }: Props) {
  const { toggleSidebar, pendingInvitations } = useStore();
  const { theme, toggleTheme } = useTheme();
  const [showTeams, setShowTeams] = useState(false);

  return (
    <div style={{
      display: 'flex', alignItems: 'center', justifyContent: 'space-between',
      padding: '8px 16px', background: 'var(--color-bg-secondary)', borderBottom: '1px solid var(--color-border-primary)',
    }}>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <button
          onClick={toggleSidebar}
          style={{
            background: 'none', border: 'none', color: 'var(--color-text-primary)', cursor: 'pointer',
            fontSize: 18, padding: '4px 8px',
          }}
        >
          ☰
        </button>
        <span style={{ fontWeight: 600, fontSize: 16 }}>Restward</span>
      </div>
      <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
        <EnvironmentSelector />
        <button
          onClick={() => setShowTeams(true)}
          title="Teams"
          style={{
            background: 'none', border: '1px solid var(--color-border-secondary)', color: 'var(--color-text-primary)',
            borderRadius: 4, padding: '4px 8px', cursor: 'pointer', fontSize: 12,
            position: 'relative',
          }}
        >
          Teams
          {pendingInvitations.length > 0 && (
            <span style={{
              position: 'absolute', top: -6, right: -6,
              background: 'var(--color-error)', color: '#fff', fontSize: 10,
              borderRadius: '50%', width: 16, height: 16,
              display: 'flex', alignItems: 'center', justifyContent: 'center',
            }}>
              {pendingInvitations.length}
            </span>
          )}
        </button>
        <button
          onClick={toggleTheme}
          title={`Switch to ${theme === 'dark' ? 'light' : 'dark'} mode`}
          style={{
            background: 'none', border: '1px solid var(--color-border-secondary)', color: 'var(--color-text-primary)',
            borderRadius: 4, padding: '4px 8px', cursor: 'pointer', fontSize: 14,
          }}
        >
          {theme === 'dark' ? '☀️' : '🌙'}
        </button>
        <button
          onClick={onLogout}
          title="Logout"
          style={{
            background: 'none', border: '1px solid var(--color-border-secondary)', color: 'var(--color-text-primary)',
            borderRadius: 4, padding: '4px 8px', cursor: 'pointer', fontSize: 12,
          }}
        >
          Logout
        </button>
      </div>
      {showTeams && <TeamManager onClose={() => setShowTeams(false)} />}
    </div>
  );
}
