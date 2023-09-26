using Domain.Enums.Identity;

namespace Domain.DatabaseEntities.Identity;

public class AppUserExtendedAttributeDb
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public string Name { get; set; } = "";
    public string Value { get; set; } = "";
    public string Description { get; set; } = "";
    public ExtendedAttributeType Type { get; set; }
}