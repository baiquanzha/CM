using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using MTool.Framework.Kcp;

namespace MTool.Framework.Network
{
    public class KcpClient : NetworkClient
    {
        private Socket socket;
        private KCP kcp;
        private IMsgProcesser MsgProcesser;
        private ByteBuffer recvBuffer = ByteBuffer.Allocate(Const.Recv_Buffer_Size);
        private MemoryStream bufferStream = new MemoryStream();
        private MemoryStream sendStream = new MemoryStream();

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

        public event Action<OptionMsg> PostOptionMsgEvent;
        public event Action<byte[]> PostMsgEvent;
        private uint NextUpdateTime = 0;

        public override void Connect(string host, int port)
        {
            try
            {
                if (IsConnected)
                {
                    CloseSocket();
                }

                IPAddress[] address = Dns.GetHostAddresses(host);
                var endpoint = new IPEndPoint(address[0], port);
                socket = new Socket(endpoint.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
                Trace.Instance.debug("BeginConnect. host = {0}, port = {1}", host, port);
                socket.BeginConnect(endpoint, new AsyncCallback(HandleConnect), socket);
                uint conv = 1;
                kcp = new KCP(conv, SendInternal);
                kcp.NoDelay(0, 30, 2, 1);
                kcp.SetStreamMode(true);
                recvBuffer.Clear();
            }
            catch (Exception e)
            {
                Trace.Instance.error("connect throw exception. host = {0}, port = {1}, exception = {2}", host, port, e.Message);
                PostOptionMsgEvent?.Invoke(new ConnectMsg(false, EConnectType.KCP));
            }
        }

        private void HandleConnect(IAsyncResult result)
        {
            ConnectMsg msg = new ConnectMsg(false, EConnectType.KCP);
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

        private void BeginReceive()
        {
            socket.BeginReceive(recvBuffer.RawBuffer, recvBuffer.WriterIndex, Const.Recv_Buffer_Size, SocketFlags.None, HandleReceive, recvBuffer.RawBuffer);
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
                    int inputN = kcp.Input(recvBuffer.RawBuffer, recvBuffer.ReaderIndex, bytesRead, true, false);
                    //UnityEngine.Debug.Log($"KcpClient Input {bytesRead} bytes = {BitConverter.ToString(recvBuffer.RawBuffer, 0, bytesRead)}");
                    recvBuffer.Clear();
                    while (true)
                    {
                        int size = kcp.PeekSize();
                        if (size <= 0)
                            break;
                        recvBuffer.EnsureWritableBytes(size);
                        int n = kcp.Recv(recvBuffer.RawBuffer, recvBuffer.WriterIndex, size);
                        if (n > 0)
                            recvBuffer.WriterIndex += n;
                    }
                    if (recvBuffer.ReadableBytes > 0)
                    {
                        long original = bufferStream.Position;
                        bufferStream.Position = bufferStream.Length;
                        bufferStream.Write(recvBuffer.RawBuffer, recvBuffer.ReaderIndex, recvBuffer.ReadableBytes);
                        bufferStream.Position = original;
                        recvBuffer.Clear();

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
                    }
                    BeginReceive();
                }
                else
                {
                    CloseSocket();
                    PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Passive, EConnectType.KCP));
                }
            }
            catch (Exception e)
            {
                Trace.Instance.warn("recevie message throw exception. exception = {0}, stack = {1}", e.Message, e.StackTrace);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.KCP));
            }
        }

        public override void Disconnect()
        {
            if (IsConnected)
            {
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Active, EConnectType.KCP));
            }
        }

        public override bool IsConnect()
        {
            return IsConnected;
        }

        public override void SendMessage(byte[] data)
        {
            try
            {
                MsgProcesser.WriteSendBytes(data, sendStream);
                byte[] pack = new byte[sendStream.Length];
                sendStream.Position = 0;
                sendStream.Read(pack, 0, pack.Length);
                int waitSnd = kcp.WaitSnd;
                if (waitSnd < kcp.SndWnd && waitSnd < kcp.RmtWnd)
                {
                    int sendBytes = 0;
                    do
                    {
                        int n = Math.Min((int)kcp.Mss, pack.Length - sendBytes);
                        kcp.Send(pack, sendBytes, n);
                        sendBytes += n;
                    } while (sendBytes < pack.Length);
                    waitSnd = kcp.WaitSnd;
                    if (waitSnd >= kcp.SndWnd || waitSnd >= kcp.RmtWnd)
                        kcp.Flush(false);
                }
            }
            catch (Exception e)
            {
                Trace.Instance.warn("send message throw exception. exception = {0}", e.Message);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.KCP));
            }
        }

        public override void SetMsgProcesser(IMsgProcesser processer)
        {
            MsgProcesser = processer;
        }

        public override void Update()
        {
            if (socket == null || kcp == null)
                return;
            if (0 == NextUpdateTime || kcp.CurrentMS >= NextUpdateTime)
            {
                kcp.Update();
                NextUpdateTime = kcp.Check();
            }
        }

        private void SendInternal(byte[] data, int length)
        {
            try
            {
                socket.BeginSend(data, 0, length, SocketFlags.None, new AsyncCallback(HandleSend), null);
                //UnityEngine.Debug.Log($"KcpClient Send {length} bytes = {BitConverter.ToString(data, 0, length)}");
            }
            catch (Exception e)
            {
                Trace.Instance.warn("send message internal throw exception. exception = {0}", e.Message);
                CloseSocket();
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.KCP));
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
                PostOptionMsgEvent?.Invoke(new DisconnectMsg(DisconnectReason.Exception, EConnectType.KCP));
            }
        }

        private void CloseSocket()
        {
            try
            {
                //socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket = null;
                recvBuffer.Clear();
                bufferStream.Dispose();
                sendStream.Dispose();
                Trace.Instance.debug("EndConnect.");
            }
            catch (Exception e)
            {
                Trace.Instance.warn("close socket throw exception. exception = {0}, trace = {1}", e.Message, e.StackTrace);
            }
        }
    }
}