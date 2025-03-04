﻿using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using RXDKXBDM.Commands;

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


        public byte[] RawBuffer = new byte[1024];
        public int IndexBiffer = 0;
        public int CurrentBufferSize = 0;

        public int ReceiveBinary(ref byte[] recieveBuffer)
        {
            if (mClient == null)
            {
                return -1;
            }

            byte[] buffer = new byte[recieveBuffer.Length];
            var bytesRead = mClient.Receive(buffer, 0, buffer.Length, SocketFlags.None, out SocketError socketError);
            if (bytesRead > 0)
            {
                Array.Copy(buffer, 0, recieveBuffer, 0, bytesRead);
                return bytesRead;
            }

            if (socketError == SocketError.Interrupted)
            {
                return 0;
            }

            if (socketError == SocketError.WouldBlock)
            {
                int timeoutMs = 1000;
                bool isReady = mClient.Poll(timeoutMs * 5000, SelectMode.SelectRead);
                if (isReady)
                {
                    return 0;
                }
            }

            return -1;
        }


        public bool TryRecieveLine(out string line)
        {
            line = string.Empty;

            var stringBuilder = new StringBuilder();
            while (true)
            {
                while (IndexBiffer >= CurrentBufferSize)
                {
                    IndexBiffer = 0;
                    CurrentBufferSize = ReceiveBinary(ref RawBuffer);
                    if (CurrentBufferSize < 0)
                    {
                        return false;
                    }
                }
                var currentChar = (char)RawBuffer[IndexBiffer++];
                if (currentChar == '\r')
                {
                    continue;
                }
                if (currentChar != '\n')
                {
                    stringBuilder.Append(currentChar);
                    continue;
                }
                break;
            }
            line = stringBuilder.ToString();
            return true;
        }

        public string[] GetMultiLineResponse()
        {
            var body = new List<string>();
            while (true)
            {
                if (TryRecieveLine(out var line) == false || line.Equals("."))
                {
                    break;
                }
                body.Add(line);
            }
            return body.ToArray();
        }

        public SocketResponse TryRecieveHeaderResponse()
        {
            if (mClient == null)
            {
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Client Not Open" };
            }
            try
            {
                if (TryRecieveLine(out var header) == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected Result" };
                }

                if (header.Substring(3, 2).Equals("- ") == false || int.TryParse(header.AsSpan(0, 3), out var responseCodeInt) == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Unexpected Result" };
                }

                var responseCode = (ResponseCode)responseCodeInt;
                var response = header.Substring(5);
                var socketResponse = new SocketResponse { ResponseCode = (ResponseCode)responseCode, Response = response };
                return socketResponse;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new SocketResponse { ResponseCode = ResponseCode.ERROR_INTERNAL_ERROR, Response = "Failed process body" };
            }
        }

        public bool TryRecieveBinarySize(out uint size)
        {
            size = 0;

            if (mClient == null)
            {
                return false;
            }

            var buffer = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                while (IndexBiffer >= CurrentBufferSize)
                {
                    IndexBiffer = 0;
                    CurrentBufferSize = ReceiveBinary(ref RawBuffer);
                    if (CurrentBufferSize < 0)
                    {
                        return false;
                    }
                }
                buffer[i] = RawBuffer[IndexBiffer++];
            }

            size = BitConverter.ToUInt32(buffer, 0);
            return true;
        }

        public bool TryStreamBinaryData(ExpectedSizeStream expectedSizeStream, CancellationToken cancellationToken)
        {
            for (var i = 0; i < expectedSizeStream.ExpectedSize; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    //todo: close and open stream to flush out data
                    return false;
                }
                while (IndexBiffer >= CurrentBufferSize)
                {
                    IndexBiffer = 0;
                    CurrentBufferSize = ReceiveBinary(ref RawBuffer);
                    if (CurrentBufferSize < 0)
                    {
                        return false;
                    }
                }
                var value = RawBuffer[IndexBiffer++];
                expectedSizeStream.WriteByte(value);
            }
            return true;
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

        public bool WaitForConnection(TimeSpan timeout)
        {
            if (mClient == null)
            {
                return false;
            }
            try
            {
                return mClient.Poll(timeout, SelectMode.SelectWrite);
            }
            catch
            {
                return false;
            }
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

                WaitForConnection(TimeSpan.FromSeconds(1));

                var response = TryRecieveHeaderResponse();
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
            mClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mClient.Blocking = false;

            var response = await TryConnectAsync();
            if (Utils.IsSuccess(response.ResponseCode) == false)
            {
                Close();
                return false;
            }
            return true;
        }


        public void Close()
        {
            if (mClient == null)
            {
                return;
            }
            try
            {
                if (mClient.Connected)
                {
                    mClient.Shutdown(SocketShutdown.Both);
                }
            }
            finally
            {
                mClient.Close();
                mClient.Dispose();
                mClient = null;
            }
        }
    }
}
