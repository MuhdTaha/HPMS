using HPMS.SharedKernel.Interfaces;

namespace HPMS.Modules.Billing.Entities;

public record FinancialLedger : IHasTenant
{
    public long Id { get; init; } // bigint in SQL
    public Guid TenantId { get; set; }
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Type { get; init; } = "Debit"; // Debit or Credit
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}