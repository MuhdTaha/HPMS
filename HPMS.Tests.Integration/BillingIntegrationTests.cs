using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using HPMS.Modules.Billing.Data;
using HPMS.Modules.Billing.Entities;
using HPMS.Modules.Identity.DTO;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HPMS.Tests.Integration;

public class BillingIntegrationTests(WebApplicationFactory<Program> factory) 
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task CompletingAppointment_ShouldCreate_InvoiceAndLedgerEntry()
    {
        // 1. Arrange: Create an authenticated user for a real tenant.
        var password = $"Test@12345!{Guid.NewGuid():N}";
        var tenant = await CreateTenantAsync($"Billing Tenant {Guid.NewGuid():N}");
        var username = $"billing_{Guid.NewGuid():N}";

        await RegisterUserAsync(tenant.Id, username, password);
        var token = await LoginAndGetTokenAsync(username, password);
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // 2. Arrange: Setup a patient and an appointment in the DB for the same tenant.
        var appointmentId = Guid.NewGuid();
        var patientId = Guid.NewGuid();

        using (var scope = Factory.Services.CreateScope())
        {
            var schedDb = scope.ServiceProvider.GetRequiredService<SchedulingDbContext>();
            schedDb.Appointments.Add(new Appointment
            {
                Id = appointmentId,
                TenantId = tenant.Id,
                PatientId = patientId,
                Status = AppointmentStatus.Scheduled,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                ProviderId = Guid.NewGuid()
            });
            await schedDb.SaveChangesAsync();
        }

        // 3. Act: Follow valid transition flow to reach Completed.
        var arrivedResponse = await Client.PatchAsJsonAsync($"/scheduling/appointments/{appointmentId}/status", new
        {
            AppointmentId = appointmentId,
            NewStatus = (int)AppointmentStatus.Arrived
        });
        arrivedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var inSessionResponse = await Client.PatchAsJsonAsync($"/scheduling/appointments/{appointmentId}/status", new
        {
            AppointmentId = appointmentId,
            NewStatus = (int)AppointmentStatus.InSession
        });
        inSessionResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await Client.PatchAsJsonAsync($"/scheduling/appointments/{appointmentId}/status", new
        {
            AppointmentId = appointmentId,
            NewStatus = (int)AppointmentStatus.Completed
        });

        // 4. Assert: Check the API response
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Verify: Check the Billing database for the side-effect
        using (var scope = Factory.Services.CreateScope())
        {
            var billingDb = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            
            // Re-apply tenant ID if your test provider isn't mocked globally
            var invoice = billingDb.Invoices
                .IgnoreQueryFilters() // Since the test runner might have a different TenantId
                .FirstOrDefault(i => i.AppointmentId == appointmentId);

            invoice.Should().NotBeNull();
            invoice!.Amount.Should().Be(150.00m);
            invoice.Status.Should().Be(InvoiceStatus.Pending);

            // Check Ledger
            var ledgerEntry = billingDb.FinancialLedgers
                .IgnoreQueryFilters()
                .FirstOrDefault(l => l.InvoiceId == invoice.Id);

            ledgerEntry.Should().NotBeNull();
            ledgerEntry!.Type.Should().Be("Debit");
        }

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
            FirstName: "Billing",
            LastName: "Test"));

        response.EnsureSuccessStatusCode();
    }

    private async Task<string> LoginAndGetTokenAsync(string username, string password, bool rememberMe = false)
    {
        var response = await Client.PostAsJsonAsync("/identity/login", new LoginRequest(username, password,  rememberMe));
        response.EnsureSuccessStatusCode();

        var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
        loginResponse.Should().NotBeNull();
        loginResponse.Token.Should().NotBeNullOrWhiteSpace();

        return loginResponse.Token;
    }

    private sealed record TenantResponse(Guid Id, string Name, bool IsActive, DateTime CreatedAt);

    private sealed record LoginResponse(string Token);
}