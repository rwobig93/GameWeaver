namespace Domain.Models;

public class RunSteamThreadDto
{
    public Action<RunSteamDto> RunSteamMethod { get; set; }
    public RunSteamDto SteamDto { get; set; }
}
