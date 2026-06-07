using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HPMS.Modules.Identity.DTO;

namespace HPMS.Tests.Integration;

internal static class IntegrationAuthHelper
{
    internal const string SystemAdminUsername = "sysadmin";
    internal const string SystemAdminPassword = "admin123";

    internal static async Task<string> LoginAsync(HttpClient client, string username, string password, bool rememberMe = true)
    {
        client.DefaultRequestHeaders.Authorization = null;

        var response = await client.PostAsJsonAsync("/identity/login", new LoginRequest(username, password, rememberMe));
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse!.Token.Should().NotBeNullOrWhiteSpace();

        return loginResponse.Token;
    }

    internal static async Task<string> LoginAsSystemAdminAsync(HttpClient client)
        => await LoginAsync(client, SystemAdminUsername, SystemAdminPassword);

    internal static async Task<TenantResponse> CreateTenantAsync(HttpClient client, string name, string systemAdminToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", systemAdminToken);

        var response = await client.PostAsJsonAsync($"/identity/tenants?name={Uri.EscapeDataString(name)}", new { });
        response.EnsureSuccessStatusCode();

        var tenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        tenant.Should().NotBeNull();
        return tenant!;
    }

    internal static async Task RegisterUserAsync(
        HttpClient client,
        Guid tenantId,
        string username,
        string password,
        string authToken,
        int roleId = 2)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken);

        var response = await client.PostAsJsonAsync("/identity/users", new UserRegistrationDto(
            tenantId,
            username,
            $"{username}@example.com",
            password,
            roleId,
            "Test",
            "User"));

        response.EnsureSuccessStatusCode();
    }

    internal sealed record TenantResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt);
    internal sealed record LoginResponse(string Token);
}
