namespace HPMS.SharedKernel.Interfaces;

public interface IHasTenant
{
    public Guid TenantId { get; set; }
}