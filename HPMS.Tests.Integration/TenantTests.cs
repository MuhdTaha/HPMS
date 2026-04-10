using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HPMS.Tests.Integration;

public class TenantTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CreateTenant_ShouldReturnCreated_WhenDataIsValid()
    {
        // Arrange: Prepare the data
        var tenantName = "Integration Test Clinic";

        // Act: Call the endpoint
        var response = await Client.PostAsJsonAsync($"/identity/tenants?name={Uri.EscapeDataString(tenantName)}", new { });

        // Assert: Verify the result
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        
        var content = await response.Content.ReadFromJsonAsync<TenantResponse>();
        content.Should().NotBeNull();
        content.Name.Should().Be(tenantName);
    }

    private sealed record TenantResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt);
}