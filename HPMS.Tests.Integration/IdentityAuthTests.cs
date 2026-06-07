using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using FluentAssertions;
using HPMS.Modules.Identity.DTO;
using HPMS.SharedKernel.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HPMS.Tests.Integration;

[Collection("Integration")]
public class IdentityAuthTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Login_ShouldEmitRoleClaim_FromDatabaseRole()
    {
        var password = $"Test@12345!{Guid.NewGuid():N}";
        var systemAdminToken = await IntegrationAuthHelper.LoginAsSystemAdminAsync(Client);

        var tenant = await IntegrationAuthHelper.CreateTenantAsync(
            Client,
            $"Role Claim Tenant {Guid.NewGuid():N}",
            systemAdminToken);

        var username = $"frontdesk_{Guid.NewGuid():N}";
        await IntegrationAuthHelper.RegisterUserAsync(
            Client,
            tenant.Id,
            username,
            password,
            systemAdminToken,
            roleId: 5);

        Client.DefaultRequestHeaders.Authorization = null;

        var token = await IntegrationAuthHelper.LoginAsync(Client, username, password);
        var role = ReadRoleClaim(token);

        role.Should().Be(HpmsRoles.FrontDesk);
    }

    [Fact]
    public async Task CreateTenant_ShouldReturnForbidden_WhenCallerIsClinicAdmin()
    {
        var password = $"Test@12345!{Guid.NewGuid():N}";
        var systemAdminToken = await IntegrationAuthHelper.LoginAsSystemAdminAsync(Client);

        var tenant = await IntegrationAuthHelper.CreateTenantAsync(
            Client,
            $"Forbidden Tenant {Guid.NewGuid():N}",
            systemAdminToken);

        var clinicAdminUsername = $"clinicadmin_{Guid.NewGuid():N}";
        await IntegrationAuthHelper.RegisterUserAsync(
            Client,
            tenant.Id,
            clinicAdminUsername,
            password,
            systemAdminToken,
            roleId: 2);

        Client.DefaultRequestHeaders.Authorization = null;

        var clinicAdminToken = await IntegrationAuthHelper.LoginAsync(Client, clinicAdminUsername, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clinicAdminToken);

        var response = await Client.PostAsJsonAsync(
            $"/identity/tenants?name={Uri.EscapeDataString("Should Not Be Created")}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Client.DefaultRequestHeaders.Authorization = null;
    }

    [Fact]
    public async Task RegisterUser_ShouldReturnForbidden_WhenClinicAdminAssignsSystemAdminRole()
    {
        var password = $"Test@12345!{Guid.NewGuid():N}";
        var systemAdminToken = await IntegrationAuthHelper.LoginAsSystemAdminAsync(Client);

        var tenant = await IntegrationAuthHelper.CreateTenantAsync(
            Client,
            $"Escalation Tenant {Guid.NewGuid():N}",
            systemAdminToken);

        var clinicAdminUsername = $"clinicadmin_{Guid.NewGuid():N}";
        await IntegrationAuthHelper.RegisterUserAsync(
            Client,
            tenant.Id,
            clinicAdminUsername,
            password,
            systemAdminToken,
            roleId: 2);

        var clinicAdminToken = await IntegrationAuthHelper.LoginAsync(Client, clinicAdminUsername, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clinicAdminToken);

        var response = await Client.PostAsJsonAsync("/identity/users", new UserRegistrationDto(
            tenant.Id,
            $"escalation_{Guid.NewGuid():N}",
            "escalation@example.com",
            password,
            RoleId: 1,
            FirstName: "Escalation",
            LastName: "Attempt"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        Client.DefaultRequestHeaders.Authorization = null;
    }

    private static string? ReadRoleClaim(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        return jwt.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role ||
            c.Type == "role")?.Value;
    }
}
