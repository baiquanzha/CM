using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MTool.Framework.Network
{
    public class TcpClient : NetworkClient
    {
        private Socket socket;
        private IMsgProcesser MsgProcesser;

        private byte[] buffer = new byte[Const.Recv_Buffer_Size];
        MemoryStream bufferStream = new MemoryStream();
        MemoryStream sendStream = new MemoryStream();

        public event Action<OptionMsg> PostOptionMsgEvent;
        public event Action<byte[]> PostMsgEvent;

        public bool IsConnected
        {
            get
            {
                if (socket != null && socket.Connected)
                {
                    return true;
                }
                return false;
            }
        }

        public override void Connect(string host, int port)
        {
            try
            {
                if (IsConnected)
                {
                    CloseSocket();
                }

                IPAddress[] address = Dns.GetHostAddresses(host);
                socket = new Socket(address[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);
                Trace.Instance.debug("BeginConnect. host = {0}, port = {1}", address[0].ToString(), port);
                socket.BeginConnect(address, port, new AsyncCallback(HandleConnect), socket);
            }
            catch (Exception e)
            {
                Trace.Instance.error("connect throw exception. host = {0}, port = {1}, exception = {2}", host, port, e.Message);
                PostOptionMsgEvent?.Invoke(new ConnectMsg(false, EConnectType.TCP));
            }
        }

        public override void SetMsgProcesser(IMsgProcesser processer)
        {
            MsgProcesser = processer;
        }

        private void HandleConnect(IAsyncResult result)
        {
            ConnectMsg msg = new ConnectMsg(false, EConnectType.TCP);
            try
            {
                Socket _socket = (Socket)result.AsyncState;
                _socket.EndConnect(result);
                if (_socket.Connected)
                {
                    msg.Success = true;
                    BeginReceive();
                }
            }
            catch (Exception e)
            {
                Trace.Instance.warn("HandleConnect throw exception. exception = {0}", e.Message);
                CloseSocket();
            }
            finally
            {
                PostOptionMsgEvent?.Invoke(msg);
            }
        }

        public void CloseSocket()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
                bufferStream.Dispose();
                sendStream.Dispose();
                Trace.Instance.debug("EndConnect.");
            }
            catch (Exception e)
            {
                Trace.Instance.warn("close socket throw exception. exception = {0}, trace = {1}", e.Message, e.StackTrace);
            }
        }

        private void BeginReceive()
        {
            socket.BeginReceive(buffer, 0, Const.Recv_Buffer_Size, SocketFlags.None, HandleReceive, buffer);
        }

        private void HandleReceive(IAsyncResult result)
        {
            try
            {
                if (!IsConnected)
                    return;
                int bytesRead = socket.EndReceive(result);
                if (bytesRead > 0)
                {
                    long original = bufferStream.Position;
                    bufferStream.Position = bufferStream.Length;
                    bufferStream.Write(buffer, 0, bytesRead);
                    bufferStream.Position = original;

                    while (true)
                    {
                        original = bufferStream.Position;
                        if (MsgProcesser.ReadRecvBytes(bufferStream, out byte[] data))
                            PostMsgEvent?.Invoke(data);
                        else
                        {
                            bufferStream.Position = original;
                            break;
                        }
                    }

                    if (bufferStream.Position == bufferStream.Length)
                    {
                        bufferStream.Position = 0;
                        bufferStream.SetLength(0);
                    }
                    else if (bufferStream.Position >= Const.Recv_Buffer_Size)
                    {
                        int len = (int)(bufferStream.Length - bufferStream.Position);
                        byte[] temp = new byte[len];
                        bufferStream.Read(temp, 0, len);
                        bufferStream.SetLength(0);
                        bufferStream.Write(temp, 0, len);
                        bufferStream.Position = 0;
                        Trace.Instance.debug("bufferStream overflow");
                    }

                    BeginReceive();
                }
                else
                {
                    CloseSocket();
                    PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Passive, EConnectType.TCP));
                }
            }
            catch (Exception e)
            {
                Trace.Instance.warn("recevie message throw exception. exception = {0}", e.Message);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.TCP));
            }
        }

        public override void SendMessage(byte[] data)
        {
            try
            {
                MsgProcesser.WriteSendBytes(data, sendStream);
                byte[] pack = new byte[sendStream.Length];
                sendStream.Position = 0;
                sendStream.Read(pack, 0, pack.Length);
                socket.BeginSend(pack, 0, pack.Length, SocketFlags.None, new AsyncCallback(HandleSend), null);
            }
            catch (Exception e)
            {
                Trace.Instance.warn("send message throw exception. exception = {0}", e.Message);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.TCP));
            }
        }

        private void HandleSend(IAsyncResult result)
        {
            try
            {
                socket.EndSend(result);
            }
            catch (Exception e)
            {
                Trace.Instance.warn("HandleSend throw exception. exception = {0}", e.Message);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.TCP));
            }
        }

        public override void Disconnect()
        {
            if (IsConnected)
            {
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Active, EConnectType.TCP));
            }
        }

        public override bool IsConnect()
        {
            return IsConnected;
        }

        public override void Update()
        {
        }
    }
}