namespace GameWeaverShared.Parsers;

public class ParserResult
{
    public bool Succeeded { get; set; }
    public List<string> Messages { get; set; } = [];
}