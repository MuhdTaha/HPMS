using MediatR;

namespace HPMS.SharedKernel.Events;

// INotification is MediatR's way of saying "One message, multiple listeners allowed"
public record AppointmentCompletedEvent(
    Guid AppointmentId, 
    Guid PatientId, 
    Guid TenantId,
    decimal VisitFee = 150.00m // Default for now
) : INotification;