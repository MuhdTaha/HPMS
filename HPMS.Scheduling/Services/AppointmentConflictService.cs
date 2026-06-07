using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Data;
using HPMS.Scheduling.Entities;
using HPMS.SharedKernel.Interfaces;

namespace HPMS.Scheduling.Services;

public class AppointmentConflictService(
    SchedulingDbContext db,
    ITenantProvider tenantProvider) : IAppointmentConflictService
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

        var tenantId = tenantProvider.GetTenantId();

        var hasConflict = await db.Appointments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(a =>
                a.TenantId == tenantId &&
                !a.IsDeleted &&
                a.ProviderId == providerId &&
                a.Status != AppointmentStatus.NoShow &&
                a.Status != AppointmentStatus.Canceled &&
                (!ignoreAppointmentId.HasValue || a.Id != ignoreAppointmentId.Value) &&
                start < a.EndTime &&
                a.StartTime < end,
                cancellationToken);

        return !hasConflict;
    }
}

