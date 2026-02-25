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
    public DbSet<Role> Roles => Set<Role>();

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
        
        // Seed default roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "SystemAdmin", Description = "Platform owner" },
            new Role { Id = 2, Name = "ClinicAdmin", Description = "Manages the clinic" },
            new Role { Id = 3, Name = "Provider", Description = "Doctors and Therapists" },
            new Role { Id = 4, Name = "BillingManager", Description = "Handles finances" },
            new Role { Id = 5, Name = "FrontDesk", Description = "Schedules and checks-in" }
        );
        
        // Seed a System Tenant (The "Owner" of the platform)
        var systemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    
        modelBuilder.Entity<Tenant>().HasData(new Tenant 
        { 
            Id = systemTenantId, 
            Name = "HPMS System Administration",
            IsActive = true 
        });
        
        // Seed a System Admin user for the System Tenant
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
            TenantId = systemTenantId,
            Username = "sysadmin",
            Email = "admin@company.com",
            FirstName = "System",
            LastName = "Admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            RoleId = 1 // SystemAdmin role
        });
    }
    
    // This helper method creates the 'Where' clause specifically for the entity type
    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, IHasTenant
    {
        modelBuilder.Entity<T>().HasQueryFilter(x => x.TenantId == tenantProvider.GetTenantId());
    }
}