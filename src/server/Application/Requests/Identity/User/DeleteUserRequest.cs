using Application.Models.Web;

namespace Application.Requests.Identity.User;

public class DeleteUserRequest : ApiObjectFromQuery<DeleteUserRequest>
{
    public Guid Id { get; set; }
}