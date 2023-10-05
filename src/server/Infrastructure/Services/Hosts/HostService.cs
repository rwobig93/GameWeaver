using Application.Models.Web;
using Application.Requests.Hosts;
using Application.Responses.Hosts;
using Application.Services.Hosts;

namespace Infrastructure.Services.Hosts;

public class HostService : IHostService
{
    public async Task<IResult<HostRegisterResponse>> Register(HostRegisterRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }

    public async Task<IResult<HostAuthResponse>> GetToken(HostAuthRequest request)
    {
        await Task.CompletedTask;
        throw new NotImplementedException();
    }
}