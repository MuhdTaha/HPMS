using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;

namespace HPMS.Scheduling.Services;

public class AppointmentConflictService(SchedulingDbContext db) : IAppointmentConflictService
{
    public async Task<bool> IsSlotAvailableAsync(
        Guid providerId,
        DateTime start,
        DateTime end,
        Guid? ignoreAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        if (start >= end)
        {
            throw new ArgumentException("Appointment end time must be after start time.");
        }

        // Tenant scoping is enforced by the global query filter on SchedulingDbContext.
        var hasConflict = await db.Appointments
            .AsNoTracking()
            .AnyAsync(a =>
                a.ProviderId == providerId &&
                a.Status != AppointmentStatus.NoShow &&
                (!ignoreAppointmentId.HasValue || a.Id != ignoreAppointmentId.Value) &&
                start < a.EndTime &&
                a.StartTime < end,
                cancellationToken);

        return !hasConflict;
    }
}

