using System.ComponentModel.DataAnnotations;
using Domain.Enums.Database;

namespace Application.Settings.AppSettings;

public class DatabaseConfiguration : IAppSettingsSection
{
    public const string SectionName = "Database";
    
    [EnumDataType(typeof(DatabaseProviderType))]
    public DatabaseProviderType Provider { get; init; } = DatabaseProviderType.MsSql;
    
    [Required]
    public string Core { get; init; } = "data source=<hostname>;initial catalog=<database>;user id=<db_username>;password=<db_password>;";
}