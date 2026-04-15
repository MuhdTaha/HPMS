using Microsoft.EntityFrameworkCore;
using HPMS.SharedKernel.Interfaces;

namespace HPMS.SharedKernel.Extensions;

public static class DbContextExtensions
{
    public static void ApplyGlobalFilters(this ModelBuilder modelBuilder, ITenantProvider tenantProvider)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(DbContextExtensions)
                    .GetMethod(nameof(ApplyTypeFilters), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(null, new object[] { modelBuilder, tenantProvider });
            }
        }
    }

    private static void ApplyTypeFilters<T>(ModelBuilder modelBuilder, ITenantProvider tenantProvider) 
        where T : class, IHasTenant
    {
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            modelBuilder.Entity<T>().HasQueryFilter(x => 
                x.TenantId == tenantProvider.GetTenantId() && 
                !((ISoftDelete)x).IsDeleted);
        }
        else
        {
            modelBuilder.Entity<T>().HasQueryFilter(x => 
                x.TenantId == tenantProvider.GetTenantId());
        }
    }

    public static void StampTenantIds(this Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker changeTracker, Guid tenantId)
    {
        foreach (var entry in changeTracker.Entries<IHasTenant>())
        {
            if (entry.State == EntityState.Added && (entry.Entity.TenantId == Guid.Empty))
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}