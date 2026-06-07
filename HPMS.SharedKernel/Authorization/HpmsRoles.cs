namespace HPMS.SharedKernel.Authorization;

/// <summary>
/// Role names seeded in IdentityDbContext. Must match JWT role claims exactly.
/// </summary>
public static class HpmsRoles
{
    public const string SystemAdmin = "SystemAdmin";
    public const string ClinicAdmin = "ClinicAdmin";
    public const string Provider = "Provider";
    public const string BillingManager = "BillingManager";
    public const string FrontDesk = "FrontDesk";
}
