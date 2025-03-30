using System.Net;
using System.Net.Sockets;
using GameServerQuery.Client.Constants;
using GameServerQuery.Client.Interfaces;
using GameServerQuery.Client.Models;

namespace GameServerQuery.Client;

public class ServerClientBase : IServerClient, IDisposable
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

    private Socket? Socket { get; set; }

    /// <inheritdoc/>
    public async Task<ConnectionResult> Connect(IPEndPoint endpoint, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000)
    {
        switch (type)
        {
            case ProtocolType.Tcp:
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Protocol = ProtocolType.Tcp;
                BufferSize = ClientConstants.DefaultTcpBufferSize;
                break;
            case ProtocolType.Udp:
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
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
            default: return new ConnectionResult { Succeeded = false, Message = "Only TCP and UDP protocols are supported" };
        }
        
        SendTimeout = sendTimeout;
        Socket.SendTimeout = sendTimeout;
        ReceiveTimeout = receiveTimeout;
        Socket.ReceiveTimeout = receiveTimeout;

        var socketConnectionResult = Socket.BeginConnect(endpoint, null, null);
        await Task.Yield();

        if (!socketConnectionResult.AsyncWaitHandle.WaitOne(ReceiveTimeout, true))
        {
            return new ConnectionResult { Succeeded = false, Message = "A timeout occurred while connecting to the server" };
        }
        IsConnected = true;
        
        return new ConnectionResult { Succeeded = true, Message = "Successfully connected to the server" };
    }

    /// <inheritdoc/>
    public async Task<ConnectionResult> Connect(string hostOrIp, int port, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000)
    {
        if (string.IsNullOrEmpty(hostOrIp))
        {
            return new ConnectionResult { Succeeded = false, Message = "Provided Hostname or IP address is empty or invalid" };
        }
        if (hostOrIp.Length > 255)
        {
            return new ConnectionResult { Succeeded = false, Message = "Provided Hostname is over 255 characters which is the hostname limit" };
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
                return new ConnectionResult { Succeeded = false, Message = "Couldn't resolve a valid record for the provided hostname" };
            }

            return await Connect(new IPEndPoint(record.First(), port), type, sendTimeout, receiveTimeout);
        }
        catch (SocketException)
        {
            return new ConnectionResult { Succeeded = false, Message = "Couldn't resolve a valid record for the provided hostname" };
        }
        catch (ArgumentException)
        {
            return new ConnectionResult { Succeeded = false, Message = "Provided Hostname or IP address is invalid" };
        }
        catch (Exception ex)
        {
            return new ConnectionResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" };
        }
    }

    /// <inheritdoc/>
    public void CloseConnection()
    {
        Socket?.Close();
    }

    /// <inheritdoc/>
    public async Task<ClientSendResult> SendAsync(byte[] payload)
    {
        if (Socket is null)
        {
            return new ClientSendResult {Succeeded = false, Message = "Current socket connection is closed"};
        }

        try
        {
            var response = await Socket.SendAsync(payload, SocketFlags.None);
            return new ClientSendResult {Succeeded = true, BytesSent = response};
        }
        catch (SocketException ex)
        {
            return new ClientSendResult {Succeeded = false, Message = ex.Message};
        }
        catch (ObjectDisposedException)
        {
            return new ClientSendResult {Succeeded = false, Message = "Current socket connection is closed"};
        }
        catch (Exception ex)
        {
            return new ClientSendResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" };
        }
    }

    /// <inheritdoc/>
    public async Task<ClientReceiveResult> ReceiveAsync()
    {
        if (Socket is null)
        {
            return new ClientReceiveResult {Succeeded = false, Message = "Current socket connection is closed"};
        }

        var databuffer = new byte[BufferSize];
        try
        {
            var response = await Socket.ReceiveAsync(databuffer, SocketFlags.None);
            return new ClientReceiveResult {Succeeded = true, Response = databuffer.Take(response).ToArray()};
        }
        catch (SocketException ex)
        {
            return new ClientReceiveResult {Succeeded = false, Message = ex.Message};
        }
        catch (ObjectDisposedException)
        {
            return new ClientReceiveResult {Succeeded = false, Message = "Current socket connection is closed"};
        }
        catch (Exception ex)
        {
            return new ClientReceiveResult { Succeeded = false, Message = $"An unexpected error occured: {ex.Message}" };
        }
    }

    public void Dispose()
    {
        CloseConnection();
    }
}