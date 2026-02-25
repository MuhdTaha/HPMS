﻿namespace HPMS.Modules.Identity.Entities;

public record Role
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}