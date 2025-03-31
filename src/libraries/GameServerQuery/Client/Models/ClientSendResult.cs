namespace GameServerQuery.Client.Models;

public class ClientSendResult
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public int BytesSent { get; set; } = -1;
}