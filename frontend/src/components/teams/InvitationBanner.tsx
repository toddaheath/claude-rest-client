import { useStore } from '../../store';
import { useTeams } from '../../hooks/useTeams';

export function InvitationBanner() {
  const { pendingInvitations } = useStore();
  const { acceptInvitation, declineInvitation } = useTeams();

  if (pendingInvitations.length === 0) return null;

  return (
    <div style={{
      padding: '8px 16px', background: 'var(--color-accent)', color: '#fff',
      display: 'flex', alignItems: 'center', gap: 12, fontSize: 13,
    }}>
      <span>
        You have {pendingInvitations.length} pending team invitation{pendingInvitations.length > 1 ? 's' : ''}
        {pendingInvitations.length === 1 && `: ${pendingInvitations[0].team_name}`}
      </span>
      {pendingInvitations.length === 1 && (
        <div style={{ display: 'flex', gap: 8, marginLeft: 'auto' }}>
          <button
            onClick={() => acceptInvitation(pendingInvitations[0].id)}
            style={{
              padding: '4px 12px', background: 'rgba(255,255,255,0.2)', color: '#fff',
              border: '1px solid rgba(255,255,255,0.3)', borderRadius: 4, cursor: 'pointer', fontSize: 12,
            }}
          >Accept</button>
          <button
            onClick={() => declineInvitation(pendingInvitations[0].id)}
            style={{
              padding: '4px 12px', background: 'transparent', color: '#fff',
              border: '1px solid rgba(255,255,255,0.3)', borderRadius: 4, cursor: 'pointer', fontSize: 12,
            }}
          >Decline</button>
        </div>
      )}
    </div>
  );
}
