namespace Application.Models.Handlers;

public class IniSection
{
    public string Name { get; set; } = "";
    public List<IniKeyValue> Keys { get; set; } = [];
}