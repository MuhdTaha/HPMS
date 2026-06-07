namespace HPMS.SharedKernel.Interfaces;

/// <summary>
/// Validates that a user ID refers to an active Provider in the given tenant.
/// Implemented in HPMS.Web using Identity data to preserve module boundaries.
/// </summary>
public interface IProviderValidator
{
    Task<bool> IsValidProviderAsync(Guid providerId, Guid tenantId, CancellationToken cancellationToken = default);
}
