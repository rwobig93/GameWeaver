namespace GameServerQuery.Client.Models;

public class ConnectionResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = null!;
}