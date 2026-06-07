using HPMS.SharedKernel.Interfaces;

namespace HPMS.Tests.Unit;

/// <summary>
/// Stable tenant provider instance for EF global query filters in unit tests.
/// EF captures the provider reference in compiled filters; Moq instances break that binding.
/// </summary>
public sealed class TestTenantProvider : ITenantProvider
{
    public Guid TenantId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public Guid GetTenantId() => TenantId;
}
