using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HPMS.Tests.Integration;

[Collection("Integration")]
public class TenantTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateTenant_ShouldReturnUnauthorized_WhenNotAuthenticated()
    {
        var response = await Client.PostAsJsonAsync(
            $"/identity/tenants?name={Uri.EscapeDataString("Unauthorized Clinic")}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTenant_ShouldReturnCreated_WhenSystemAdminIsAuthenticated()
    {
        var tenantName = $"Integration Test Clinic {Guid.NewGuid():N}";
        var systemAdminToken = await IntegrationAuthHelper.LoginAsSystemAdminAsync(Client);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", systemAdminToken);

        var response = await Client.PostAsJsonAsync(
            $"/identity/tenants?name={Uri.EscapeDataString(tenantName)}",
            new { });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadFromJsonAsync<IntegrationAuthHelper.TenantResponse>();
        content.Should().NotBeNull();
        content!.Name.Should().Be(tenantName);

        Client.DefaultRequestHeaders.Authorization = null;
    }
}
