﻿using Application.Models.External.Steam;
using Domain.Contracts;

namespace Application.Services.External;

public interface ISteamApiService
{
    Task<IResult<SteamAppList>> GetAllApps();
    Task<IResult<SteamAppInfo?>> GetCurrentAppBuild(int appId);
}