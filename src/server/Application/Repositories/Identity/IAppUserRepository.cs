using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Domain.Contracts;
using Domain.DatabaseEntities.Identity;
using Domain.Enums.Identity;
using Domain.Models.Database;
using Domain.Models.Identity;

namespace Application.Repositories.Identity;

public interface IAppUserRepository
{
    Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> GetAllAsync();
    Task<DatabaseActionResult<IEnumerable<AppUserServicePermissionDb>>> GetAllServiceAccountsForPermissionsAsync();
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllServiceAccountsPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllDisabledPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> GetAllLockedOutPaginatedAsync(int pageNumber, int pageSize);
    Task<DatabaseActionResult<int>> GetCountAsync();
    Task<DatabaseActionResult<AppUserSecurityDb>> GetByIdAsync(Guid id);
    Task<DatabaseActionResult<AppUserFullDb?>> GetByIdFullAsync(Guid id);
    Task<DatabaseActionResult<AppUserSecurityDb>> GetByIdSecurityAsync(Guid id);
    Task<DatabaseActionResult<AppUserSecurityDb>> GetByUsernameAsync(string username);
    Task<DatabaseActionResult<AppUserFullDb>> GetByUsernameFullAsync(string username);
    Task<DatabaseActionResult<AppUserSecurityDb>> GetByUsernameSecurityAsync(string username);
    Task<DatabaseActionResult<AppUserSecurityDb>> GetByEmailAsync(string email);
    Task<DatabaseActionResult<AppUserFullDb>> GetByEmailFullAsync(string email);
    Task<DatabaseActionResult<Guid>> CreateAsync(AppUserCreate createObject);
    Task<DatabaseActionResult> UpdateAsync(AppUserUpdate updateObject);
    Task<DatabaseActionResult> DeleteAsync(Guid id, Guid modifyingUserId);
    Task<DatabaseActionResult<Guid>> SetUserId(Guid currentId, Guid newId);
    Task<DatabaseActionResult> SetCreatedById(Guid userId, Guid createdById);
    Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> SearchAsync(string searchText);
    Task<DatabaseActionResult<PaginatedDbEntity<IEnumerable<AppUserSecurityDb>>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<DatabaseActionResult<Guid>> AddExtendedAttributeAsync(AppUserExtendedAttributeCreate addAttribute);
    Task<DatabaseActionResult> UpdateExtendedAttributeAsync(Guid attributeId, string? value, string? description);
    Task<DatabaseActionResult> RemoveExtendedAttributeAsync(Guid attributeId);
    Task<DatabaseActionResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate);
    Task<DatabaseActionResult<AppUserPreferenceDb>> GetPreferences(Guid userId);
    Task<DatabaseActionResult<AppUserExtendedAttributeDb>> GetExtendedAttributeByIdAsync(Guid attributeId);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb?>>> GetExtendedAttributeByTypeAndValueAsync(
        ExtendedAttributeType type, string value);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByTypeAsync(
        Guid userId, ExtendedAttributeType type);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByTypeAndValueAsync(
        Guid userId, ExtendedAttributeType type, string value);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetUserExtendedAttributesByNameAsync(Guid userId, string name);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllUserExtendedAttributesAsync(Guid userId);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesByTypeAsync(ExtendedAttributeType type);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesByNameAsync(string name);
    Task<DatabaseActionResult<IEnumerable<AppUserExtendedAttributeDb>>> GetAllExtendedAttributesAsync();
    Task<DatabaseActionResult<Guid>> CreateSecurityAsync(AppUserSecurityAttributeCreate securityCreate);
    Task<DatabaseActionResult<AppUserSecurityAttributeDb>> GetSecurityAsync(Guid userId);
    Task<DatabaseActionResult> UpdateSecurityAsync(AppUserSecurityAttributeUpdate securityUpdate);
    Task<DatabaseActionResult<IEnumerable<AppUserSecurityDb>>> GetAllLockedOutAsync();
}