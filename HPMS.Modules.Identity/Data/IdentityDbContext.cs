using Microsoft.EntityFrameworkCore;
using HPMS.SharedKernel.Interfaces;
using HPMS.Modules.Identity.Entities;

namespace HPMS.Modules.Identity.Data;

public class IdentityDbContext(
    DbContextOptions<IdentityDbContext> options, 
    ITenantProvider tenantProvider) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // This loop finds every entity that implements IHasTenant
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Check if the entity implements IHasTenant
            if (typeof(IHasTenant).IsAssignableFrom(entityType.ClrType))
            { 
                // Call a helper method to apply the filter
                var method = typeof(IdentityDbContext)
                    .GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.MakeGenericMethod(entityType.ClrType);

                method?.Invoke(this, new object[] { modelBuilder });
            }
        }
        
        // Seed a System Tenant (The "Owner" of the platform)
        var systemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
        modelBuilder.Entity<Tenant>().HasData(new Tenant 
        { 
            Id = systemTenantId, 
            Name = "HPMS System Administration",
            IsActive = true 
        });
    }
    
    // This helper method creates the 'Where' clause specifically for the entity type
    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, IHasTenant
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => x.TenantId == tenantProvider.GetTenantId());
    }
}