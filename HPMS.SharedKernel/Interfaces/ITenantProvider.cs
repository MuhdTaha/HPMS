namespace HPMS.SharedKernel.Interfaces;

public interface ITenantProvider
{
    Guid GetTenantId();
}