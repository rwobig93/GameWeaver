using System.Net;
using System.Net.Sockets;
using GameServerQuery.Client.Models;

namespace GameServerQuery.Client.Interfaces;

public interface IServerClient : IDisposable
{
    /// <summary>
    /// Time in milliseconds to allow a send request to take before a timeout occurs
    /// </summary>
    public int SendTimeout { get; }
    
    /// <summary>
    /// Time in milliseconds to allow a receiving request to take before a timeout occurs
    /// </summary>
    public int ReceiveTimeout { get; }
    
    /// <summary>
    /// Whether the client is currently connected or not
    /// </summary>
    public bool IsConnected { get; }
    
    /// <summary>
    /// Currently buffer size
    /// </summary>
    public int BufferSize { get; }

    /// <summary>
    /// Protocol used for the current client connection
    /// </summary>
    public ProtocolType Protocol { get; }
    
    /// <summary>
    /// Create a new connection to a remote endpoint
    /// </summary>
    /// <param name="endpoint">Remote endpoint to connect to</param>
    /// <param name="type">Protocol to use when connecting</param>
    /// <param name="sendTimeout">Time in milliseconds to allow a send request to take before a timeout occurs, defaults to 3 seconds (3000ms)</param>
    /// <param name="receiveTimeout">Time in milliseconds to allow a receiving request to take before a timeout occurs, defaults to 3 seconds (3000ms)</param>
    /// <returns>Connection result based on the outcome of connecting</returns>
    public Task<ConnectionResult> Connect(IPEndPoint endpoint, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000);
    
    /// <summary>
    /// Create a new connection to a remote endpoint via hostname or ip address and port
    /// </summary>
    /// <param name="hostOrIp">Hostname or IP Address of the endpoint to connect to</param>
    /// <param name="port">Port of the endpoint to connect to</param>
    /// <param name="type">Protocol to use when connecting</param>
    /// <param name="sendTimeout">Time in milliseconds to allow a send request to take before a timeout occurs, defaults to 3 seconds (3000ms)</param>
    /// <param name="receiveTimeout">Time in milliseconds to allow a receiving request to take before a timeout occurs, defaults to 3 seconds (3000ms)</param>
    /// <returns>Connection result based on the outcome of connecting</returns>
    public Task<ConnectionResult> Connect(string hostOrIp, int port, ProtocolType type, int sendTimeout = 3000, int receiveTimeout = 3000);
    
    /// <summary>
    /// Close the current client connection
    /// </summary>
    public void CloseConnection();
    
    /// <summary>
    /// Send a payload to the connected remote endpoint
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public Task<ClientSendResult> SendAsync(byte[] payload);
    
    /// <summary>
    /// Receive a payload from the connected remote endpoint
    /// </summary>
    /// <returns></returns>
    public Task<ClientReceiveResult> ReceiveAsync();
}