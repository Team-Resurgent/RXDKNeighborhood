using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using RXDKXBDM.Commands;
using System.Runtime.InteropServices;

namespace RXDKXBDM
{
    

    public class Connection : IDisposable
    {
        private Socket? mClient = null;
        private string mAddress = string.Empty;

        private bool disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                Close();
            }
            disposed = true;
        }

        private bool WaitAvailable()
        {
            if (mClient == null)
            {
                return false;
            }

            int count = 0;
            while (mClient.Available == 0)
            {
                if (count == 5)
                {
                    return false;
                }
                Thread.Sleep(1000);
                count++;

            }

            return true;
        }

        private string ExtractLine(ref byte[] buffer, int bufferLen, ref int position)
        {
            var stringBuilder = new StringBuilder();
            while (position < bufferLen) 
            {
                var currentChar = (char)buffer[position];
                position++;
                if (currentChar == '\r')
                {
                    continue;
                }
                if (currentChar == '\n')
                {
                    break;
                }
                stringBuilder.Append(currentChar);
            }
            return stringBuilder.ToString();
        }

        public async Task<SocketResponse> TryRecieveBodyAsync(CancellationToken? cancellationToken = null, ExpectedSizeStream ? binaryResponseStream = null)
        {
            if (mClient == null)
            {
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Client Not Open" };
            }
            try
            {
                if (WaitAvailable() == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Timeout" };
                }

                var initialReadBuffer = new byte[16384];
                var initialBytesRead = await mClient.ReceiveAsync(initialReadBuffer);
                if (initialBytesRead < 5)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected Result" };
                }

                var initialPosition = 0;
                var header = ExtractLine(ref initialReadBuffer, initialBytesRead, ref initialPosition);
                if (header.Substring(3, 2).Equals("- ") == false || int.TryParse(header.AsSpan(0, 3), out var responseCodeInt) == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected Result" };
                }

                var responseCode = (ResponseCode)responseCodeInt;
                var response = header.Substring(5);
                var socketResponse = new SocketResponse { ResponseCode = (ResponseCode)responseCode, Response = response };
                if (responseCode == ResponseCode.SUCCESS_OK || responseCode == ResponseCode.SUCCESS_CONNECTED)
                {
                    return socketResponse;
                }

                var readBuffer = new byte[16384];
                if (responseCode == ResponseCode.SUCCESS_MULTIRESPONSE)
                {
                    using var stream = new MemoryStream();
                    stream.Write(initialReadBuffer, initialPosition, initialBytesRead - initialPosition);
                    while (mClient.Available > 0)
                    {
                        var bytesRead = await mClient.ReceiveAsync(readBuffer);
                        stream.Write(readBuffer, 0, bytesRead);
                    }

                    var body = new List<string>();
                    var multiLineBuffer = stream.ToArray();
                    var multiLineBufferPosition = 0;
                    while (multiLineBufferPosition < multiLineBuffer.Length)
                    {
                        var line = ExtractLine(ref multiLineBuffer, multiLineBuffer.Length, ref multiLineBufferPosition);
                        if (line == ".")
                        {
                            break;
                        }
                        body.Add(line);
                    }

                    socketResponse.Body = body.ToArray();
                    return socketResponse;
                }

                if (responseCode == ResponseCode.SUCCESS_BINRESPONSE)
                {
                    if (binaryResponseStream == null)
                    {
                        return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Binary response stream not provided" };
                    }

                    binaryResponseStream.Write(initialReadBuffer, initialPosition, initialBytesRead - initialPosition);
                    var expectedSize = binaryResponseStream.ExpectedSize;

                    while (binaryResponseStream.Length != expectedSize)
                    {
                        while (mClient.Available > 0)
                        {
                            var bytesRead = await mClient.ReceiveAsync(readBuffer);
                            if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                            {
                                FlushIncomingData();
                                return new SocketResponse { ResponseCode = ResponseCode.SUCCESS_CANCELLED, Response = "Cancelled" };
                            }
                            binaryResponseStream.Write(readBuffer, 0, bytesRead);
                        }
                    }

                    return new SocketResponse { ResponseCode = ResponseCode.SUCCESS_OK, Response = "OK" };
                }

                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected Result" };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Failed process body" };
            }
        }

        public async void FlushIncomingData()
        {
            if (mClient == null)
            {
                return;
            }

            byte[] buffer = new byte[1024];
            while (mClient.Available > 0)
            {
                _ = await mClient.ReceiveAsync(buffer);
            }
        }

        public async Task<SocketResponse> TrySendStringAsync(string value)
        {
            if (mClient == null)
            {
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Client not open" };
            }
            int retryCount = 0;
            while (retryCount < 2)
            {
                retryCount++;
                try
                {
                    var buffer = Encoding.UTF8.GetBytes(value);
                    var sent = await mClient.SendAsync(buffer, SocketFlags.None);
                    if (sent != buffer.Length)
                    {
                        return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected result" };
                    }
                    return new SocketResponse { ResponseCode = ResponseCode.SUCCESS_OK, Response = "OK" };
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                    await OpenAsync(mAddress);
                }
            }

            return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Failed to send message" };
        }

        private async Task<SocketResponse> TryConnectAsync()
        {
            if (mClient == null)
            {
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Client not open" };
            }
            try
            {
                await mClient.ConnectAsync(IPAddress.Parse(mAddress), 731);
                var response = await TryRecieveBodyAsync();
                if (response.ResponseCode != ResponseCode.SUCCESS_CONNECTED)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected result" };
                }
                return new SocketResponse { ResponseCode = ResponseCode.SUCCESS_OK, Response = "OK" };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Failed to connect" };
            }
        }

        public async Task<bool> OpenAsync(string ipAddress)
        {
            Close();

            mAddress = ipAddress;
            mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 1000
            };
            var response = await TryConnectAsync();
            if (Utils.IsSuccess(response.ResponseCode) == false)
            {
                Close();
                return false;
            }
            return true;
        }

        public bool Connected()
        {
            if (mClient == null)
            {
                return false;
            }
            return mClient.Connected == true;
        }

        public void Close()
        {
            if (mClient == null)
            {
                return;
            }
            if (mClient.Connected == true)
            {
                //mClient.Send("bye");
                mClient.Shutdown(SocketShutdown.Both);
                mClient.Close();
            }
            mClient.Dispose();
            mClient = null;
        }
    }
}
