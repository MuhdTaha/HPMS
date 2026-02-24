using HPMS.SharedKernel.Interfaces;

namespace HPMS.Web.Services;

public class FakeTenantProvider : ITenantProvider
{
    // For now, we return a hardcoded ID so the app doesn't crash.
    // In Phase 5, this will read the actual ID from the user's login token.
    public Guid GetTenantId() => Guid.Parse("00000000-0000-0000-0000-000000000001");
}