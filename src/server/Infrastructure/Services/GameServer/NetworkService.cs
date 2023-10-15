using System.Net;
using System.Net.Sockets;
using Application.Models.GameServer.Network;
using Application.Models.Web;
using Application.Services.GameServer;
using Domain.Enums.GameServer;
using QueryMaster;
using QueryMaster.GameServer;

namespace Infrastructure.Services.GameServer;

public class NetworkService : INetworkService
{
    private readonly ILogger _logger;

    public NetworkService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<IResult<bool>> IsGameServerConnectableAsync(GameServerConnectivityCheck check)
    {
        var hostSocket = $"{check.HostIp}:{check.PortQuery}";

        try
        {
            check.TimeoutMilliseconds = check.TimeoutMilliseconds switch
            {
                < 500 => 500,
                > 10000 => 10000,
                _ => check.TimeoutMilliseconds
            };

            if (check.Source == GameSource.Steam)
            {
                // TODO: Figure out path forward, SteamQueryNet doesn't have native timeout and doesn't seem to get server data, QueryMaster gets packet header failure
                var serverQuery = ServerQuery.GetServerInstance(EngineType.Source, check.HostIp, (ushort)check.PortQuery, 
                    sendTimeout: check.TimeoutMilliseconds, receiveTimeout: check.TimeoutMilliseconds, throwExceptions: true);

                var serverInfo = serverQuery.GetInfo();
                var serverPlayers = serverQuery.GetPlayers();

                _logger.Debug("Server Info: {OS} > {Name}/{Map}/{Version} [Players]({Players}/{MaxPlayers})",
                    serverInfo.Environment, serverInfo.Name, serverInfo.Map, serverInfo.GameVersion, serverPlayers.Count, serverInfo.MaxPlayers);

                return await Result<bool>.SuccessAsync(true);
            }

            // TODO: Port checking works for both UDP & TCP, since steam queries varies per game we should be getting server info from the
            //       server configuration itself, define modular way to do so based on configuration items
            var portOpenCheck = await IsPortOpenAsync(check.HostIp, check.PortGame, check.Protocol, check.TimeoutMilliseconds);
            if (portOpenCheck.Data)
                return await Result<bool>.SuccessAsync(true);

            return await Result<bool>.SuccessAsync(false);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error occured attempting to check GameServer connectivity: {ErrorMessage}", ex.Message);
            return await Result<bool>.FailAsync(false, $"GameServer is not connectable: {hostSocket}");
        }
    }

    public async Task<IResult<bool>> IsPortOpenAsync(string ipAddress, int port, NetworkProtocol protocol, int timeoutMilliseconds)
    {
        try
        {
            switch (protocol)
            {
                case NetworkProtocol.Tcp:
                {
                    using var client = new TcpClient();
                    client.Client.SendTimeout = timeoutMilliseconds;
                    client.Client.ReceiveTimeout = timeoutMilliseconds;
                    // TODO: Resolve host if hostname or loopback is provided instead
                    var connectTask = client.BeginConnect(IPAddress.Parse(ipAddress), port, null, null);
                    var connectResponseReceived = connectTask.AsyncWaitHandle.WaitOne(timeoutMilliseconds);
                    client.Close();

                    return await Result<bool>.SuccessAsync(connectResponseReceived);
                }
                case NetworkProtocol.Udp:
                {
                    using var client = new UdpClient();
                    client.Client.SendTimeout = timeoutMilliseconds;
                    client.Client.ReceiveTimeout = timeoutMilliseconds;
                    client.Connect(ipAddress, port);
                    await client.SendAsync(new byte[69]);
                    var receiveTask = client.BeginReceive(null, null);
                    var responseReceived = receiveTask.AsyncWaitHandle.WaitOne(timeoutMilliseconds);
                    client.Close();
                    
                    return await Result<bool>.SuccessAsync(responseReceived);
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(protocol), protocol, null);
            }
        }
        catch
        {
            return await Result<bool>.FailAsync(false);
        }
    }
}