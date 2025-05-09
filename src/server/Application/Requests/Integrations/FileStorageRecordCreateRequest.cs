using Domain.Enums.Integrations;

namespace Application.Requests.Integrations;

public class FileStorageRecordCreateRequest
{
    public FileStorageFormat Format { get; set; }
    public FileStorageType LinkedType { get; set; }
    public Guid LinkedId { get; set; }
    public string FriendlyName { get; set; } = "";
    public string Filename { get; set; } = null!;
    public string Description { get; set; } = "";
    public string Version { get; set; } = "";
    public long SizeBytes { get; set; }
}