using System.Text.Json;
using HPMS.Scheduling.Data;
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
    
    public string PHI_Data { get; set; } = string.Empty; // Stores encrypted JSON blob of the PatientPhi class
    
    // Helper property (Not mapped to DB) to work with the data easily
    public PatientPhi? DecryptedPhi => string.IsNullOrEmpty(PHI_Data) 
        ? new PatientPhi() 
        : JsonSerializer.Deserialize<PatientPhi>(PHI_Data);
}