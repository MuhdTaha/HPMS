using HPMS.Modules.Identity.Data;
using HPMS.SharedKernel.Authorization;
using HPMS.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HPMS.Web.Services;

public class IdentityProviderValidator(IdentityDbContext db) : IProviderValidator
{
    public async Task<bool> IsValidProviderAsync(
        Guid providerId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(
                u => u.Id == providerId &&
                     u.TenantId == tenantId &&
                     u.RoleId == HpmsRoleIds.Provider &&
                     !u.IsDeleted,
                cancellationToken);
    }
}
