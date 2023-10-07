using Application.Models.Web;

namespace Application.Requests.v1.Identity.User;

public class DeleteUserRequest : ApiObjectFromQuery<DeleteUserRequest>
{
    public Guid Id { get; set; }
}