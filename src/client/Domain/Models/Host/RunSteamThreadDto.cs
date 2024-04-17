namespace Domain.Models.Host;

public class RunSteamThreadDto
{
    public Action<RunSteamDto>? RunSteamMethod { get; set; }
    public RunSteamDto SteamDto { get; set; } = new();
}
