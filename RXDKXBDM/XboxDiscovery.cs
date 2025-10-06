using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using RXDKXBDM.Models;
using RXDKXBDM.Commands;
using System.Reflection.PortableExecutable;

namespace RXDKNeighborhood.Helpers
{
    public static class XboxDiscovery
    {
        public static IEnumerable<XboxItem> Discover()
        {
            var buffer = new byte[1024];
            var connections = new List<XboxItem>();

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces().Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.Supports(NetworkInterfaceComponent.IPv4));
            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties().UnicastAddresses.Where(unicast => unicast.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(unicast.Address));
                foreach (var ipProperty in ipProperties)
                {
                    try
                    {
                        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                        socket.EnableBroadcast = true;
                        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        socket.Bind(new IPEndPoint(ipProperty.Address, 0));
                        socket.ReceiveTimeout = 100;
                        socket.SendTo([0x03, 0x00], new IPEndPoint(IPAddress.Broadcast, 731));

                        var timer = Stopwatch.StartNew();
                        while (timer.ElapsedMilliseconds < 500)
                        {
                            try
                            {
                                var remoteEndpoint = new IPEndPoint(IPAddress.Any, 0) as EndPoint;
                                var received = socket.ReceiveFrom(buffer, ref remoteEndpoint);
                                if (received >= 2)
                                {
                                    var nameLength = buffer[1];
                                    if (buffer[0] == 2 && nameLength + 2 == received)
                                    {
                                        var ipAddress = ((IPEndPoint)remoteEndpoint).Address.ToString();
                                        if (!connections.Any(x => x.IpAddress.Equals(ipAddress)))
                                        {
                                            var name = Encoding.ASCII.GetString(buffer, 2, nameLength);
                                            connections.Add(new XboxItem(name, ipAddress));
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // do nothing
                            }
                        }
                    }
                    catch
                    {
                        // do nothing
                    }

                }
            }
            return connections;
        }
    }
}
