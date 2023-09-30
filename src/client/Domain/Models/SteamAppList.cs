namespace Domain.Models;

public class SteamAppList
{
    public Applist Applist { get; set; }
}

public class Applist
{
    public App[] Apps { get; set; }
}

public class App
{
    public int AppId { get; set; }
    public string Name { get; set; }
}
