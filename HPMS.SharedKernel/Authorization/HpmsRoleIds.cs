namespace HPMS.SharedKernel.Authorization;

/// <summary>
/// Numeric role identifiers seeded in IdentityDbContext.
/// </summary>
public static class HpmsRoleIds
{
    public const int SystemAdmin = 1;
    public const int ClinicAdmin = 2;
    public const int Provider = 3;
    public const int BillingManager = 4;
    public const int FrontDesk = 5;
}
