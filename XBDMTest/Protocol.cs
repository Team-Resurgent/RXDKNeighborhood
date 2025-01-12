using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XBDMTest.Commands;

namespace XBDMTest
{

    public static class Protocol
    {

        public static void DoCloseSharedConnection(SharedConnectionInfo sharedConnectionInfo, Connection? connection)
        {
            lock (sharedConnectionInfo.SharedConnectionLock)
            {
                if (connection == null)
                {
                    HrUseSharedConnection(sharedConnectionInfo, false);
                    HrUseSharedConnection(sharedConnectionInfo, true);
                    sharedConnectionInfo.UlXboxIPAddr = null;
                    return;
                }
                else if (connection == sharedConnectionInfo.SharedConnection)
                {
                    if (sharedConnectionInfo.FAllowSharing)
                    {
                        return;
                    }
                    else
                    {
                        sharedConnectionInfo.SharedConnection = null;
                        sharedConnectionInfo.TidShared = 0;
                    }
                }
            }

            DmCloseConnection(connection);
        }

        public static int ReceiveBinary(Connection connection, byte[] recieveBuffer)
        {
            if (connection.Socket == null)
            {
                return -1;
            }

            byte[] buffer = new byte[recieveBuffer.Length];
            var bytesRead = connection.Socket.Receive(buffer, 0, buffer.Length, SocketFlags.None, out SocketError socketError);
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
                if (connection.SharedConnectionInfo != null && connection.SharedConnectionInfo.DwConversationTimeout > 0)
                {
                    int timeoutMs = (int)connection.SharedConnectionInfo.DwConversationTimeout;
                    bool isReady = connection.Socket.Poll(timeoutMs * 1000, SelectMode.SelectRead);
                    if (isReady)
                    {
                        return 0; 
                    }
                }
            }

            connection.Close();
            return -1;
        }

        public static bool WaitForConnection(Socket socket, TimeSpan timeout)
        {
            try
            {
                return socket.Poll(timeout, SelectMode.SelectWrite);
            }
            catch
            {
                return false;
            }
        }





        public static ResultCode HrFromStatus(string sz)
        {
            var code = sz.Substring(0, 3);
            return (ResultCode)int.Parse(code);
        }



        public static ResultCode HrOpenConnectionCore(SharedConnectionInfo sharedConnectionInfo, out Connection? connection)
        {
            connection = null;

            var tempConnection = new Connection();
            if (tempConnection.Socket == null || sharedConnectionInfo.UlXboxIPAddr == null)
            {
                return ResultCode.ERROR_CANNOTCONNECT;
            }

            // Special check to see whether we need to update from the registry
            if (sharedConnectionInfo == Globals.GlobalSharedConnection)
            {
            //    hr = DmSetXboxName(null);
            //    if (hr.Failed())
            //        return hr;
            }

            //if (psci.UlXboxIPAddr != 0 && psci.FCacheAddr)
            //    sin.sin_addr.s_addr = psci.UlXboxIPAddr;
            //else
            //{
            //    hr = HrResolveNameIP(psci.szXboxName, ref sin);
            //    if (hr.Failed())
            //        return hr;
            //    psci.ulXboxIPAddr = sin.sin_addr.s_addr;
            //}

            tempConnection.SharedConnectionInfo = sharedConnectionInfo;
            if (sharedConnectionInfo.DwConnectionTimeout != 0)
            {
                try
                {
                    tempConnection.Socket.Blocking = false;
                }
                catch
                {
                    tempConnection.Socket.Close();
                    return ResultCode.ERROR_CANNOTCONNECT;
                }
            }

            try
            {
                tempConnection.Socket.Connect(new IPEndPoint(sharedConnectionInfo.UlXboxIPAddr, 731));
            }
            catch (SocketException ex) when (sharedConnectionInfo.DwConnectionTimeout != 0 && ex.SocketErrorCode == SocketError.WouldBlock)
            {
                var timeout = TimeSpan.FromMilliseconds(sharedConnectionInfo.DwConnectionTimeout);
                if (!WaitForConnection(tempConnection.Socket, timeout))
                {
                    tempConnection.Socket.Close();
                    return ResultCode.ERROR_CANNOTCONNECT;
                }
            }
            catch
            {
                tempConnection.Socket.Close();
                return ResultCode.ERROR_CANNOTCONNECT;
            }

            if (sharedConnectionInfo.DwConnectionTimeout == 0)
            {
                try
                {
                    tempConnection.Socket.Blocking = false;
                }
                catch
                {
                    tempConnection.Socket.Close();
                    return ResultCode.ERROR_CANNOTCONNECT;
                }
            }

            connection = tempConnection;
            return ResultCode.SUCCESS_OK;
        }



        public static ResultCode HrOpenConnection(SharedConnectionInfo sharedConnectionInfo, out Connection? connection, bool fRequireAccess)
        {
            ResultCode hr;

            hr = HrOpenConnectionCore(sharedConnectionInfo, out connection);
            if (!Utils.IsSuccess(hr) || connection == null)
            {
                return hr;
            }

            hr = DmReceiveStatusResponse(connection, out var response);
            if (!Utils.IsSuccess(hr))
            {
                DmCloseConnection(connection);
                return hr;
            }

            if (Utils.FGetQwordParam(response, "BOXID", out var boxId) && Utils.FGetQwordParam(response, "NONCE", out var connectNonce))
            {
                System.Diagnostics.Debug.Print("TODO");
            //    hrT = HrAuthenticateUser(ppdcon, ref luBoxId, ref luConnectNonce);
            //    if (fRequireAccess)
            //        hr = hrT;
            //    if (IsFailed(hr))
            //    {
            //        DmCloseConnection(ppdcon);
            //        return hr;
            //    }
            }

            return ResultCode.SUCCESS_OK;
        }

        public static ResultCode HrDoOpenSharedConnection(SharedConnectionInfo sharedConnectionInfo, out Connection? connection)
        {
            ResultCode hr = ResultCode.ERROR_INVALIDARG;
            bool fCanShare = false;

            connection = null;
            if (sharedConnectionInfo.FAllowSharing)
            {
                lock (sharedConnectionInfo.SharedConnectionLock)
                {
                    if (sharedConnectionInfo.SharedConnection != null && sharedConnectionInfo.SharedConnection.Socket != null)
                    {
                        DmCloseConnection(sharedConnectionInfo.SharedConnection);
                        sharedConnectionInfo.SharedConnection = null;
                    }

                    if (sharedConnectionInfo.SharedConnection == null)
                    {
                        fCanShare = true;
                        hr = HrOpenConnection(sharedConnectionInfo, out var tempConnection, false);
                        if (!Utils.IsSuccess(hr))
                        {
                            sharedConnectionInfo.SharedConnection = null;
                        }
                        else
                        {
                            sharedConnectionInfo.SharedConnection = tempConnection;
                        }
                    }
                    else if (sharedConnectionInfo.TidShared == 0)
                    {
                        fCanShare = true;
                        hr = ResultCode.SUCCESS_OK;
                    }

                    if (fCanShare && sharedConnectionInfo.SharedConnection != null)
                    {
                        sharedConnectionInfo.TidShared = Thread.CurrentThread.ManagedThreadId;
                        connection = sharedConnectionInfo.SharedConnection;
                    }
                }
            }

            if (!fCanShare)
            {
                hr = HrOpenConnection(sharedConnectionInfo, out connection, false);
            }
            return hr;
        }

        public static ResultCode HrUseSharedConnection(SharedConnectionInfo sharedConnectionInfo, bool fShare)
        {
            if (sharedConnectionInfo.FAllowSharing == fShare)
            {
                return ResultCode.SUCCESS_OK;
            }

            lock (sharedConnectionInfo.SharedConnectionLock)
            {
                if (!fShare && sharedConnectionInfo.SharedConnection != null)
                {
                    if (sharedConnectionInfo.TidShared == 0)
                    {
                        DmCloseConnection(sharedConnectionInfo.SharedConnection);
                    }
                    sharedConnectionInfo.SharedConnection = null;
                }

                sharedConnectionInfo.FAllowSharing = fShare;
            }

            return ResultCode.SUCCESS_OK;
        }

        

        public static  ResultCode HrDoOneLineCmd(Connection connection, string command)
        {
            ResultCode hr;

            hr = DmSendCommand(connection, command, out _);
            if (hr == ResultCode.SUCCESS_READYFORBIN || hr == ResultCode.SUCCESS_MULTIRESPONSE || hr == ResultCode.SUCCESS_BINRESPONSE)
            {
                connection.Close();
                hr = ResultCode.ERROR_UNEXPECTED;
            }
            return hr;
        }






        public static ResultCode DmReceiveSocketLine(Connection connection, out string response)
        {
            response = string.Empty;

            var stringBuilder = new StringBuilder();
            char currentChar;
            do
            {
                while (connection.IndexBiffer >= connection.CurrentBufferSize)
                {
                    connection.IndexBiffer = 0;
                    connection.CurrentBufferSize = ReceiveBinary(connection, connection.RawBuffer);
                    if (connection.CurrentBufferSize < 0)
                    {
                        return ResultCode.ERROR_CONNECTIONLOST;
                    }
                }
                currentChar = (char)connection.RawBuffer[connection.IndexBiffer++];
                if (currentChar == '\r')
                {
                    continue;
                }
                if (currentChar != '\n')
                {
                    stringBuilder.Append(currentChar);
                }
            } 
            while (currentChar != '\n');

            response = stringBuilder.ToString();
            return ResultCode.SUCCESS_OK;
        }

        public static ResultCode DmReceiveStatusResponse(Connection connection, out string response)
        {
            ResultCode hr = DmReceiveSocketLine(connection, out response);
            if (!Utils.IsSuccess(hr))
            {
                return hr;
            }
            return HrFromStatus(response.ToString());
        }

        public static ResultCode DmSendBinary(Connection connection, byte[] data)
        {
            if (connection.Socket == null)
            {
                return ResultCode.ERROR_CONNECTIONLOST;
            }

            try
            {
                var dataLength = data.Length;
                while (dataLength > 0)
                {
                    try
                    {
                        var bytesSent = connection.Socket.Send(data, data.Length - dataLength, dataLength, SocketFlags.None);
                        dataLength -= bytesSent;
                    }
                    catch (SocketException ex)
                    {
                        SocketError socketError = ex.SocketErrorCode;

                        if (socketError == SocketError.Interrupted)
                        {
                            continue;
                        }
                        else if (socketError == SocketError.WouldBlock)
                        {
                            if (connection.Socket.Poll(TimeSpan.FromSeconds(1), SelectMode.SelectWrite))
                            {
                                continue;
                            }
                        }
                        break;
                    }
                    catch
                    {
                        break;
                    }
                }

                if (dataLength > 0)
                {
                    connection.Close();
                    return ResultCode.ERROR_CONNECTIONLOST;
                }
            }
            catch
            {
                connection.Close();
                return ResultCode.ERROR_CONNECTIONLOST;
            }

            return ResultCode.SUCCESS_OK;
        }

        public static ResultCode DmCloseConnection(Connection? connection)
        {
            if (connection == null)
            {
                return ResultCode.ERROR_INVALIDARG;
            }
            if (connection.Socket != null)
            {
                _ = DmSendBinary(connection, Encoding.UTF8.GetBytes("BYE\r\n"));
                connection.Close();
            }
            return ResultCode.SUCCESS_OK;
        }

        public static ResultCode DmSendCommand(Connection? connection, string? command, out string response)
        {
            response = string.Empty;

            ResultCode hr;

            bool sharedConnection = connection == null;

            if (sharedConnection)
            {
                hr = HrDoOpenSharedConnection(Globals.GlobalSharedConnection, out connection);
                if (!Utils.IsSuccess(hr))
                {
                    return hr;
                }
            }

            if (connection == null)
            {
                return ResultCode.ERROR_CONNECTIONLOST;
            }

            if (command == null)
            {
                if (sharedConnection)
                {
                    DoCloseSharedConnection(Globals.GlobalSharedConnection, connection);
                }
                return ResultCode.SUCCESS_OK;
            }

            hr = DmSendBinary(connection, UTF8Encoding.UTF8.GetBytes(command + "\r\n"));
            if (Utils.IsSuccess(hr))
            {
                hr = DmReceiveStatusResponse(connection, out response);
            }

            if (sharedConnection)
            {
                DoCloseSharedConnection(Globals.GlobalSharedConnection, connection);
            }

            return hr;
        }
    }
}
