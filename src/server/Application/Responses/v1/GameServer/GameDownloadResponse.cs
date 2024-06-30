namespace Application.Responses.v1.GameServer;

public class GameDownloadResponse
{
    public Guid Id { get; set; }
    public string GameName { get; set; } = "";
    public string FileName { get; set; } = null!;
    public string Version { get; set; } = "";
    public string HashSha256 { get; set; } = null!;
    public byte[] Content { get; set; } = null!;
}