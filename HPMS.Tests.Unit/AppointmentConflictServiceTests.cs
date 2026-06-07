using FluentAssertions;
using HPMS.Scheduling;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using HPMS.Scheduling.Services;
using Microsoft.EntityFrameworkCore;
namespace HPMS.Tests.Unit;

public class AppointmentConflictServiceTests
{
    private readonly SchedulingDbContext _context;
    private readonly AppointmentConflictService _service;
    private static readonly TestTenantProvider TenantProvider = new();
    private static readonly Guid TestTenantId = TenantProvider.TenantId;

    public AppointmentConflictServiceTests()
    {
        var options = new DbContextOptionsBuilder<SchedulingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SchedulingDbContext(options, TenantProvider);
        _service = new AppointmentConflictService(_context, TenantProvider);
    }

    [Fact]
    public async Task IsSlotAvailable_ShouldReturnFalse_WhenTimeOverlaps()
    {
        // Arrange: Add an existing appointment to the "fake" DB
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10); // 10:00 AM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = providerId,
            StartTime = baseTime,
            EndTime = baseTime.AddHours(1), // 10:00 - 11:00
            Status = AppointmentStatus.Scheduled,
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();

        // Act: Try to book a conflicting slot (10:30 - 11:30)
        var result = await _service.IsSlotAvailableAsync(
            providerId, 
            baseTime.AddMinutes(30), 
            baseTime.AddHours(1.5));

        // Assert: It should detect the conflict
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsSlotAvailable_ShouldReturnTrue_WhenTimeIsAfterExisting()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10); // 10:00 AM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = providerId,
            StartTime = baseTime,
            EndTime = baseTime.AddHours(1), // 10:00 - 11:00
            Status = AppointmentStatus.Scheduled,
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();

        // Act: Book a slot after (11:00 - 12:00)
        var result = await _service.IsSlotAvailableAsync(
            providerId, 
            baseTime.AddHours(1), 
            baseTime.AddHours(2));

        // Assert: No conflict
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsSlotAvailable_ShouldReturnTrue_WhenNewSlotStartsExactlyWhenExistingEnds()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(10); // 10:00 AM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = providerId,
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30), // 10:00 - 10:30 AM
            Status = AppointmentStatus.Scheduled,
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();
        
        // Act: New slot starts exactly when existing ends (10:30 - 11:00)
        var result = await _service.IsSlotAvailableAsync(
            providerId,
            baseTime.AddMinutes(30),
            baseTime.AddHours(1)
        );
        
        // Assert: No conflict since it starts right after the existing appointment ends
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsSlotAvailable_ShouldReturnTrue_WhenExistingAppointmentIsCancelled()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(14); // 2:00 PM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = providerId,
            StartTime = baseTime,
            EndTime = baseTime.AddHours(1),
            Status = AppointmentStatus.Canceled, // Logic should ignore this
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();

        // Act: Try to book the same slot (2:00 - 3:00 PM)
        var result = await _service.IsSlotAvailableAsync(providerId, baseTime, baseTime.AddHours(1));

        // Assert: Should be available since the existing appointment is canceled
        result.Should().BeTrue();
    }
    
    [Fact]
    public async Task IsSlotAvailable_ShouldReturnFalse_WhenNewSlotEnvelopesExistingAppointment()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(9); // 9:00 AM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = providerId,
            StartTime = baseTime.AddMinutes(30), // 9:30
            EndTime = baseTime.AddMinutes(45),   // 9:45
            Status = AppointmentStatus.Scheduled,
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();

        // Act: New slot from 9:00 to 10:00
        var result = await _service.IsSlotAvailableAsync(providerId, baseTime, baseTime.AddHours(1));

        // Assert: Should detect conflict since the new slot completely overlaps the existing appointment
        result.Should().BeFalse();
    }
    
    [Fact]
    public async Task IsSlotAvailable_ShouldReturnTrue_WhenConflictExistsButForDifferentProvider()
    {
        // Arrange
        var doctor1 = Guid.NewGuid();
        var doctor2 = Guid.NewGuid();
        var baseTime = DateTime.UtcNow.Date.AddHours(8); // 8:00 AM

        _context.Appointments.Add(new Appointment
        {
            TenantId = TestTenantId,
            ProviderId = doctor1,
            StartTime = baseTime,
            EndTime = baseTime.AddHours(1),
            Status = AppointmentStatus.Scheduled,
            RowVersion = new byte[8]
        });
        await _context.SaveChangesAsync();

        // Act: Book doctor 2 at the same time
        var result = await _service.IsSlotAvailableAsync(doctor2, baseTime, baseTime.AddHours(1));

        // Assert: Should be available since the conflict is for a different provider
        result.Should().BeTrue();
    }
    
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Arrived, true)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Scheduled, false)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Canceled, true)]
    public void StateMachine_ShouldValidateTransitions(AppointmentStatus current, AppointmentStatus next, bool expected)
    {
        // Act: Check if the transition is valid according to the state machine rules
        var result = SchedulingModule.IsValidTransition(current, next);

        // Assert: The result should match the expected outcome based on the defined state machine rules
        result.Should().Be(expected);
    }
}