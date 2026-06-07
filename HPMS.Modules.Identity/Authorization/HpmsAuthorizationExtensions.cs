using HPMS.SharedKernel.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace HPMS.Modules.Identity.Authorization;

public static class HpmsAuthorizationExtensions
{
    public static IServiceCollection AddHpmsAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(HpmsPolicies.SystemAdmin, policy =>
                policy.RequireRole(HpmsRoles.SystemAdmin));

            options.AddPolicy(HpmsPolicies.ClinicAdminOrAbove, policy =>
                policy.RequireRole(HpmsRoles.SystemAdmin, HpmsRoles.ClinicAdmin));

            options.AddPolicy(HpmsPolicies.ClinicalStaff, policy =>
                policy.RequireRole(
                    HpmsRoles.SystemAdmin,
                    HpmsRoles.ClinicAdmin,
                    HpmsRoles.Provider,
                    HpmsRoles.FrontDesk));

            options.AddPolicy(HpmsPolicies.SchedulingWrite, policy =>
                policy.RequireRole(
                    HpmsRoles.SystemAdmin,
                    HpmsRoles.ClinicAdmin,
                    HpmsRoles.FrontDesk));

            options.AddPolicy(HpmsPolicies.VisitManagement, policy =>
                policy.RequireRole(
                    HpmsRoles.SystemAdmin,
                    HpmsRoles.ClinicAdmin,
                    HpmsRoles.Provider,
                    HpmsRoles.FrontDesk));

            options.AddPolicy(HpmsPolicies.BillingStaff, policy =>
                policy.RequireRole(
                    HpmsRoles.SystemAdmin,
                    HpmsRoles.ClinicAdmin,
                    HpmsRoles.BillingManager));
        });

        return services;
    }
}
