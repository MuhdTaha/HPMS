namespace HPMS.SharedKernel.Authorization;

/// <summary>
/// Named authorization policies registered in HPMS.Web and applied on minimal API routes.
/// </summary>
public static class HpmsPolicies
{
    public const string SystemAdmin = nameof(SystemAdmin);
    public const string ClinicAdminOrAbove = nameof(ClinicAdminOrAbove);
    public const string ClinicalStaff = nameof(ClinicalStaff);
    public const string SchedulingWrite = nameof(SchedulingWrite);
    public const string VisitManagement = nameof(VisitManagement);
    public const string BillingStaff = nameof(BillingStaff);
}
