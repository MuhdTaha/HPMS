using Microsoft.EntityFrameworkCore;
using HPMS.Scheduling.Entities;
using HPMS.SharedKernel.Interfaces;
using HPMS.SharedKernel.Services;
using HPMS.SharedKernel.Extensions;

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
        
        // Use value converter for name encryption on the Patient entity.
        var patientEntity = modelBuilder.Entity<Patient>();
        
        // 1. Module-Specific: PHI Encryption
        modelBuilder.Entity<Patient>()
            .Property(p => p.PHI_Data)
            .HasConversion(
                v => EncryptionHelper.Encrypt(v), // Encrypt when saving to DB
                v => EncryptionHelper.Decrypt(v)  // Decrypt when reading from DB
            );
        
        // 2. Module-Specific: Concurrency
        modelBuilder.Entity<Appointment>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
        
        // 3. Shared: Global Filters (Tenant Isolation + Soft Delete)
        modelBuilder.ApplyGlobalFilters(tenantProvider);
    }

    public override int SaveChanges()
    {
        StampTenantIds();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ChangeTracker.StampTenantIds(tenantProvider.GetTenantId());
        return base.SaveChangesAsync(ct);
    }

    // Method to apply a global query filter for tenant isolation and soft deletes.
    private void ApplyFilters<T>(ModelBuilder modelBuilder) where T : class, IHasTenant
    {
        // We check if the class ALSO implements ISoftDelete
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(T)))
        {
            // Apply BOTH Tenant Isolation AND Soft Delete
            modelBuilder.Entity<T>().HasQueryFilter(x => 
                x.TenantId == tenantProvider.GetTenantId() && 
                !((ISoftDelete)x).IsDeleted);
        }
        else
        {
            // Apply ONLY Tenant Isolation
            modelBuilder.Entity<T>().HasQueryFilter(x => 
                x.TenantId == tenantProvider.GetTenantId());
        }
    }

    // Method to stamp TenantIds on new entities before they are saved to the database.
    private void StampTenantIds()
    {
        var currentTenantId = tenantProvider.GetTenantId();

        foreach (var entry in ChangeTracker.Entries<IHasTenant>())
        {
            if (entry.State == EntityState.Added && entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = currentTenantId;
            }
        }
    }
}