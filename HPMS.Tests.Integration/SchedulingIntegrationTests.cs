using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using HPMS.Scheduling.Data;
using HPMS.SharedKernel.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;

namespace HPMS.Tests.Integration;

[Collection("Integration")]
public class SchedulingIntegrationTests(WebApplicationFactory<Program> factory) : BaseIntegrationTest(factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Fact]
    public async Task CreateAppointment_ShouldReturnConflict_WhenSlotOverlaps()
    {
        var context = await SeedSchedulingContextAsync();
        var start = DateTime.UtcNow.Date.AddHours(10);
        var end = start.AddHours(1);

        await BookAppointmentAsync(context.ClinicAdminToken, context.PatientId, context.ProviderId, start, end);

        var response = await BookAppointmentRawAsync(
            context.ClinicAdminToken,
            context.PatientId,
            context.ProviderId,
            start.AddMinutes(30),
            end.AddMinutes(30));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateAppointment_ShouldSucceed_WhenClinicAdminUsesForceBooking()
    {
        var context = await SeedSchedulingContextAsync();
        var start = DateTime.UtcNow.Date.AddHours(11);
        var end = start.AddHours(1);

        await BookAppointmentAsync(context.ClinicAdminToken, context.PatientId, context.ProviderId, start, end);

        var response = await BookAppointmentRawAsync(
            context.ClinicAdminToken,
            context.PatientId,
            context.ProviderId,
            start.AddMinutes(15),
            end.AddMinutes(15),
            forceBooking: true);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAppointment_ShouldReturnBadRequest_WhenProviderIsInvalid()
    {
        var context = await SeedSchedulingContextAsync();
        var start = DateTime.UtcNow.Date.AddHours(14);
        var end = start.AddHours(1);

        var response = await BookAppointmentRawAsync(
            context.ClinicAdminToken,
            context.PatientId,
            Guid.NewGuid(),
            start,
            end);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ListAppointments_ShouldReturnFilteredResults()
    {
        var context = await SeedSchedulingContextAsync();
        var start = DateTime.UtcNow.Date.AddHours(9);
        var end = start.AddHours(1);

        await BookAppointmentAsync(context.ClinicAdminToken, context.PatientId, context.ProviderId, start, end);

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.ClinicAdminToken);

        var response = await Client.GetAsync(
            $"/scheduling/appointments?providerId={context.ProviderId}&from={Uri.EscapeDataString(start.AddHours(-1).ToString("O"))}&to={Uri.EscapeDataString(end.AddHours(1).ToString("O"))}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var appointments = await response.Content.ReadFromJsonAsync<List<AppointmentListItem>>(JsonOptions);
        appointments.Should().NotBeNull();
        appointments!.Should().ContainSingle();
        appointments[0].ProviderId.Should().Be(context.ProviderId);

        Client.DefaultRequestHeaders.Authorization = null;
    }

    private async Task<SchedulingTestContext> SeedSchedulingContextAsync()
    {
        ClearClientAuth();

        var password = $"Test@12345!{Guid.NewGuid():N}";
        var patientFirstName = $"Alex_{Guid.NewGuid():N}";
        var systemAdminToken = await IntegrationAuthHelper.LoginAsSystemAdminAsync(Client);

        var tenant = await IntegrationAuthHelper.CreateTenantAsync(
            Client,
            $"Scheduling Tenant {Guid.NewGuid():N}",
            systemAdminToken);

        var clinicAdminUsername = $"clinicadmin_{Guid.NewGuid():N}";
        await IntegrationAuthHelper.RegisterUserAsync(
            Client,
            tenant.Id,
            clinicAdminUsername,
            password,
            systemAdminToken,
            roleId: HpmsRoleIds.ClinicAdmin);

        var providerUsername = $"provider_{Guid.NewGuid():N}";
        await IntegrationAuthHelper.RegisterUserAsync(
            Client,
            tenant.Id,
            providerUsername,
            password,
            systemAdminToken,
            roleId: HpmsRoleIds.Provider);

        Client.DefaultRequestHeaders.Authorization = null;

        var clinicAdminToken = await IntegrationAuthHelper.LoginAsync(Client, clinicAdminUsername, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clinicAdminToken);

        var providersResponse = await Client.GetAsync("/identity/providers");
        providersResponse.EnsureSuccessStatusCode();
        var providers = await providersResponse.Content.ReadFromJsonAsync<List<ProviderSummary>>(JsonOptions);
        providers.Should().NotBeNull().And.ContainSingle();
        providers![0].Id.Should().NotBe(Guid.Empty);

        var patientResponse = await Client.PostAsJsonAsync("/scheduling/patients", new PatientDto(
            patientFirstName,
            "Patient",
            new DateOnly(1995, 5, 5),
            "alex@example.com",
            "123 Main St",
            "+1 (215) 555-0100"));
        patientResponse.EnsureSuccessStatusCode();

        var createdPatient = await patientResponse.Content.ReadFromJsonAsync<PatientListItem>(JsonOptions);
        createdPatient.Should().NotBeNull();
        createdPatient!.FirstName.Should().Be(patientFirstName);
        createdPatient.Id.Should().NotBe(Guid.Empty);

        Client.DefaultRequestHeaders.Authorization = null;

        return new SchedulingTestContext(
            clinicAdminToken,
            providers[0].Id,
            createdPatient.Id);
    }

    private async Task BookAppointmentAsync(
        string token,
        Guid patientId,
        Guid providerId,
        DateTime start,
        DateTime end,
        bool forceBooking = false)
    {
        var response = await BookAppointmentRawAsync(token, patientId, providerId, start, end, forceBooking);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, $"booking failed: {body}");
    }

    private async Task<HttpResponseMessage> BookAppointmentRawAsync(
        string token,
        Guid patientId,
        Guid providerId,
        DateTime start,
        DateTime end,
        bool forceBooking = false)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await Client.PostAsJsonAsync("/scheduling/appointments", new CreateAppointmentDto(
            patientId,
            providerId,
            start,
            end,
            forceBooking));
    }

    private sealed record SchedulingTestContext(string ClinicAdminToken, Guid ProviderId, Guid PatientId);
    private sealed record ProviderSummary(Guid Id, string Username, string FirstName, string LastName, int RoleId, string RoleName);
    private sealed record PatientListItem(Guid Id, string FirstName, string LastName);
    private sealed record AppointmentListItem(Guid Id, Guid PatientId, Guid ProviderId, DateTime StartTime, DateTime EndTime, int Status);
}
