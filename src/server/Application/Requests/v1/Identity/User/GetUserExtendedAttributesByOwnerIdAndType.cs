using Domain.Enums.Identity;

namespace Application.Requests.v1.Identity.User;

public class GetUserExtendedAttributesByOwnerIdAndType
{
    public Guid Id { get; set; }
    public AttributeType Type { get; set; }
}