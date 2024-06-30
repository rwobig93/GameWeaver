using Domain.Enums.Identity;

namespace Application.Requests.Identity.User;

public class GetUserExtendedAttributesByOwnerIdAndType
{
    public Guid Id { get; set; }
    public AttributeType Type { get; set; }
}