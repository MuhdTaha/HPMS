using System.Security.Claims;
using HPMS.SharedKernel.Interfaces;

namespace HPMS.Web.Services;

public class ClaimsTenantProvider(IHttpContextAccessor httpContextAccessor) : ITenantProvider
{
    public Guid GetTenantId()
    {
        // 1. Look at the current HTTP Request
        var user = httpContextAccessor.HttpContext?.User;

        // 2. Find the 'TenantId' claim inside the user's Token
        var tenantClaim = user?.FindFirst("TenantId")?.Value;

        // 3. Convert it from a string to a Guid
        if (Guid.TryParse(tenantClaim, out var tenantId))
        {
            return tenantId;
        }

        // For now, if no tenant is found (like during onboarding), return empty.
        // In a real app, you might throw an unauthorized exception here.
        return Guid.Empty;
    }
}