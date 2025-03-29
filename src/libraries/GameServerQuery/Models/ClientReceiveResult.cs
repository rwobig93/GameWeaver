namespace GameServerQuery.Models;

public class ClientReceiveResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[] Response { get; set; } = [];
}