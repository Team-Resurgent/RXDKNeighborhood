using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RXDKTestRig
{
    public class EchoServer
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _serverTask;

        public EchoServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            if (_serverTask != null)
                throw new InvalidOperationException("Server already running.");

            _cts = new CancellationTokenSource();
            _serverTask = Task.Run(() => RunServerLoopAsync(_cts.Token));
        }

        private async Task RunServerLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    _listener = new TcpListener(IPAddress.Any, _port);
                    _listener.Start();
                    Debug.Print($"[Server] Listening on port {_port}...");

                    while (!token.IsCancellationRequested)
                    {
                        var client = await _listener.AcceptTcpClientAsync(token);
                        _ = HandleClientAsync(client, token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Graceful shutdown
                }
                catch (SocketException ex)
                {
                    Debug.Print($"[Server] Socket error: {ex.Message}, retrying in 3s...");
                    await Task.Delay(3000, token);
                }
                catch (Exception ex)
                {
                    Debug.Print($"[Server] Unexpected error: {ex.Message}, retrying in 3s...");
                    await Task.Delay(3000, token);
                }
                finally
                {
                    try
                    {
                        _listener?.Stop();
                    }
                    catch { }
                }
            }

            Debug.Print("[Server] Stopped.");
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            var endpoint = client.Client.RemoteEndPoint?.ToString();
            Debug.Print($"[Client {endpoint}] Connected.");

            using (client)
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                        if (bytesRead == 0)
                            break;

                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Debug.Print($"[Client {endpoint}] Received: {message.Trim()}");

                        await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Server stopping
                }
                catch (Exception ex)
                {
                    Debug.Print($"[Client {endpoint}] Error: {ex.Message}");
                }
                finally
                {
                    Debug.Print($"[Client {endpoint}] Disconnected.");
                }
            }
        }

        public async Task StopAsync()
        {
            if (_cts == null)
                return;

            Debug.Print("[Server] Stopping...");
            _cts.Cancel();

            try
            {
                if (_serverTask != null)
                    await _serverTask;
            }
            catch (OperationCanceledException) { }

            _listener?.Stop();
            _cts.Dispose();
            _cts = null;
            _serverTask = null;

            Debug.Print("[Server] Fully stopped.");
        }
    }
}
