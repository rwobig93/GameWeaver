using Domain.Enums.Integrations;

namespace Application.Models.Integrations;

public class FileStorageRecordUpdate
{
    public Guid Id { get; set; }
    public FileStorageFormat? Format { get; set; }
    public FileStorageType? LinkedType { get; set; }
    public Guid? LinkedId { get; set; }
    public string? FriendlyName { get; set; }
    public string? Filename { get; set; }
    public string? Description { get; set; }
    public string? HashSha256 { get; set; }
    public string? Version { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }
    public bool? IsDeleted { get; set; }
    public DateTime? DeletedOn { get; set; }
}