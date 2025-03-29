namespace GameServerQuery.Models;

public class ClientSendResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int BytesSent { get; set; } = -1;
}