using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net.Mail;
using System;
using RXDKXBDM.Commands;
using System.Linq;
using System.IO;

namespace RXDKXBDM
{
    public enum ConnectionState
    {
        Success,
        UnexpectedResult,
        Timeout,
        ClientNotOpen,
        SocketError,
    }

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

        //202- multiline response follows
        //freetocallerlo = 0xf2674000 freetocallerhi=0x00000000 totalbyteslo=0x311a0000 totalbyteshi=0x00000001 totalfreebyteslo=0xf2674000 totalfreebyteshi=0x00000000
        //.

        //202- multiline response follows
        //name = "crashdump.xdmp" sizehi=0x0 sizelo=0x8002000 createhi=0x01c3cc0a createlo=0xc39b9b00 changehi=0x01c3cc0a changelo=0xcbf3d600
        //name="xbdm.ini" sizehi=0x0 sizelo=0xb2 createhi=0x01d7ff07 createlo=0x1b624d00 changehi=0x01d7ff07 changelo=0x1b624d00
        //name="xonline.ini" sizehi=0x0 sizelo=0x21 createhi=0x01c3cc0a createlo=0xcbf3d600 changehi=0x01c3cc0a changelo=0xcbf3d600
        //name="dxt" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xcbf3d600 changehi=0x01c3cc0a changelo=0xcbf3d600 directory
        //name = "NexgenRedux-OG" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xcd250300 changehi=0x01c3cc0a changelo=0xcd250300 directory
        //name = "pix" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xcd250300 changehi=0x01c3cc0a changelo=0xcd250300 directory
        //name = "ProfileData" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xce563000 changehi=0x01c3cc0a changelo=0xce563000 directory
        //name = "XBMC" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xce563000 changehi=0x01c3cc0a changelo=0xce563000 directory
        //name = "SAMPLES" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xd31ae400 changehi=0x01c3cc0a changelo=0xd31ae400 directory
        //name = "TOOLS" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xd44c1100 changehi=0x01c3cc0a changelo=0xd44c1100 directory
        //name = "VTune" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc0a createlo=0xd57d3e00 changehi=0x01c3cc0a changelo=0xd57d3e00 directory
        //name = "Avalaunch" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc81 createlo=0x87efe800 changehi=0x01c3cc81 changelo=0x87efe800 directory
        //name = "PrometheOSXbe" sizehi=0x0 sizelo=0x0 createhi=0x01c3cc86 createlo=0x98ef2800 changehi=0x01c3cc86 changelo=0x98ef2800 directory
        //name = "PrometheOSLauncher" sizehi=0x0 sizelo=0x0 createhi=0x01db58cb createlo=0xcd16f200 changehi=0x01db58cb changelo=0xcd16f200 directory
        //name = "GamepadTest" sizehi=0x0 sizelo=0x0 createhi=0x01db5a72 createlo=0xa515c300 changehi=0x01db5a72 changelo=0xa515c300 directory
        //.

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

        public async Task<SocketResponse> TryRecieveStringAsync()
        {
            if (mClient == null)
            {
                return new SocketResponse { ResponseCode = ResponseCode.ClientNotOpen, Response = "Client Not Open" };
            }
            try
            {
                if (WaitAvailable() == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.Timeout, Response = "Timeout" };
                }
             
                var readBuffer = new byte[1024];

                var bytesRead = await mClient.ReceiveAsync(readBuffer);
                if (bytesRead < 5)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.UnexpectedResult, Response = "Unexpected Result" };
                }

                var position = 0;
                var header = ExtractLine(ref readBuffer, bytesRead, ref position);
                if (header.Substring(3, 2).Equals("- ") == false || int.TryParse(header.AsSpan(0, 3), out var responseCodeInt) == false)
                {
                    return new SocketResponse { ResponseCode = ResponseCode.UnexpectedResult, Response = "Unexpected Result" };
                }

                var responseCode = (ResponseCode)responseCodeInt;
                var response = header.Substring(5);
                var socketResponse = new SocketResponse { ResponseCode = (ResponseCode)responseCode, Response = response };
                if (responseCode == ResponseCode.XBDM_SUCCESS_OK || responseCode == ResponseCode.XBDM_SUCCESS_CONNECTED)
                {
                    return socketResponse;
                }

                if (responseCode == ResponseCode.XBDM_SUCCESS_MULTIRESPONSE)
                {
                    using var stream = new MemoryStream();
                    stream.Write(readBuffer, position, bytesRead - position);
                    while (mClient.Available > 0)
                    {
                        var bytesRead2 = await mClient.ReceiveAsync(readBuffer);
                        stream.Write(readBuffer, 0, bytesRead2);
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

                return new SocketResponse { ResponseCode = ResponseCode.UnexpectedResult, Response = "Unexpected Result" };
            }
            catch (Exception ex)
            {
                return new SocketResponse { ResponseCode = ResponseCode.SocketError, Response = ex.ToString() };
            }
        }

        public async Task<ConnectionState> TrySendStringAsync(string value)
        {
            if (mClient == null)
            {
                Debug.Print("Error in TrySendString: Client Not Open");
                return ConnectionState.ClientNotOpen;
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
                        Debug.Print("Error in TrySendString: Unexpected Result");
                        return ConnectionState.UnexpectedResult;
                    }
                    return ConnectionState.Success;
                }
                catch (Exception ex)
                {
                    Debug.Print($"Error in TrySendString: {ex}");
                    await OpenAsync(mAddress);
                }
            }
            return ConnectionState.SocketError;
        }

        private async Task<ConnectionState> TryConnectAsync()
        {
            if (mClient == null)
            {
                Debug.Print("Error in TryConnect: Client Not Open");
                return ConnectionState.ClientNotOpen;
            }
            try
            {
                await mClient.ConnectAsync(IPAddress.Parse(mAddress), 731);
                var response = await TryRecieveStringAsync();
                if (response.ResponseCode != ResponseCode.XBDM_SUCCESS_CONNECTED)
                { 
                    Debug.Print("Error in TrySendString: Unexpected Result");
                    return ConnectionState.UnexpectedResult;
                }
                return ConnectionState.Success;
            }
            catch (Exception ex)
            {
                Debug.Print($"Error in TrySendString: {ex}");
                return ConnectionState.SocketError;
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
            if (await TryConnectAsync() != ConnectionState.Success)
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
