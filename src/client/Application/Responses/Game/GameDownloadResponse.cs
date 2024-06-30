using Domain.Contracts;
using Domain.Models.ControlServer;

namespace Application.Responses.Game;

public class GameDownloadResponse : Result
{
    public GameDownload Data { get; set; } = new();
}