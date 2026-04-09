using HPMS.SharedKernel.Interfaces;

namespace HPMS.Scheduling.Entities;

public record Patient : IHasTenant, ISoftDelete
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    
    // AES-256 encrypted fields for PHI will be added later
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    
    public bool IsDeleted { get; set; } // Soft delete flag for HIPAA compliance
}