import { useState, useEffect } from 'react';
import { useStore } from '../../store';
import { useTeams } from '../../hooks/useTeams';
import type { Team } from '../../types';

interface Props {
  onClose: () => void;
}

export function TeamManager({ onClose }: Props) {
  const { teams, pendingInvitations } = useStore();
  const { createTeam, deleteTeam, getTeam, inviteMember, acceptInvitation, declineInvitation, removeMember } = useTeams();
  const [selectedTeamId, setSelectedTeamId] = useState<string | null>(null);
  const [teamDetail, setTeamDetail] = useState<Team | null>(null);
  const [newTeamName, setNewTeamName] = useState('');
  const [inviteEmail, setInviteEmail] = useState('');
  const [error, setError] = useState('');
  const [tab, setTab] = useState<'teams' | 'invitations'>('teams');

  useEffect(() => {
    if (selectedTeamId) {
      getTeam(selectedTeamId).then(setTeamDetail).catch(console.error);
    }
  }, [selectedTeamId, getTeam]);

  const selectTeam = (id: string | null) => {
    setSelectedTeamId(id);
    if (!id) setTeamDetail(null);
  };

  const handleCreateTeam = async () => {
    if (!newTeamName.trim()) return;
    setError('');
    try {
      const team = await createTeam(newTeamName.trim());
      setNewTeamName('');
      selectTeam(team.id);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to create team');
    }
  };

  const handleInvite = async () => {
    if (!selectedTeamId || !inviteEmail.trim()) return;
    setError('');
    try {
      await inviteMember(selectedTeamId, inviteEmail.trim());
      setInviteEmail('');
      const updated = await getTeam(selectedTeamId);
      setTeamDetail(updated);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to send invitation');
    }
  };

  const handleRemoveMember = async (memberId: string) => {
    if (!selectedTeamId) return;
    try {
      await removeMember(selectedTeamId, memberId);
      const updated = await getTeam(selectedTeamId);
      setTeamDetail(updated);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to remove member');
    }
  };

  return (
    <div
      style={{
        position: 'fixed', top: 0, left: 0, right: 0, bottom: 0,
        background: 'var(--color-overlay)', display: 'flex', justifyContent: 'center', alignItems: 'center',
        zIndex: 100,
      }}
      onClick={onClose}
    >
      <div
        onClick={(e) => e.stopPropagation()}
        style={{
          background: 'var(--color-bg-secondary)', borderRadius: 8, width: 700, maxHeight: '80vh',
          display: 'flex', flexDirection: 'column', overflow: 'hidden', border: '1px solid var(--color-border-secondary)',
        }}
      >
        {/* Header with tabs */}
        <div style={{ display: 'flex', borderBottom: '1px solid var(--color-border-primary)', alignItems: 'center' }}>
          <button
            onClick={() => setTab('teams')}
            style={{
              padding: '12px 16px', background: 'none', border: 'none',
              borderBottom: tab === 'teams' ? '2px solid var(--color-accent)' : '2px solid transparent',
              color: tab === 'teams' ? 'var(--color-text-bright)' : 'var(--color-text-secondary)',
              cursor: 'pointer', fontSize: 14, fontWeight: 600,
            }}
          >
            Teams
          </button>
          <button
            onClick={() => setTab('invitations')}
            style={{
              padding: '12px 16px', background: 'none', border: 'none',
              borderBottom: tab === 'invitations' ? '2px solid var(--color-accent)' : '2px solid transparent',
              color: tab === 'invitations' ? 'var(--color-text-bright)' : 'var(--color-text-secondary)',
              cursor: 'pointer', fontSize: 14, fontWeight: 600,
            }}
          >
            Invitations {pendingInvitations.length > 0 && `(${pendingInvitations.length})`}
          </button>
          <div style={{ flex: 1 }} />
          <button
            onClick={onClose}
            style={{
              background: 'none', border: 'none', color: 'var(--color-text-muted)',
              cursor: 'pointer', fontSize: 18, padding: '8px 12px',
            }}
          >×</button>
        </div>

        {error && (
          <div style={{ padding: '8px 16px', color: 'var(--color-error)', fontSize: 13 }}>{error}</div>
        )}

        {tab === 'teams' && (
          <div style={{ display: 'flex', flex: 1, overflow: 'hidden' }}>
            {/* Team list */}
            <div style={{ width: 220, borderRight: '1px solid var(--color-border-primary)', display: 'flex', flexDirection: 'column' }}>
              <div style={{ flex: 1, overflow: 'auto' }}>
                {teams.map((team) => (
                  <div
                    key={team.id}
                    onClick={() => selectTeam(team.id)}
                    style={{
                      padding: '8px 12px', cursor: 'pointer', fontSize: 13,
                      background: team.id === selectedTeamId ? 'var(--color-bg-active)' : 'transparent',
                      display: 'flex', justifyContent: 'space-between', alignItems: 'center',
                    }}
                  >
                    <span>{team.name}</span>
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        deleteTeam(team.id);
                        if (selectedTeamId === team.id) selectTeam(null);
                      }}
                      style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 14 }}
                    >×</button>
                  </div>
                ))}
                {teams.length === 0 && (
                  <div style={{ padding: 16, color: 'var(--color-text-muted)', fontSize: 13, textAlign: 'center' }}>
                    No teams yet
                  </div>
                )}
              </div>
              <div style={{ padding: 8, borderTop: '1px solid var(--color-border-primary)', display: 'flex', gap: 4 }}>
                <input
                  value={newTeamName}
                  onChange={(e) => setNewTeamName(e.target.value)}
                  onKeyDown={(e) => e.key === 'Enter' && handleCreateTeam()}
                  placeholder="New team"
                  style={{
                    flex: 1, padding: '4px 8px', background: 'var(--color-bg-elevated)', border: '1px solid var(--color-border-tertiary)',
                    borderRadius: 4, color: 'var(--color-text-bright)', fontSize: 12, outline: 'none',
                  }}
                />
                <button
                  onClick={handleCreateTeam}
                  style={{
                    padding: '4px 8px', background: 'var(--color-accent)', color: '#fff',
                    border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12,
                  }}
                >+</button>
              </div>
            </div>

            {/* Team detail */}
            <div style={{ flex: 1, display: 'flex', flexDirection: 'column', overflow: 'auto' }}>
              {teamDetail ? (
                <div style={{ padding: 16 }}>
                  <h3 style={{ margin: '0 0 16px', fontSize: 16 }}>{teamDetail.name}</h3>

                  <div style={{ marginBottom: 16 }}>
                    <label style={{ fontSize: 13, color: 'var(--color-text-secondary)', display: 'block', marginBottom: 8 }}>Members</label>
                    {teamDetail.members?.map((member) => (
                      <div key={member.id} style={{
                        display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                        padding: '6px 0', borderBottom: '1px solid var(--color-bg-input)', fontSize: 13,
                      }}>
                        <div>
                          <span style={{ color: 'var(--color-text-primary)' }}>{member.user_name}</span>
                          {member.user_email && (
                            <span style={{ color: 'var(--color-text-muted)', marginLeft: 8 }}>{member.user_email}</span>
                          )}
                          <span style={{
                            marginLeft: 8, fontSize: 10, padding: '2px 6px', borderRadius: 3,
                            background: member.role === 'Owner' ? 'var(--color-accent)' : 'var(--color-bg-elevated)',
                            color: member.role === 'Owner' ? '#fff' : 'var(--color-text-secondary)',
                          }}>
                            {member.role}
                          </span>
                        </div>
                        {member.role !== 'Owner' && (
                          <button
                            onClick={() => handleRemoveMember(member.id)}
                            style={{ background: 'none', border: 'none', color: 'var(--color-text-muted)', cursor: 'pointer', fontSize: 14 }}
                          >×</button>
                        )}
                      </div>
                    ))}
                  </div>

                  <div>
                    <label style={{ fontSize: 13, color: 'var(--color-text-secondary)', display: 'block', marginBottom: 4 }}>Invite Member</label>
                    <div style={{ display: 'flex', gap: 8 }}>
                      <input
                        type="email"
                        value={inviteEmail}
                        onChange={(e) => setInviteEmail(e.target.value)}
                        onKeyDown={(e) => e.key === 'Enter' && handleInvite()}
                        placeholder="Email address"
                        style={{
                          flex: 1, padding: '6px 8px', background: 'var(--color-bg-input)', color: 'var(--color-text-bright)',
                          border: '1px solid var(--color-border-secondary)', borderRadius: 4, fontSize: 13, outline: 'none',
                        }}
                      />
                      <button
                        onClick={handleInvite}
                        style={{
                          padding: '6px 16px', background: 'var(--color-accent)', color: '#fff',
                          border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 13,
                        }}
                      >Invite</button>
                    </div>
                  </div>
                </div>
              ) : (
                <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', flex: 1, color: 'var(--color-text-muted)' }}>
                  Select or create a team
                </div>
              )}
            </div>
          </div>
        )}

        {tab === 'invitations' && (
          <div style={{ flex: 1, overflow: 'auto', padding: 16 }}>
            {pendingInvitations.length === 0 ? (
              <div style={{ textAlign: 'center', color: 'var(--color-text-muted)', padding: 32, fontSize: 13 }}>
                No pending invitations
              </div>
            ) : (
              pendingInvitations.map((inv) => (
                <div key={inv.id} style={{
                  display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                  padding: '12px', marginBottom: 8, background: 'var(--color-bg-input)',
                  borderRadius: 6, border: '1px solid var(--color-border-secondary)',
                }}>
                  <div>
                    <div style={{ fontSize: 14, color: 'var(--color-text-primary)', marginBottom: 4 }}>
                      {inv.team_name}
                    </div>
                    <div style={{ fontSize: 12, color: 'var(--color-text-muted)' }}>
                      Invited by {inv.invited_by_name}
                    </div>
                  </div>
                  <div style={{ display: 'flex', gap: 8 }}>
                    <button
                      onClick={() => acceptInvitation(inv.id)}
                      style={{
                        padding: '6px 12px', background: 'var(--color-accent)', color: '#fff',
                        border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12,
                      }}
                    >Accept</button>
                    <button
                      onClick={() => declineInvitation(inv.id)}
                      style={{
                        padding: '6px 12px', background: 'var(--color-border-primary)', color: 'var(--color-text-bright)',
                        border: 'none', borderRadius: 4, cursor: 'pointer', fontSize: 12,
                      }}
                    >Decline</button>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
}
