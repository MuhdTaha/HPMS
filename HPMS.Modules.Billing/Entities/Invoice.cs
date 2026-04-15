using HPMS.SharedKernel.Interfaces;

namespace HPMS.Modules.Billing.Entities;

public enum InvoiceStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Canceled = 4
}

public record Invoice : IHasTenant, ISoftDelete
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    
    public decimal Amount { get; set; }
    public DateTime DateGenerated { get; set; } = DateTime.UtcNow;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
    public bool IsDeleted { get; set; }
}