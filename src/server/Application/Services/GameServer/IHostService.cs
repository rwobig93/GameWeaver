using Application.Models.Web;
using Application.Requests.Hosts;
using Application.Responses.Hosts;

namespace Application.Services.GameServer;

public interface IHostService
{
    Task<IResult<HostRegisterResponse>> Register(HostRegisterRequest request);
    Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request);
}