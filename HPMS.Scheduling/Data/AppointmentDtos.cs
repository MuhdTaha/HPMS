namespace HPMS.Scheduling.Data;

// Used when creating a new patient record
public record PatientDto(
    string FirstName, 
    string LastName, 
    DateOnly DateOfBirth,
    string Email,
    string Address,
    string PhoneNumber,
    string? Ssn = null,
    string? InsuranceNumber = null,
    string? EmergencyContact = null);

// Used when a Front Desk user books a new slot
public record CreateAppointmentDto(
    Guid PatientId,
    Guid ProviderId,
    DateTime StartTime,
    DateTime EndTime,
    bool ForceBooking = false);

public record AppointmentListItemDto(
    Guid Id,
    Guid PatientId,
    Guid ProviderId,
    DateTime StartTime,
    DateTime EndTime,
    int Status);

// Used when updating the status (Scheduled -> Arrived -> Completed)
public record UpdateAppointmentStatusDto(
    Guid AppointmentId, 
    int NewStatus);