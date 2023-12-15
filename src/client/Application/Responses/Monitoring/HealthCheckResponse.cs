namespace Application.Responses.Monitoring;

public class HealthCheckResponse
{
    public string status { get; set; } = "";
    public string totalDuration { get; set; } = "";
    public Entries entries { get; set; } = new();
}

public class Entries
{
    public Database Database { get; set; } = new();
}

public class Database
{
    public string duration { get; set; } = "";
    public string status { get; set; } = "";
    public object[] tags { get; set; }
}

