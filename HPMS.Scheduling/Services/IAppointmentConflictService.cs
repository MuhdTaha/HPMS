namespace HPMS.Scheduling.Services;

public interface IAppointmentConflictService
{
    Task<bool> IsSlotAvailableAsync(
        Guid providerId,
        DateTime start,
        DateTime end,
        Guid? ignoreAppointmentId = null,
        CancellationToken cancellationToken = default);
}

