using FluentAssertions;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using HPMS.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace HPMS.Tests.Unit;

public class SchedulingDbContextTests
{
    private readonly SchedulingDbContext _context;
    private readonly TestTenantProvider _tenantProvider = new();
    private readonly Guid _testTenantId;

    public SchedulingDbContextTests()
    {
        _testTenantId = _tenantProvider.TenantId;

        var options = new DbContextOptionsBuilder<SchedulingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SchedulingDbContext(options, _tenantProvider);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldAutomaticallyStampTenantId_WhenEntityIsAdded()
    {
        // Arrange: Create a new appointment WITHOUT setting TenantId
        var appointment = new Appointment
        {
            ProviderId = Guid.NewGuid(),
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddMinutes(30),
            RowVersion = new byte[8]
            // Note: TenantId is NOT set here
        };

        // Act: Add the appointment and save changes
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        // Assert: The TenantId should be automatically set to _testTenantId
        appointment.TenantId.Should().Be(_testTenantId);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldExcludeData_FromOtherTenants()
    {
        // Arrange: Create an appointment for a different tenant
        var otherTenantId = Guid.NewGuid();
        
        // Using a "dirty" trick: since we want to bypass the provider for SETUP, 
        // we add it directly. In-Memory behaves slightly differently than SQL here, 
        // but it proves the filter logic on the GET side.
        var secretAppointment = new Appointment
        {
            TenantId = otherTenantId,
            ProviderId = Guid.NewGuid(),
            RowVersion = new byte[8]
        };

        _context.Appointments.Add(secretAppointment);
        await _context.SaveChangesAsync();

        // Act: Query the appointments as the current tenant (_testTenantId)
        var results = await _context.Appointments.ToListAsync();

        // Assert: The results should NOT contain the appointment from the other tenant
        results.Should().NotContain(a => a.TenantId == otherTenantId);
        results.Count.Should().Be(0);
    }

    [Fact]
    public async Task GlobalQueryFilter_ShouldExclude_SoftDeletedEntities()
    {
        // Arrange: Create a soft-deleted appointment for the current tenant
        var deletedAppointment = new Appointment
        {
            TenantId = _testTenantId,
            IsDeleted = true,
            ProviderId = Guid.NewGuid(),
            RowVersion = new byte[8]
        };

        _context.Appointments.Add(deletedAppointment);
        await _context.SaveChangesAsync();

        // Act: Query the appointments as the current tenant
        var results = await _context.Appointments.ToListAsync();

        // Assert: The results should NOT contain the soft-deleted appointment
        results.Should().BeEmpty();
    }
}