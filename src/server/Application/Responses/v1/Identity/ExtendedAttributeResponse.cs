namespace Application.Responses.v1.Identity;

public class ExtendedAttributeResponse
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Type { get; set; } = null!;
}