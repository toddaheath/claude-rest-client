import { useCallback } from 'react';
import { api } from '../api/client';
import { useStore } from '../store';
import type { Team, TeamInvitation } from '../types';

export function useTeams() {
  const { setTeams, setPendingInvitations } = useStore();

  const fetchTeams = useCallback(async () => {
    const teams = await api.get<Team[]>('/teams');
    setTeams(teams);
    return teams;
  }, [setTeams]);

  const fetchInvitations = useCallback(async () => {
    const invitations = await api.get<TeamInvitation[]>('/teams/invitations');
    setPendingInvitations(invitations);
    return invitations;
  }, [setPendingInvitations]);

  const createTeam = useCallback(async (name: string) => {
    const team = await api.post<Team>('/teams', { name });
    await fetchTeams();
    return team;
  }, [fetchTeams]);

  const deleteTeam = useCallback(async (id: string) => {
    await api.delete(`/teams/${id}`);
    await fetchTeams();
  }, [fetchTeams]);

  const getTeam = useCallback(async (id: string) => {
    return api.get<Team>(`/teams/${id}`);
  }, []);

  const inviteMember = useCallback(async (teamId: string, email: string) => {
    await api.post(`/teams/${teamId}/invite`, { email });
  }, []);

  const acceptInvitation = useCallback(async (invitationId: string) => {
    await api.post(`/teams/invitations/${invitationId}/accept`);
    await fetchTeams();
    await fetchInvitations();
  }, [fetchTeams, fetchInvitations]);

  const declineInvitation = useCallback(async (invitationId: string) => {
    await api.post(`/teams/invitations/${invitationId}/decline`);
    await fetchInvitations();
  }, [fetchInvitations]);

  const removeMember = useCallback(async (teamId: string, memberId: string) => {
    await api.delete(`/teams/${teamId}/members/${memberId}`);
  }, []);

  return {
    fetchTeams,
    fetchInvitations,
    createTeam,
    deleteTeam,
    getTeam,
    inviteMember,
    acceptInvitation,
    declineInvitation,
    removeMember,
  };
}
