using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HPMS.Modules.Identity.DTO;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HPMS.Tests.Integration;

public class PatientIsolationTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetPatients_ShouldOnlyReturnDataForCurrentTenant()
    {
        // Arrange
        var password = $"Test@12345!{Guid.NewGuid():N}";

        var tenantAName = $"Integration Tenant A {Guid.NewGuid():N}";
        var tenantBName = $"Integration Tenant B {Guid.NewGuid():N}";

        var tenantA = await CreateTenantAsync(tenantAName);
        var tenantB = await CreateTenantAsync(tenantBName);

        var usernameA = $"tenantA_{Guid.NewGuid():N}";
        var usernameB = $"tenantB_{Guid.NewGuid():N}";

        await RegisterUserAsync(tenantA.Id, usernameA, password);
        await RegisterUserAsync(tenantB.Id, usernameB, password);

        var tokenA = await LoginAndGetTokenAsync(usernameA, password);
        var tokenB = await LoginAndGetTokenAsync(usernameB, password);

        await CreatePatientAsync(tokenA, "Alice", "TenantA", new DateOnly(1990, 1, 1));
        await CreatePatientAsync(tokenB, "Bob", "TenantB", new DateOnly(1991, 2, 2));

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

        // Act
        var response = await Client.GetAsync("/scheduling/patients");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var patients = await response.Content.ReadFromJsonAsync<List<Patient>>();
        patients.Should().NotBeNull();
        patients.Should().ContainSingle();
        patients[0].FirstName.Should().Be("Alice");
        patients.Should().OnlyContain(p => p.TenantId == tenantA.Id);

        Client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<TenantResponse> CreateTenantAsync(string name)
    {
        var response = await Client.PostAsJsonAsync($"/identity/tenants?name={Uri.EscapeDataString(name)}", new { });
        response.EnsureSuccessStatusCode();

        var tenant = await response.Content.ReadFromJsonAsync<TenantResponse>();
        tenant.Should().NotBeNull();
        return tenant;
    }

    private async Task RegisterUserAsync(Guid tenantId, string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/identity/users", new UserRegistrationDto(
            tenantId,
            username,
            $"{username}@example.com",
            password,
            RoleId: 2,
            FirstName: "Test",
            LastName: "User"));

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginAndGetTokenAsync(string username, string password)
    {
        var response = await Client.PostAsJsonAsync("/identity/login", new LoginRequest(username, password));
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse.Token.Should().NotBeNullOrWhiteSpace();

        return loginResponse.Token;
    }

    private async Task CreatePatientAsync(string token, string firstName, string lastName, DateOnly dateOfBirth)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await Client.PostAsJsonAsync("/scheduling/patients", new PatientDto(firstName, lastName, dateOfBirth));
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    private sealed record TenantResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt);

    private sealed record LoginResponse(string Token);
}