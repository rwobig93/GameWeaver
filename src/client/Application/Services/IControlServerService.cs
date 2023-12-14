using Application.Requests.Host;
using Application.Responses.Host;
using Domain.Contracts;

namespace Application.Services;

public interface IControlServerService
{
    public bool ServerIsUp { get; }
    public HostAuthResponse ActiveToken { get; }
    
    Task<bool> CheckIfServerIsUp();
    Task<IResult<HostRegisterResponse>> RegistrationConfirm();
    Task<IResult<HostAuthResponse>> GetToken();
    Task<IResult> Checkin(HostCheckInRequest request);
}