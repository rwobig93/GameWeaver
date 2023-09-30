namespace Domain.Models;

public class ConfigurationSet
{
    public string CategoryParentPath { get; set; }
    public string Category { get; set; }
    public Dictionary<string, string> Properties { get; set; }
}