import { useEffect, useState } from 'react';
import { useStore } from '../../store';
import { isAuthenticated as checkAuth, clearAuth, isDemoMode } from '../../api/client';
import { useCollections } from '../../hooks/useCollections';
import { useEnvironments } from '../../hooks/useEnvironments';
import { useTeams } from '../../hooks/useTeams';
import { Sidebar } from './Sidebar';
import { TopBar } from './TopBar';
import { RequestPanel } from '../request/RequestPanel';
import { ResponsePanel } from '../response/ResponsePanel';
import { LoginPage } from '../auth/LoginPage';
import { InvitationBanner } from '../teams/InvitationBanner';

export function AppLayout() {
  const { sidebarOpen } = useStore();
  const { fetchCollections } = useCollections();
  const { fetchEnvironments } = useEnvironments();
  const { fetchTeams, fetchInvitations } = useTeams();
  const [authenticated, setAuthenticated] = useState(isDemoMode || checkAuth());

  useEffect(() => {
    if (authenticated) {
      fetchCollections().catch(console.error);
      fetchEnvironments().catch(console.error);
      fetchTeams().catch(console.error);
      fetchInvitations().catch(console.error);
    }
  }, [authenticated, fetchCollections, fetchEnvironments, fetchTeams, fetchInvitations]);

  const handleLogout = () => {
    clearAuth();
    setAuthenticated(false);
  };

  if (!authenticated) {
    return <LoginPage onAuthenticated={() => setAuthenticated(true)} />;
  }

  return (
    <div style={{ display: 'flex', height: '100vh', background: 'var(--color-bg-primary)', color: 'var(--color-text-primary)' }}>
      {sidebarOpen && <Sidebar />}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
        <TopBar onLogout={handleLogout} />
        <InvitationBanner />
        <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
          <div style={{ flex: 1, overflow: 'auto', borderBottom: '1px solid var(--color-border-primary)' }}>
            <RequestPanel />
          </div>
          <div style={{ flex: 1, overflow: 'auto' }}>
            <ResponsePanel />
          </div>
        </div>
      </div>
    </div>
  );
}
