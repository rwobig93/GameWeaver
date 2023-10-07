namespace Application.Requests.Api;

public class ApiGetTokenRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}