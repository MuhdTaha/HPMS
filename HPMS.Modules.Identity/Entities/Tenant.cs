namespace HPMS.Modules.Identity.Entities;

public record Tenant
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}