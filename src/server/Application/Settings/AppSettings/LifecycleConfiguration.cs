using System.ComponentModel.DataAnnotations;
using Domain.Enums.Lifecycle;

namespace Application.Settings.AppSettings;

public class LifecycleConfiguration : IAppSettingsSection
{
    public const string SectionName = "Lifecycle";
    
    public bool EnforceTestAccounts { get; set; }
    
    public bool EnforceDefaultRolePermissions { get; set; }
    
    public bool AuditLoginLogout { get; set; }

    [EnumDataType(typeof(CleanupTimeframe))]
    public CleanupTimeframe AuditLogLifetime { get; set; } = CleanupTimeframe.OneYear;
}