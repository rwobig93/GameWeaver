namespace Application.Requests.Integrations;

public class FileStorageRecordUpdateRequest
{
    public Guid Id { get; set; }
    public string FriendlyName { get; set; } = "";
    public string Filename { get; set; } = null!;
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
}