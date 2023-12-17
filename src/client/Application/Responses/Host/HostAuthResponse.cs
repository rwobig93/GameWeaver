using Domain.Contracts;
using Domain.Models.Host;

namespace Application.Responses.Host;

public class HostAuthResponse : Result
{
    public HostAuthorization Data { get; set; } = null!;
}