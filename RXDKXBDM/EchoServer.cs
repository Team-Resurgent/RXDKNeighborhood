using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;

namespace RXDKXBDM
{
    public class LineReceivedEventArgs : EventArgs
    {
        public string ClientEndpoint { get; }
        public string MessageType { get; }
        public string Message { get; }
        public string FullLine { get; }

        public LineReceivedEventArgs(string clientEndpoint, string messageType, string message, string fullLine)
        {
            ClientEndpoint = clientEndpoint;
            MessageType = messageType;
            Message = message;
            FullLine = fullLine;
        }
    }

    public class EchoServer
    {
        private readonly int _port;
        private TcpListener? _listener;
        private CancellationTokenSource? _cts;
        private Task? _serverTask;
        private int? _filterClientPort = null; // Filter by specific client port

        public event EventHandler<LineReceivedEventArgs>? LineReceived;

        public void SetClientPortFilter(int clientPort)
        {
            _filterClientPort = clientPort;
            Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FILTER] Set client port filter to: {clientPort}");
        }

        public void ClearClientPortFilter()
        {
            _filterClientPort = null;
            Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FILTER] Cleared client port filter - accepting all connections");
        }

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
                    var startMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Listening on all interfaces (0.0.0.0) port {_port}...";
                    Debug.Print(startMessage);

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
                    var socketErrorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Socket error: {ex.Message}, retrying in 3s...";
                    Debug.Print(socketErrorMessage);
                    await Task.Delay(3000, token);
                }
                catch (Exception ex)
                {
                    var unexpectedErrorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Unexpected error: {ex.Message}, retrying in 3s...";
                    Debug.Print(unexpectedErrorMessage);
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

            var stoppedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Stopped.";
            Debug.Print(stoppedMessage);
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            var remoteEndpoint = client.Client.RemoteEndPoint?.ToString();
            var localEndpoint = client.Client.LocalEndPoint?.ToString();
            var connectMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {remoteEndpoint} -> Local {localEndpoint}] Connected.";
            Debug.Print(connectMessage);

            // Auto-set filter to first connection's client port if enabled
            if (remoteEndpoint != null && remoteEndpoint.Contains(':'))
            {
                string portString = remoteEndpoint.Substring(remoteEndpoint.LastIndexOf(':') + 1);
                if (int.TryParse(portString, out int clientPort))
                {
                    SetClientPortFilter(clientPort);
                }
            }

            using (client)
            {
                var stream = client.GetStream();
                var buffer = new byte[1024];
                var lineBuffer = new StringBuilder();

                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), token);
                        if (bytesRead == 0)
                        {
                            break;
                        }

                        var receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        lineBuffer.Append(receivedData);

                        ProcessCompleteLines(lineBuffer, remoteEndpoint, localEndpoint);
                    }
                }
                catch (OperationCanceledException)
                {
                    await stream.WriteAsync(Encoding.UTF8.GetBytes("bye\r\n"));
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {remoteEndpoint}] Error: {ex.Message}";
                    Debug.Print(errorMessage);
                }
                finally
                {
                    var disconnectMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {remoteEndpoint}] Disconnected.";
                    Debug.Print(disconnectMessage);
                }
            }
        }

        private void ProcessCompleteLines(StringBuilder lineBuffer, string? remoteEndpoint, string? localEndpoint)
        {
            string bufferContent = lineBuffer.ToString();
            int crlfIndex;

            while ((crlfIndex = bufferContent.IndexOf("\r\n")) >= 0)
            {
                string completeLine = bufferContent.Substring(0, crlfIndex);
                bufferContent = bufferContent.Substring(crlfIndex + 2);
                ProcessLine(completeLine, remoteEndpoint, localEndpoint);
            }

            lineBuffer.Clear();
            lineBuffer.Append(bufferContent);
        }

        private void ProcessLine(string line, string? remoteEndpoint, string? localEndpoint)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // Extract client port from remote endpoint (e.g. "192.168.1.93:19436" -> 19436)
            int clientPort = 0;
            if (remoteEndpoint != null && remoteEndpoint.Contains(':'))
            {
                string portString = remoteEndpoint.Substring(remoteEndpoint.LastIndexOf(':') + 1);
                int.TryParse(portString, out clientPort);
            }

            // Check if we should filter by client port
            var shouldProcess = _filterClientPort == null || clientPort == _filterClientPort;
            if (shouldProcess)
            {
                //Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [MATCH] [Client Port {clientPort}] [Client {remoteEndpoint}] Line: {line}");

                // Parse the line: first word is type, rest is message
                string[] parts = line.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);

                string messageType = parts.Length > 0 ? parts[0] : "";
                string message = parts.Length > 1 ? parts[1] : "";

                // Raise the LineReceived event
                try
                {
                    LineReceived?.Invoke(this, new LineReceivedEventArgs(remoteEndpoint ?? "unknown", messageType.ToLower(), message, line));
                }
                catch (Exception ex)
                {
                    Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [MATCH] [Client {remoteEndpoint}] Error in LineReceived event handler: {ex.Message}");
                }
            }
            else
            {
                //Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FILTERED] Client Port {clientPort} (Filter: {_filterClientPort}) [Client {remoteEndpoint}] Line: {line}");
            }
        }

        public async Task StopAsync()
        {
            if (_cts == null)
            {
                return;
            }

            var stoppingMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Stopping...";
            Debug.Print(stoppingMessage);
            _cts.Cancel();

            try
            {
                if (_serverTask != null)
                {
                    await _serverTask;
                }
            }
            catch (OperationCanceledException) 
            {
                
            }

            _listener?.Stop();
            _cts.Dispose();
            _cts = null;
            _serverTask = null;

            var fullyStoppedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Fully stopped.";
            Debug.Print(fullyStoppedMessage);
        }
    }
}
