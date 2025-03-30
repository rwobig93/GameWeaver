namespace GameServerQuery.Client.Models;

public class ClientReceiveResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public byte[] Response { get; set; } = [];
}