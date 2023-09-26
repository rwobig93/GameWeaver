using Domain.DatabaseEntities.Identity;
using Microsoft.AspNetCore.Identity;

namespace Application.Services.Identity;

public interface IAppIdentityRoleService : IRoleStore<AppRoleDb>
{
    
}