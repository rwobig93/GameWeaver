﻿using Domain.Enums.Lifecycle;

namespace Application.Models.Lifecycle;

public class AuditTrailSlim
{
    public Guid Id { get; set; }
    public string TableName { get; set; } = null!;
    public Guid RecordId { get; set; }
    public Guid ChangedBy { get; set; }
    public string ChangedByUsername { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public AuditAction Action { get; set; }
    public Dictionary<string, string>? Before { get; set; }
    public Dictionary<string, string> After { get; set; } = null!;
}