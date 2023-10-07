namespace Domain.Models;

public class SteamAppList
{
    public required AppList AppList { get; set; }
}

public class AppList
{
    public required App[] Apps { get; set; }
}

public class App
{
    public int AppId { get; set; }
    public string Name { get; set; } = "";
}
