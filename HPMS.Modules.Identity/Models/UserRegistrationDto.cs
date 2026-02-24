namespace HPMS.Modules.Identity.DTO;

public record UserRegistrationDto(
    Guid TenantId, 
    string Username, 
    string Email, 
    string Password,
    string FirstName, 
    string LastName);