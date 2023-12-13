using Application.Requests.Host;
using Application.Responses.Host;
using Domain.Contracts;

namespace Application.Services;

public interface IControlServerService
{
    public bool ServerIsUp { get; }
    public HostAuthResponse ActiveToken { get; }
    
    Task<bool> CheckIfServerIsUp();
    Task<IResult<HostRegisterResponse>> RegistrationConfirm(HostRegisterRequest request);
    Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request);
    Task<IResult> Checkin(HostCheckInRequest request);
}