using System.Net.Sockets;

namespace Domain.Models.Network;

public class NetworkListeningSocket
{
    public ProtocolType Protocol { get; set; } = ProtocolType.Tcp;
    public int Port { get; set; }
}