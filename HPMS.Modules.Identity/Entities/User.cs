using HPMS.SharedKernel.Interfaces;

namespace HPMS.Modules.Identity.Entities;

// We use 'public record' for a clean, data-focused class
public record User : IHasTenant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    // This is the "Contract" we signed with IHasTenant
    public Guid TenantId { get; set; } 
    
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Soft delete flag for HIPAA compliance
    public bool IsDeleted { get; set; } = false;
}