using System.Net;
using System.Net.Sockets;
using GameServerQuery.Client.Constants;
using GameServerQuery.Client.Interfaces;
using GameServerQuery.Client.Models;

namespace GameServerQuery.Client;

public class ServerClientBase : IServerClient
{
    /// <inheritdoc/>
    public int SendTimeout { get; private set; } = 3000;

    /// <inheritdoc/>
    public int ReceiveTimeout { get; private set; } = 3000;

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public int BufferSize { get; private set; }

    /// <inheritdoc/>
    public ProtocolType Protocol { get; private set; } = ProtocolType.Udp;

    private Socket? _socket;

    /// <inheritdoc/>
    public async Task<ConnectionResult> Connect(IPEndPoint endpoint, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000)
    {
        SendTimeout = sendTimeout;
        ReceiveTimeout = receiveTimeout;

        switch (type)
        {
            case ProtocolType.Tcp:
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Protocol = ProtocolType.Tcp;
                BufferSize = ClientConstants.DefaultTcpBufferSize;
                break;
            case ProtocolType.Udp:
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                Protocol = ProtocolType.Udp;
                BufferSize = ClientConstants.DefaultUdpBufferSize;
                break;
            case ProtocolType.Unknown:
            case ProtocolType.IP:
            case ProtocolType.Icmp:
            case ProtocolType.Igmp:
            case ProtocolType.Ggp:
            case ProtocolType.IPv4:
            case ProtocolType.Pup:
            case ProtocolType.Idp:
            case ProtocolType.IPv6:
            case ProtocolType.IPv6RoutingHeader:
            case ProtocolType.IPv6FragmentHeader:
            case ProtocolType.IPSecEncapsulatingSecurityPayload:
            case ProtocolType.IPSecAuthenticationHeader:
            case ProtocolType.IcmpV6:
            case ProtocolType.IPv6NoNextHeader:
            case ProtocolType.IPv6DestinationOptions:
            case ProtocolType.ND:
            case ProtocolType.Raw:
            case ProtocolType.Ipx:
            case ProtocolType.Spx:
            case ProtocolType.SpxII:
            default: return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Only TCP and UDP protocols are supported" });
        }

        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, SendTimeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, ReceiveTimeout);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, BufferSize);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, BufferSize);

        await _socket.ConnectAsync(endpoint);
        if (!_socket.Connected)
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Failed to connect to the server" });
        }

        IsConnected = true;
        return await Task.FromResult(new ConnectionResult { Succeeded = true, Message = "Successfully connected to the server" });
    }

    /// <inheritdoc/>
    public async Task<ConnectionResult> Connect(string hostOrIp, int port, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000)
    {
        if (string.IsNullOrEmpty(hostOrIp))
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Provided Hostname or IP address is empty or invalid" });
        }
        if (hostOrIp.Length > 255)
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Provided Hostname is over 255 characters which is the hostname limit" });
        }

        if (IPAddress.TryParse(hostOrIp, out var address))
        {
            return await Connect(new IPEndPoint(address, port), type, sendTimeout, receiveTimeout);
        }

        try
        {
            var record = Dns.GetHostAddresses(hostOrIp);
            if (record.Length == 0)
            {
                return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Couldn't resolve a valid record for the provided hostname" });
            }

            return await Connect(new IPEndPoint(record.First(), port), type, sendTimeout, receiveTimeout);
        }
        catch (SocketException)
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Couldn't resolve a valid record for the provided hostname" });
        }
        catch (ArgumentException)
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = "Provided Hostname or IP address is invalid" });
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new ConnectionResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" });
        }
    }

    /// <inheritdoc/>
    public void CloseConnection()
    {
        _socket?.Close();
        IsConnected = false;
    }

    /// <inheritdoc/>
    public async Task<ClientSendResult> SendAsync(byte[] payload)
    {
        if (_socket is null || !_socket.Connected)
        {
            return await Task.FromResult(new ClientSendResult { Succeeded = false, Message = "The socket is currently closed" });
        }

        try
        {
            var socketSend = _socket.BeginSend(payload, 0, payload.Length, SocketFlags.None, null, null);
            socketSend.AsyncWaitHandle.WaitOne(SendTimeout);
            if (!socketSend.IsCompleted)
            {
                return await Task.FromResult(new ClientSendResult { Succeeded = false, Message = "Timeout occurred while waiting to send data" });
            }

            var bytesSent = _socket.EndSend(socketSend);
            return await Task.FromResult(new ClientSendResult {Succeeded = true, BytesSent = bytesSent});
        }
        catch (SocketException ex)
        {
            return await Task.FromResult(new ClientSendResult {Succeeded = false, Message = ex.Message});
        }
        catch (ObjectDisposedException)
        {
            return await Task.FromResult(new ClientSendResult {Succeeded = false, Message = "Socket connection was closed unexpectedly"});
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new ClientSendResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" });
        }
    }

    /// <inheritdoc/>
    public async Task<ClientReceiveResult> ReceiveAsync()
    {
        if (_socket is null || !_socket.Connected)
        {
            return await Task.FromResult(new ClientReceiveResult { Succeeded = false, Message = "The socket is currently closed" });
        }

        var databuffer = new byte[BufferSize];
        try
        {
            var socketReceive = _socket.BeginReceive(databuffer, 0, databuffer.Length, SocketFlags.None, null, null);
            socketReceive.AsyncWaitHandle.WaitOne(ReceiveTimeout);
            if (!socketReceive.IsCompleted)
            {
                return await Task.FromResult(new ClientReceiveResult { Succeeded = false, Message = "Timeout occurred while waiting to receive data" });
            }

            _socket.EndReceive(socketReceive);
            return await Task.FromResult(new ClientReceiveResult {Succeeded = true, Response = databuffer});
        }
        catch (SocketException ex)
        {
            return await Task.FromResult(new ClientReceiveResult {Succeeded = false, Message = ex.Message});
        }
        catch (ObjectDisposedException)
        {
            return await Task.FromResult(new ClientReceiveResult {Succeeded = false, Message = "Connection was closed unexpectedly"});
        }
        catch (Exception ex)
        {
            return await Task.FromResult(new ClientReceiveResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" });
        }
    }

    public void Dispose()
    {
        CloseConnection();
    }
}