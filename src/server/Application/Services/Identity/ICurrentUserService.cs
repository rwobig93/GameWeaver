using System.Security.Claims;
using Application.Models.Identity.User;
using Application.Responses.v1.Identity;
using Domain.DatabaseEntities.Identity;

namespace Application.Services.Identity;

public interface ICurrentUserService
{
    Task<ClaimsPrincipal?> GetCurrentUserPrincipal();
    Task<Guid?> GetCurrentUserId();
    Guid GetIdFromPrincipal(ClaimsPrincipal principal);
    public Task<UserBasicResponse?> GetCurrentUserBasic();
    public Task<AppUserFull?> GetCurrentUserFull();
    public Task<AppUserSecurityFull?> GetCurrentUserSecurityFull();
    public Task<AppUserDb?> GetCurrentUserDb();
}