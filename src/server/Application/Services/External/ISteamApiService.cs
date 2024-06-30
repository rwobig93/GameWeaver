using Application.Models.External.Steam;
using Domain.Contracts;

namespace Application.Services.External;

public interface ISteamApiService
{
    Task<IResult<IEnumerable<SteamApiAppResponseJson>>> GetAllApps();
    Task<IResult<SteamAppInfo?>> GetCurrentAppBuild(int appId);
    Task<IResult<SteamAppDetailResponseJson?>> GetAppDetail(int appId);
}