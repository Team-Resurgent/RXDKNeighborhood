using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RXDKTestRig
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

        public event EventHandler<LineReceivedEventArgs>? LineReceived;

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
            var endpoint = client.Client.RemoteEndPoint?.ToString();
            var connectMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {endpoint}] Connected.";
            Debug.Print(connectMessage);

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

                        string receivedData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                       
                        // Append new data to the line buffer
                        lineBuffer.Append(receivedData);

                        // Process complete lines ending with CRLF
                        ProcessCompleteLines(lineBuffer, endpoint);

                        // Echo back the raw data
                        await stream.WriteAsync(buffer.AsMemory(0, bytesRead), token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Server stopping
                }
                catch (Exception ex)
                {
                    var errorMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {endpoint}] Error: {ex.Message}";
                    Debug.Print(errorMessage);
                }
                finally
                {
                    var disconnectMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {endpoint}] Disconnected.";
                    Debug.Print(disconnectMessage);
                }
            }
        }

        private void ProcessCompleteLines(StringBuilder lineBuffer, string? endpoint)
        {
            string bufferContent = lineBuffer.ToString();
            int crlfIndex;

            while ((crlfIndex = bufferContent.IndexOf("\r\n")) >= 0)
            {
                // Extract the complete line (without CRLF)
                string completeLine = bufferContent.Substring(0, crlfIndex);
                
                // Remove the processed line and CRLF from buffer
                bufferContent = bufferContent.Substring(crlfIndex + 2);
                
                // Process the complete line
                ProcessLine(completeLine, endpoint);
            }

            // Update the buffer with remaining partial data
            lineBuffer.Clear();
            lineBuffer.Append(bufferContent);
        }

        private void ProcessLine(string line, string? endpoint)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            // Parse the line: first word is type, rest is message
            string[] parts = line.Split(new char[] { ' ', '\t' }, 2, StringSplitOptions.RemoveEmptyEntries);
            
            string messageType = parts.Length > 0 ? parts[0] : "";
            string message = parts.Length > 1 ? parts[1] : "";

            // Raise the LineReceived event
            try
            {
                LineReceived?.Invoke(this, new LineReceivedEventArgs(endpoint ?? "unknown", messageType, message, line));
            }
            catch (Exception ex)
            {
                Debug.Print($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Client {endpoint}] Error in LineReceived event handler: {ex.Message}");
            }
        }

        public async Task StopAsync()
        {
            if (_cts == null)
                return;

            var stoppingMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Stopping...";
            Debug.Print(stoppingMessage);
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

            var fullyStoppedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Server] Fully stopped.";
            Debug.Print(fullyStoppedMessage);
        }
    }
}
