﻿using System.Net;
using System.Net.Sockets;
using Application.Helpers.Runtime;
using Application.Models.GameServer.Network;
using Application.Services.GameServer;
using Domain.Contracts;
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
                var serverQuery = ServerQuery.GetServerInstance(EngineType.Source, check.HostIp, (ushort)check.PortQuery, 
                    sendTimeout: check.TimeoutMilliseconds, receiveTimeout: check.TimeoutMilliseconds, throwExceptions: true);

                var serverInfo = serverQuery.GetInfo();
                var serverPlayers = serverQuery.GetPlayers();

                _logger.Debug("Server Info: {OS} > {Name}/{Map}/{Version} [Players]({Players}/{MaxPlayers})",
                    serverInfo.Environment, serverInfo.Name, serverInfo.Map, serverInfo.GameVersion, serverPlayers.Count, serverInfo.MaxPlayers);

                return await Result<bool>.SuccessAsync(true);
            }

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

    public async Task<IResult<bool>> IsPortOpenAsync(string ipOrHost, int port, NetworkProtocol protocol, int timeoutMilliseconds)
    {
        try
        {
            var parsedIp = await NetworkHelpers.ParseIpFromIpOrHost(ipOrHost);
            if (parsedIp is null)
            {
                return await Result<bool>.FailAsync(false, "Failed to parse provided IP or Hostname into a usable IP Address");
            }
            
            switch (protocol)
            {
                case NetworkProtocol.Tcp:
                {
                    using var tcpClient = new TcpClient();
                    tcpClient.Client.SendTimeout = timeoutMilliseconds;
                    tcpClient.Client.ReceiveTimeout = timeoutMilliseconds;
                    var connectTask = tcpClient.BeginConnect(IPAddress.Parse(parsedIp), port, null, null);
                    var connectResponseReceived = connectTask.AsyncWaitHandle.WaitOne(timeoutMilliseconds);
                    tcpClient.Close();

                    return await Result<bool>.SuccessAsync(connectResponseReceived);
                }
                case NetworkProtocol.Udp:
                {
                    using var udpClient = new UdpClient();
                    udpClient.Client.SendTimeout = timeoutMilliseconds;
                    udpClient.Client.ReceiveTimeout = timeoutMilliseconds;
                    udpClient.Connect(parsedIp, port);
                    await udpClient.SendAsync(new byte[69]);
                    var receiveTask = udpClient.BeginReceive(null, null);
                    var responseReceived = receiveTask.AsyncWaitHandle.WaitOne(timeoutMilliseconds);
                    udpClient.Close();
                    
                    return await Result<bool>.SuccessAsync(responseReceived);
                }
                default:
                    return await Result<bool>.FailAsync(false, $"Invalid protocol type provided: {nameof(protocol)} => {protocol}");
            }
        }
        catch
        {
            return await Result<bool>.FailAsync(false);
        }
    }
}