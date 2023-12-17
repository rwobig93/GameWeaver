using System.Text.Json.Serialization;
using Domain.Contracts;
using Domain.Models.Host;

namespace Application.Responses.Host;

public class HostRegisterResponse : Result
{
    public HostAuthentication Data { get; set; } = null!;
}