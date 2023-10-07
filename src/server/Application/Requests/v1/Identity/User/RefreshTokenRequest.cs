namespace Application.Requests.v1.Identity.User;

public class LocalStorageRequest
{
    public string? ClientId { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}