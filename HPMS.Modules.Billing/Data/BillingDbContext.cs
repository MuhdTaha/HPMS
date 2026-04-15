using Microsoft.EntityFrameworkCore;
using HPMS.Modules.Billing.Entities;
using HPMS.SharedKernel.Interfaces;
using HPMS.SharedKernel.Extensions;

namespace HPMS.Modules.Billing.Data;

public class BillingDbContext(
    DbContextOptions<BillingDbContext> options,
    ITenantProvider tenantProvider) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<FinancialLedger> FinancialLedgers => Set<FinancialLedger>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure the Ledger Entry as an immutable record (no updates allowed)
        modelBuilder.Entity<FinancialLedger>()
            .Property(l => l.Amount).HasPrecision(18, 2); // Set precision for financial amounts
        
        // Apply shared global filters
        modelBuilder.ApplyGlobalFilters(tenantProvider);
    }

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        ChangeTracker.StampTenantIds(tenantProvider.GetTenantId());
        return base.SaveChangesAsync(ct);
    }
}