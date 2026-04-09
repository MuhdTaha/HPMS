using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Entities;
using HPMS.SharedKernel.Interfaces;

namespace HPMS.Scheduling.Data;

public class SchedulingDbContext(
    DbContextOptions<SchedulingDbContext> options,
    ITenantProvider tenantProvider ) : DbContext(options)
{
    // Define DbSets for the entities in the scheduling module.
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Explicitly configure the RowVersion property on the Appointment entity to be a concurrency token.
        modelBuilder.Entity<Appointment>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
        
        // Apply multi-tenant filters to entities that implement IHasTenant.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if the entity implements IHasTenant and apply a global query filter to ensure tenant isolation.
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                var hasSoftDeleteProperty = entityType.ClrType.GetProperty(nameof(ISoftDelete.IsDeleted)) != null;
                var filterMethodName = hasSoftDeleteProperty && typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType)
                    ? nameof(ApplyTenantAndSoftDeleteFilter)
                    : nameof(ApplyTenantFilter);

                var method = typeof(SchedulingDbContext)
                    .GetMethod(filterMethodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
    }

    // Helper method to apply a global query filter for multi-tenancy on entities that implement IHasTenant.
    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, IHasTenant
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => x.TenantId == tenantProvider.GetTenantId());
    }

    private void ApplyTenantAndSoftDeleteFilter<T>(ModelBuilder modelBuilder) where T : class, IHasTenant, ISoftDelete
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => x.TenantId == tenantProvider.GetTenantId() && !x.IsDeleted);
    }
}