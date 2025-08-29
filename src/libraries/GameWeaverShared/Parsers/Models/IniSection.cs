namespace GameWeaverShared.Parsers.Models;

public class IniSection
{
    public string Name { get; set; } = "";
    public List<IniKeyValue> Keys { get; set; } = [];
}