﻿using Application.Requests.Host;
using Application.Responses.Host;
using Domain.Contracts;
using Domain.Models.Host;

namespace Application.Services;

public interface IControlServerService
{
    public bool ServerIsUp { get; }
    public bool RegisteredWithServer { get; }
    public HostAuthorization Authorization { get; }
    
    Task<bool> CheckIfServerIsUp();
    Task<IResult<HostRegisterResponse>> RegistrationConfirm();
    Task<IResult<HostAuthResponse>> GetToken();
    Task<IResult> EnsureAuthTokenIsUpdated();
    Task<IResult> Checkin(HostCheckInRequest request);
}