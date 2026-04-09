using HPMS.SharedKernel.Interfaces;

namespace HPMS.Scheduling.Entities;

// FSM for appointment status to help manage the lifecycle of an appointment
public enum AppointmentStatus
{
    Scheduled = 1,
    Arrived = 2,
    InSession = 3,
    Completed = 4,
    NoShow = 5,
    Canceled = 6
}

public record Appointment : IHasTenant, ISoftDelete
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; } // Foreign key to User entity with Role = Provider
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Scheduled;

    public bool IsDeleted { get; set; }
    
    // Concurrency token for optimistic concurrency control and to prevent double-booking
    public byte[] RowVersion { get; set; } = default!;
}