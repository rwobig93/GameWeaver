using Application.Models.Identity.User;
using Application.Models.Identity.UserExtensions;
using Application.Models.Web;
using Domain.Contracts;
using Domain.Enums.Identity;
using Domain.Models.Identity;

namespace Application.Services.Identity;

public interface IAppUserService
{
    Task<IResult<IEnumerable<AppUserSlim>>> GetAllAsync();
    Task<IResult<IEnumerable<AppUserSlim>>> GetAllPaginatedAsync(int pageNumber, int pageSize);
    Task<IResult<int>> GetCountAsync();
    Task<IResult<AppUserSlim?>> GetByIdAsync(Guid userId);
    Task<IResult<AppUserFull?>> GetByIdFullAsync(Guid userId);
    Task<IResult<AppUserSecurityFull?>> GetByIdSecurityFullAsync(Guid userId);
    Task<IResult<AppUserSlim?>> GetByUsernameAsync(string username);
    Task<IResult<AppUserFull?>> GetByUsernameFullAsync(string username);
    Task<IResult<AppUserSecurityFull?>> GetByUsernameSecurityFullAsync(string username);
    Task<IResult<AppUserSlim?>> GetByEmailAsync(string email);
    Task<IResult<AppUserFull?>> GetByEmailFullAsync(string email);
    Task<IResult> UpdateAsync(AppUserUpdate updateObject, Guid modifyingUserId);
    Task<IResult> DeleteAsync(Guid userId, Guid modifyingUserId);
    Task<IResult<IEnumerable<AppUserSlim>>> SearchAsync(string searchText);
    Task<IResult<IEnumerable<AppUserSlim>>> SearchPaginatedAsync(string searchText, int pageNumber, int pageSize);
    Task<IResult<Guid>> CreateAsync(AppUserCreate createObject, Guid modifyingUserId);
    Task<IResult<Guid>> AddExtendedAttributeAsync(AppUserExtendedAttributeCreate addAttribute);
    Task<IResult> UpdateExtendedAttributeAsync(Guid attributeId, string? value, string? description);
    Task<IResult> RemoveExtendedAttributeAsync(Guid attributeId);
    Task<IResult> UpdatePreferences(Guid userId, AppUserPreferenceUpdate preferenceUpdate);
    Task<IResult<AppUserPreferenceFull?>> GetPreferences(Guid userId);
    Task<IResult<AppUserExtendedAttributeSlim?>> GetExtendedAttributeByIdAsync(Guid attributeId);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetUserExtendedAttributesByTypeAsync(Guid userId, ExtendedAttributeType type);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetUserExtendedAttributesByNameAsync(Guid userId, string name);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllUserExtendedAttributesAsync(Guid userId);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesByTypeAsync(ExtendedAttributeType type);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesByNameAsync(string name);
    Task<IResult<IEnumerable<AppUserExtendedAttributeSlim>>> GetAllExtendedAttributesAsync();
    Task<IResult<AppUserSecurityAttributeInfo?>> GetSecurityInfoAsync(Guid userId);
}