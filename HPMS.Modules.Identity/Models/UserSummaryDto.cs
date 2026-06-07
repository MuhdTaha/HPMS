namespace HPMS.Modules.Identity.DTO;

public record UserSummaryDto(
    Guid Id,
    string Username,
    string FirstName,
    string LastName,
    int RoleId,
    string RoleName);
