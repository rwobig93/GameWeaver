using Domain.Contracts;
using Domain.Models.ControlServer;

namespace Application.Responses.Host;

public class HostCheckInResponse : Result
{
    public List<WeaverWorkClient> Data { get; set; } = new();
}