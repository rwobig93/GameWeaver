namespace Application.Models.Lifecycle;

public class AuditDiff
{
    public Dictionary<string, string> Before { get; set; } = new();
    public Dictionary<string, string> After { get; set; } = new();
}