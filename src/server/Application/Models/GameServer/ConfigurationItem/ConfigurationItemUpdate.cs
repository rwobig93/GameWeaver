﻿namespace Application.Models.GameServer.ConfigurationItem;

public class ConfigurationItemUpdate
{
    public Guid Id { get; set; }
    public Guid? LocalResourceId { get; set; }
    public bool? DuplicateKey { get; set; }
    public string? Path { get; set; }
    public string? Category { get; set; }
    public string? Key { get; set; }
    public string? Value { get; set; }
    public string? FriendlyName { get; set; }
}