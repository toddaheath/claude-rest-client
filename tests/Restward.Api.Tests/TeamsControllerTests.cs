using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Restward.Api.Models.Dtos;

namespace Restward.Api.Tests;

public class TeamsControllerTests : IClassFixture<RestwardWebApplicationFactory>
{
    private readonly RestwardWebApplicationFactory _factory;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public TeamsControllerTests(RestwardWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Registers a new user and returns an authenticated HttpClient with their API key.
    /// </summary>
    private async Task<(HttpClient Client, string Email, string ApiKey)> RegisterUserAsync(string name, string email)
    {
        using var regClient = _factory.CreateClient();
        var regResponse = await regClient.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Name = name,
            Email = email,
            Password = "password123"
        });
        regResponse.EnsureSuccessStatusCode();
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponseDto>();

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Api-Key", auth!.ApiKey);
        return (client, email, auth.ApiKey);
    }

    [Fact]
    public async Task CreateTeam_ReturnsCreated()
    {
        using var client = _factory.CreateAuthenticatedClient();

        var response = await client.PostAsJsonAsync("/api/teams", new { name = "Test Team" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var team = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Test Team", team.GetProperty("name").GetString());
        Assert.NotEqual(Guid.Empty.ToString(), team.GetProperty("id").GetString());
    }

    [Fact]
    public async Task ListTeams_ReturnsUserTeams()
    {
        using var client = _factory.CreateAuthenticatedClient();

        // Create a team first
        var createResponse = await client.PostAsJsonAsync("/api/teams", new { name = "Listed Team" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        // List teams
        var response = await client.GetAsync("/api/teams");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var teams = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions);
        Assert.NotNull(teams);
        Assert.Contains(teams, t => t.GetProperty("name").GetString() == "Listed Team");
    }

    [Fact]
    public async Task GetTeamById_IncludesMembers()
    {
        using var client = _factory.CreateAuthenticatedClient();

        // Create a team
        var createResponse = await client.PostAsJsonAsync("/api/teams", new { name = "Detail Team" });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var teamId = created.GetProperty("id").GetString();

        // Get the team by ID
        var response = await client.GetAsync($"/api/teams/{teamId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        var team = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
        Assert.Equal("Detail Team", team.GetProperty("name").GetString());
        var members = team.GetProperty("members");
        Assert.True(members.GetArrayLength() > 0);
        // Creator should be the Owner
        var firstMember = members[0];
        Assert.Equal("Owner", firstMember.GetProperty("role").GetString());
    }

    [Fact]
    public async Task DeleteTeam_OwnerOnly_ReturnsNoContent()
    {
        using var client = _factory.CreateAuthenticatedClient();

        // Create a team
        var createResponse = await client.PostAsJsonAsync("/api/teams", new { name = "Delete Me Team" });
        var created = JsonSerializer.Deserialize<JsonElement>(
            await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
        var teamId = created.GetProperty("id").GetString();

        // Delete the team
        var response = await client.DeleteAsync($"/api/teams/{teamId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify it's gone
        var getResponse = await client.GetAsync($"/api/teams/{teamId}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task InviteByEmail_ReturnsCreated()
    {
        var email1 = $"owner-invite-{Guid.NewGuid():N}@example.com";
        var (ownerClient, _, _) = await RegisterUserAsync("Invite Owner", email1);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Invite Team" });
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Invite someone
            var inviteResponse = await ownerClient.PostAsJsonAsync(
                $"/api/teams/{teamId}/invite",
                new { email = "invitee@example.com" });

            Assert.Equal(HttpStatusCode.Created, inviteResponse.StatusCode);
            var body = await inviteResponse.Content.ReadAsStringAsync();
            var invitation = JsonSerializer.Deserialize<JsonElement>(body, _jsonOptions);
            Assert.Equal("invitee@example.com", invitation.GetProperty("email").GetString());
            Assert.Equal("Pending", invitation.GetProperty("status").GetString());
        }
        finally
        {
            ownerClient.Dispose();
        }
    }

    [Fact]
    public async Task ListInvitations_ReturnsInvitationsForCurrentUserEmail()
    {
        var ownerEmail = $"inv-list-owner-{Guid.NewGuid():N}@example.com";
        var inviteeEmail = $"inv-list-target-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("Inv List Owner", ownerEmail);
        var (inviteeClient, _, _) = await RegisterUserAsync("Inv List Invitee", inviteeEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Inv List Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Invite the invitee
            await ownerClient.PostAsJsonAsync($"/api/teams/{teamId}/invite",
                new { email = inviteeEmail });

            // Invitee lists their invitations
            var response = await inviteeClient.GetAsync("/api/teams/invitations");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            var invitations = JsonSerializer.Deserialize<JsonElement[]>(body, _jsonOptions);
            Assert.NotNull(invitations);
            Assert.Contains(invitations, i =>
                i.GetProperty("team_name").GetString() == "Inv List Team"
                && i.GetProperty("email").GetString() == inviteeEmail);
        }
        finally
        {
            ownerClient.Dispose();
            inviteeClient.Dispose();
        }
    }

    [Fact]
    public async Task AcceptInvitation_CreatesMember()
    {
        var ownerEmail = $"accept-owner-{Guid.NewGuid():N}@example.com";
        var inviteeEmail = $"accept-target-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("Accept Owner", ownerEmail);
        var (inviteeClient, _, _) = await RegisterUserAsync("Accept Invitee", inviteeEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Accept Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Invite the invitee
            await ownerClient.PostAsJsonAsync($"/api/teams/{teamId}/invite",
                new { email = inviteeEmail });

            // Invitee lists invitations to find the invitation ID
            var listResponse = await inviteeClient.GetAsync("/api/teams/invitations");
            var invitations = JsonSerializer.Deserialize<JsonElement[]>(
                await listResponse.Content.ReadAsStringAsync(), _jsonOptions)!;
            var invitationId = invitations.First(i =>
                i.GetProperty("team_name").GetString() == "Accept Team")
                .GetProperty("id").GetString();

            // Accept the invitation
            var acceptResponse = await inviteeClient.PostAsync(
                $"/api/teams/invitations/{invitationId}/accept", null);

            Assert.Equal(HttpStatusCode.OK, acceptResponse.StatusCode);

            // Verify invitee is now a member by getting the team
            var teamResponse = await inviteeClient.GetAsync($"/api/teams/{teamId}");
            Assert.Equal(HttpStatusCode.OK, teamResponse.StatusCode);
            var team = JsonSerializer.Deserialize<JsonElement>(
                await teamResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var members = team.GetProperty("members");
            Assert.Equal(2, members.GetArrayLength());
        }
        finally
        {
            ownerClient.Dispose();
            inviteeClient.Dispose();
        }
    }

    [Fact]
    public async Task DeclineInvitation_UpdatesStatus()
    {
        var ownerEmail = $"decline-owner-{Guid.NewGuid():N}@example.com";
        var inviteeEmail = $"decline-target-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("Decline Owner", ownerEmail);
        var (inviteeClient, _, _) = await RegisterUserAsync("Decline Invitee", inviteeEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Decline Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Invite the invitee
            await ownerClient.PostAsJsonAsync($"/api/teams/{teamId}/invite",
                new { email = inviteeEmail });

            // Invitee lists invitations
            var listResponse = await inviteeClient.GetAsync("/api/teams/invitations");
            var invitations = JsonSerializer.Deserialize<JsonElement[]>(
                await listResponse.Content.ReadAsStringAsync(), _jsonOptions)!;
            var invitationId = invitations.First(i =>
                i.GetProperty("team_name").GetString() == "Decline Team")
                .GetProperty("id").GetString();

            // Decline the invitation
            var declineResponse = await inviteeClient.PostAsync(
                $"/api/teams/invitations/{invitationId}/decline", null);

            Assert.Equal(HttpStatusCode.OK, declineResponse.StatusCode);

            // Verify invitation no longer appears in pending list
            var listAfterDecline = await inviteeClient.GetAsync("/api/teams/invitations");
            var remainingInvitations = JsonSerializer.Deserialize<JsonElement[]>(
                await listAfterDecline.Content.ReadAsStringAsync(), _jsonOptions)!;
            Assert.DoesNotContain(remainingInvitations, i =>
                i.GetProperty("team_name").GetString() == "Decline Team");
        }
        finally
        {
            ownerClient.Dispose();
            inviteeClient.Dispose();
        }
    }

    [Fact]
    public async Task RemoveMember_OwnerCanRemoveMember()
    {
        var ownerEmail = $"remove-owner-{Guid.NewGuid():N}@example.com";
        var memberEmail = $"remove-member-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("Remove Owner", ownerEmail);
        var (memberClient, _, _) = await RegisterUserAsync("Remove Member", memberEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Remove Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Invite and accept
            await ownerClient.PostAsJsonAsync($"/api/teams/{teamId}/invite",
                new { email = memberEmail });

            var listResponse = await memberClient.GetAsync("/api/teams/invitations");
            var invitations = JsonSerializer.Deserialize<JsonElement[]>(
                await listResponse.Content.ReadAsStringAsync(), _jsonOptions)!;
            var invitationId = invitations.First(i =>
                i.GetProperty("team_name").GetString() == "Remove Team")
                .GetProperty("id").GetString();

            await memberClient.PostAsync($"/api/teams/invitations/{invitationId}/accept", null);

            // Get team to find the member's membership ID
            var teamResponse = await ownerClient.GetAsync($"/api/teams/{teamId}");
            var team = JsonSerializer.Deserialize<JsonElement>(
                await teamResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var members = team.GetProperty("members");
            var memberToRemove = members.EnumerateArray()
                .First(m => m.GetProperty("role").GetString() == "Member");
            var memberId = memberToRemove.GetProperty("id").GetString();

            // Remove the member
            var removeResponse = await ownerClient.DeleteAsync(
                $"/api/teams/{teamId}/members/{memberId}");

            Assert.Equal(HttpStatusCode.NoContent, removeResponse.StatusCode);

            // Verify member is removed
            var teamAfter = await ownerClient.GetAsync($"/api/teams/{teamId}");
            var teamData = JsonSerializer.Deserialize<JsonElement>(
                await teamAfter.Content.ReadAsStringAsync(), _jsonOptions);
            Assert.Equal(1, teamData.GetProperty("members").GetArrayLength());
        }
        finally
        {
            ownerClient.Dispose();
            memberClient.Dispose();
        }
    }

    [Fact]
    public async Task NonMember_CannotAccessTeam_ReturnsNotFound()
    {
        var ownerEmail = $"access-owner-{Guid.NewGuid():N}@example.com";
        var nonMemberEmail = $"access-nonmember-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("Access Owner", ownerEmail);
        var (nonMemberClient, _, _) = await RegisterUserAsync("Non Member", nonMemberEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "Private Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Non-member tries to access the team
            var response = await nonMemberClient.GetAsync($"/api/teams/{teamId}");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        finally
        {
            ownerClient.Dispose();
            nonMemberClient.Dispose();
        }
    }

    [Fact]
    public async Task CannotRemoveOwner()
    {
        var ownerEmail = $"noremove-owner-{Guid.NewGuid():N}@example.com";
        var adminEmail = $"noremove-admin-{Guid.NewGuid():N}@example.com";

        var (ownerClient, _, _) = await RegisterUserAsync("NoRemove Owner", ownerEmail);
        var (adminClient, _, _) = await RegisterUserAsync("NoRemove Admin", adminEmail);

        try
        {
            // Create a team
            var createResponse = await ownerClient.PostAsJsonAsync("/api/teams", new { name = "NoRemove Team" });
            var created = JsonSerializer.Deserialize<JsonElement>(
                await createResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var teamId = created.GetProperty("id").GetString();

            // Get team to find owner's membership ID
            var teamResponse = await ownerClient.GetAsync($"/api/teams/{teamId}");
            var team = JsonSerializer.Deserialize<JsonElement>(
                await teamResponse.Content.ReadAsStringAsync(), _jsonOptions);
            var ownerMemberId = team.GetProperty("members")[0].GetProperty("id").GetString();

            // Owner tries to remove themselves (the owner)
            var removeResponse = await ownerClient.DeleteAsync(
                $"/api/teams/{teamId}/members/{ownerMemberId}");

            // Should be forbidden since we can't remove the owner
            Assert.Equal(HttpStatusCode.Forbidden, removeResponse.StatusCode);
        }
        finally
        {
            ownerClient.Dispose();
            adminClient.Dispose();
        }
    }
}
