﻿using MemoryPack;

namespace Domain.Models.GameServer;


[MemoryPackable(SerializeLayout.Explicit)]
public partial class ConfigurationSet
{
    [MemoryPackOrder(0)]
    public Guid Id { get; set; }
    [MemoryPackOrder(1)]
    public bool DuplicateKey { get; set; }
    [MemoryPackOrder(2)]
    public string Category { get; set; } = null!;
    [MemoryPackOrder(3)]
    public string Key { get; set; } = null!;
    [MemoryPackOrder(4)]
    public string Value { get; set; } = null!;
}