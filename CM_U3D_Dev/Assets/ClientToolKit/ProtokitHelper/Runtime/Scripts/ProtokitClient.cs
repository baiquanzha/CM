using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Google.Protobuf;
using gen.msg;
using gen.netutils;
using MTool.Framework;
using MTool.Framework.Base;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace ProtokitHelper
{
    public delegate void DelegateResponseMetadataHandler(IMessage message, Dictionary<string, string> metadata);
    public delegate void DelegateHandShakeResp(uint code, string desc);

    public enum TransProtocolType
    {
        TCP,
        KCP
    }

    public class ProtokitClient : Singleton<ProtokitClient>
    {
        private Queue<RawProto> RawPacketQueue = new Queue<RawProto>();
        private Dictionary<string, Action<IMessage>> RequestHandlers = new Dictionary<string, Action<IMessage>>();
        private Dictionary<string, Action<IMessage>> MessageHandlers = new Dictionary<string, Action<IMessage>>();
        private Dictionary<int, RequestBatchRecord> RequestBatchRecordMap = new Dictionary<int, RequestBatchRecord>();
        private Queue<TypeLengthValueRawProto> TypeLengthValueQueue = new Queue<TypeLengthValueRawProto>();
        private Dictionary<int, Dictionary<string, string>> ResponseMetadataMap = new Dictionary<int, Dictionary<string, string>>();
        private Dictionary<string, DelegateResponseMetadataHandler> RequestMetadataHandlers = new Dictionary<string, DelegateResponseMetadataHandler>();

        public ProtoPacketType PacketType { get; private set; } = ProtoPacketType.RawPacket;

        public TransProtocolType ProtocolType { get; private set; } = TransProtocolType.TCP;

        /// <summary>
        /// 是否完成初始化
        /// </summary>
        public bool Inited { get; private set; } = false;

        /// <summary>
        /// 是否允许使用queue模式合并请求
        /// </summary>
        public bool EnableBatchRequest { get; private set; }

        /// <summary>
        /// 消息最大合并数
        /// </summary>
        public int MaxBatchCount { get; private set; } = 1;

        /// <summary>
        /// 是否使用Gate支持，对于使用Gate支持的连接，连接成功后必须先调用SendHandShake请求握手，当握手成功后才能发送逻辑消息
        /// </summary>
        public bool UseGate { get; private set; } = false;
        /// <summary>
        /// 是否已和Gate完成握手
        /// </summary>
        public bool HandShakeComplete { get; private set; } = false;
        /// <summary>
        /// 握手消息
        /// </summary>
        private Handshake handshakeMsg;

        /// <summary>
        /// 消息接收
        /// </summary>
        public event DelegateRecvProto Evt_RecvMsg;
        /// <summary>
        /// 消息包解析完成
        /// </summary>
        public event DelegateRecvPacketFinish Evt_RecvPackFinish;
        /// <summary>
        /// Update轮询
        /// </summary>
        public event Action Evt_Update;
        /// <summary>
        /// 握手消息返回
        /// </summary>
        public event DelegateHandShakeResp Evt_HandShakeResp;

        private StringBuilder sb = new StringBuilder();

        internal static readonly Lazy<ILogger> Logger = new Lazy<ILogger>(() => LoggerManager.GetLogger("ProtokitClient"));

        /// <summary>
        /// 是否输出消息收发日志
        /// </summary>
        public bool EnableProtoLog = true;

        public void Init(bool enableQueue, int batchLimit = 1, TransProtocolType protocolType = TransProtocolType.TCP)
        {
            EnableBatchRequest = enableQueue;
            MaxBatchCount = batchLimit;
            ProtocolType = protocolType;
            if (ProtocolType == TransProtocolType.TCP)
            {
                GameLauncher.Network.onConnectSuccess += OnConnectSuccess;
                GameLauncher.Network.onDisconnect += OnDisconnect;
            }
            else if (ProtocolType == TransProtocolType.KCP)
            {
                GameLauncher.Network.onKcpConnectSuccess += OnConnectSuccess;
                GameLauncher.Network.onKcpDisconnect += OnDisconnect;
            }
            Inited = true;
        }

        public void Reset()
        {
            if (Inited)
            {
                handshakeMsg = null;
                if (ProtocolType == TransProtocolType.TCP)
                {
                    GameLauncher.Network.onConnectSuccess -= OnConnectSuccess;
                    GameLauncher.Network.onDisconnect -= OnDisconnect;
                }
                else if (ProtocolType == TransProtocolType.KCP)
                {
                    GameLauncher.Network.onKcpConnectSuccess -= OnConnectSuccess;
                    GameLauncher.Network.onKcpDisconnect -= OnDisconnect;
                }
                Inited = false;
            }
        }

        public void StartConnect(string host, int port)
        {
            if (Inited)
            {
                UseGate = false;
                if (ProtocolType == TransProtocolType.TCP)
                    GameLauncher.Network.StartTcpConnect(host, port);
                else if (ProtocolType == TransProtocolType.KCP)
                    GameLauncher.Network.StartKcpConnect(host, port);
            }
            else
                Logger.Value?.Warn("ProtokitClient is not init, call ProtokitClient Init method first.");
        }

        public void StartConnectGate(string host, int port, Handshake handshake)
        {
            if (Inited)
            {
                UseGate = true;
                HandShakeComplete = false;
                handshakeMsg = handshake;
                if (handshakeMsg != null)
                {
                    if (ProtocolType == TransProtocolType.TCP)
                        GameLauncher.Network.StartTcpConnect(host, port);
                    else if (ProtocolType == TransProtocolType.KCP)
                        GameLauncher.Network.StartKcpConnect(host, port);
                }
                else
                    Logger.Value?.Warn("must pass valid handshake msg when try connect to gate");
            }
            else
                Logger.Value?.Warn("ProtokitClient is not init, call ProtokitClient Init method first.");
        }

        public void SetProtoPacketType(ProtoPacketType packetType)
        {
            PacketType = packetType;
        }

        private void OnConnectSuccess()
        {
            if (UseGate)
            {
                SendHandShake(handshakeMsg);
            }
        }

        private void OnDisconnect(string reason)
        {
            handshakeMsg = null;
            HandShakeComplete = false;
        }

        private void SendHandShake(Handshake handshake)
        {
            if (PacketType == ProtoPacketType.RawPacket)
            {
                var sendMsg = ProtokitUtil.Instance.GetRawPorto(handshake);
                RawPacketQueue.Enqueue(sendMsg);
            }
            else if (PacketType == ProtoPacketType.TypeLengthValue)
            {
                var tlvMsg = TypeLengthValueUtils.Instance.GenTypeLengthValueRawProto(handshake);
                TypeLengthValueQueue.Enqueue(tlvMsg);
            }
        }

        private void RecvHandShakeResp(HandshakeResp handshakeResp)
        {
            if (handshakeResp.Code == gen.msg.GErrorCode.Gsuccess)
                HandShakeComplete = true;
            else
                Logger.Value?.WarnFormat("Handshake Failed. Code:{0}, Desc:{1}", handshakeResp.Code.ToString(), handshakeResp.Desc);
            Evt_HandShakeResp?.Invoke((uint)handshakeResp.Code, handshakeResp.Desc);
        }

        public void SendMessage(IMessage message, Action<IMessage> handler = null, Dictionary<string, string> metadata = null)
        {
            if (!Inited)
            {
                Logger.Value?.Warn("send message canceled! ProtokitClient is not init, call ProtokitClient Init method first.");
                return;
            }
            if (UseGate && !HandShakeComplete)
            {
                Logger.Value?.Warn("send message canceled! because the HandShake must be the first message when use gate support.");
                return;
            }
            if (PacketType == ProtoPacketType.RawPacket)
            {
                var sendMsg = ProtokitUtil.Instance.GetRawPorto(message, metadata);
                RawPacketQueue.Enqueue(sendMsg);
                if (handler != null)
                    RequestHandlers.Add(sendMsg.Passthrough, handler);
            }
            else if (PacketType == ProtoPacketType.TypeLengthValue)
            {
                var tlvMsg = TypeLengthValueUtils.Instance.GenTypeLengthValueRawProto(message, metadata);
                TypeLengthValueQueue.Enqueue(tlvMsg);
                if (handler != null)
                    RequestHandlers.Add(tlvMsg.Passthrough, handler);
            }
        }

        public void SendMessageHandleMetadata(IMessage message, DelegateResponseMetadataHandler handler, Dictionary<string, string> metadata = null)
        {
            if (UseGate && !HandShakeComplete)
            {
                Logger.Value?.Warn("send message canceled! because the HandShake must be the first message when use gate support.");
                return;
            }
            if (PacketType == ProtoPacketType.RawPacket)
            {
                var sendMsg = ProtokitUtil.Instance.GetRawPorto(message, metadata);
                RawPacketQueue.Enqueue(sendMsg);
                if (handler != null)
                    RequestMetadataHandlers.Add(sendMsg.Passthrough, handler);
            }
            else if (PacketType == ProtoPacketType.TypeLengthValue)
            {
                var tlvMsg = TypeLengthValueUtils.Instance.GenTypeLengthValueRawProto(message, metadata);
                TypeLengthValueQueue.Enqueue(tlvMsg);
                if (handler != null)
                    RequestMetadataHandlers.Add(tlvMsg.Passthrough, handler);
            }
        }

        public void AddResponseMetadata(int sequenceId, Dictionary<string, string> metadata)
        {
            ResponseMetadataMap[sequenceId] = metadata;
        }

        public void RemoveResponseMetadata(int sequenceId)
        {
            if (ResponseMetadataMap.ContainsKey(sequenceId))
                ResponseMetadataMap.Remove(sequenceId);
        }

        /// <summary>
        /// 注册对某一类型消息的监听函数
        /// </summary>
        /// <param name="uri">消息名称</param>
        /// <param name="handler">消息处理回调</param>
        public void RegisterMessageHandler(string uri, Action<IMessage> handler)
        {
            if (MessageHandlers.ContainsKey(uri))
                MessageHandlers[uri] += handler;
            else
                MessageHandlers.Add(uri, handler);
        }

        /// <summary>
        /// 移除对某一类型消息的监听函数
        /// </summary>
        /// <param name="uri">消息名称</param>
        /// <param name="handler">消息处理回调</param>
        public void RemoveMessageHandler(string uri, Action<IMessage> handler)
        {
            if (MessageHandlers.ContainsKey(uri))
            {
                MessageHandlers[uri] -= handler;
                if (MessageHandlers[uri] == null)
                    MessageHandlers.Remove(uri);
            }
        }

        public void Update()
        {
            if (PacketType == ProtoPacketType.RawPacket)
                TrySendRawPacket();
            else if (PacketType == ProtoPacketType.TypeLengthValue)
                TrySendTypeLengthValue();
            Evt_Update?.Invoke();
        }

        private void TrySendRawPacket()
        {
            if (EnableBatchRequest)
            {
                if (RawPacketQueue.Count > 0)
                {
                    RawPacket rp = new RawPacket
                    {
                        Version = 1,
                        SequenceID = ProtokitUtil.Instance.GetSequenceId()
                    };
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = rp.SequenceID;
                    int batchCount = 0;
                    sb.Length = 0;
                    while (RawPacketQueue.Count > 0 && batchCount < MaxBatchCount)
                    {
                        var peekMsg = RawPacketQueue.Peek();
                        if (peekMsg.Metadata == null)
                        {
                            var msg = RawPacketQueue.Dequeue();
                            RawAny rawAny = new RawAny
                            {
                                Uri = msg.MsgUri,
                                Raw = msg.MsgRaw,
                                PassThrough = msg.Passthrough
                            };
                            rp.RawAny.Add(rawAny);
                            batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                            batchCount++;
                            sb.AppendFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                        }
                        else
                        {
                            if (batchCount == 0)
                            {
                                var msg = RawPacketQueue.Dequeue();
                                RawAny rawAny = new RawAny
                                {
                                    Uri = msg.MsgUri,
                                    Raw = msg.MsgRaw,
                                    PassThrough = msg.Passthrough
                                };
                                rp.RawAny.Add(rawAny);
                                var e = msg.Metadata.GetEnumerator();
                                while (e.MoveNext())
                                {
                                    rp.Metadata.Add(e.Current.Key);
                                    rp.Metadata.Add(e.Current.Value);
                                }
                                batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                                batchCount++;
                                sb.AppendFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                                break;
                            }
                            else
                                break;
                        }
                    }
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        rp.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        if (ProtocolType == TransProtocolType.TCP)
                            GameLauncher.Network.SendMessage(data);
                        else if (ProtocolType == TransProtocolType.KCP)
                            GameLauncher.Network.SendKcpMessage(data);
                        if (EnableProtoLog)
                            Logger.Value?.Debug(sb.ToString());
                    }
                }
            }
            else
            {
                while (RawPacketQueue.Count > 0)
                {
                    RawPacket rp = new RawPacket
                    {
                        Version = 1,
                        SequenceID = ProtokitUtil.Instance.GetSequenceId()
                    };
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = rp.SequenceID;
                    var msg = RawPacketQueue.Dequeue();
                    RawAny rawAny = new RawAny
                    {
                        Uri = msg.MsgUri,
                        Raw = msg.MsgRaw,
                        PassThrough = msg.Passthrough
                    };
                    rp.RawAny.Add(rawAny);
                    if (msg.Metadata != null)
                    {
                        var e = msg.Metadata.GetEnumerator();
                        while (e.MoveNext())
                        {
                            rp.Metadata.Add(e.Current.Key);
                            rp.Metadata.Add(e.Current.Value);
                        }
                    }
                    batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        rp.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        if (ProtocolType == TransProtocolType.TCP)
                            GameLauncher.Network.SendMessage(data);
                        else if (ProtocolType == TransProtocolType.KCP)
                            GameLauncher.Network.SendKcpMessage(data);
                        if (EnableProtoLog)
                            Logger.Value?.DebugFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                    }
                }
            }
        }

        private void TrySendTypeLengthValue()
        {
            if (EnableBatchRequest)
            {
                if (TypeLengthValueQueue.Count > 0)
                {
                    var tlvPacket = TypeLengthValueUtils.Instance.GenTypeLengthValuePacket();
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = (int)tlvPacket.Sequence;
                    int batchCount = 0;
                    sb.Length = 0;
                    while (TypeLengthValueQueue.Count > 0 && batchCount < MaxBatchCount)
                    {
                        var peekMsg = TypeLengthValueQueue.Peek();
                        if (peekMsg.Metadata == null)
                        {
                            var msg = TypeLengthValueQueue.Dequeue();
                            tlvPacket.AddProto(msg);
                            batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                            batchCount++;
                            sb.AppendFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                        }
                        else
                        {
                            if (batchCount == 0)
                            {
                                var msg = TypeLengthValueQueue.Dequeue();
                                tlvPacket.AddProto(msg);
                                batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                                batchCount++;
                                sb.AppendFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                                break;
                            }
                            else
                                break;
                        }
                    }
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        tlvPacket.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        if (ProtocolType == TransProtocolType.TCP)
                            GameLauncher.Network.SendMessage(data);
                        else if (ProtocolType == TransProtocolType.KCP)
                            GameLauncher.Network.SendKcpMessage(data);
                        if (EnableProtoLog)
                            Logger.Value?.Debug(sb.ToString());
                    }
                }
            }
            else
            {
                while (TypeLengthValueQueue.Count > 0)
                {
                    var tlvPacket = TypeLengthValueUtils.Instance.GenTypeLengthValuePacket();
                    RequestBatchRecord batchRecordItem = ProtokitUtil.Instance.GetRequestBatchRecord();
                    batchRecordItem.SequenceID = (int)tlvPacket.Sequence;
                    var msg = TypeLengthValueQueue.Dequeue();
                    tlvPacket.AddProto(msg);
                    if (msg.Metadata != null)
                    {
                        var e = msg.Metadata.GetEnumerator();
                        while (e.MoveNext())
                        {
                            tlvPacket.SetMetaData(e.Current.Key, e.Current.Value);
                        }
                    }
                    batchRecordItem.Requests.Add(msg.Passthrough, msg.MsgUri);
                    RequestBatchRecordMap.Add(batchRecordItem.SequenceID, batchRecordItem);
                    using (var stream = new MemoryStream())
                    {
                        tlvPacket.WriteTo(stream);
                        byte[] data = stream.ToArray();
                        if (ProtocolType == TransProtocolType.TCP)
                            GameLauncher.Network.SendMessage(data);
                        else if (ProtocolType == TransProtocolType.KCP)
                            GameLauncher.Network.SendKcpMessage(data);
                        if (EnableProtoLog)
                            Logger.Value?.DebugFormat("[{0}][Send] {1}", ProtocolType, msg.ProtoLog);
                    }
                }
            }
        }

        public void RecvMsg(int sequenceId, string passthrough, string uri, byte[] data)
        {
            try
            {
                sb.Length = 0;
                sb.AppendFormat("[TCP][Recv] name:{0}", uri);
                if (MessageHandlers.ContainsKey(uri))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        MessageHandlers[uri].Invoke(message);
                    }
                    else
                        Logger.Value?.Warn($"can't find message parser, uri={uri}");
                }
                if (RequestHandlers.ContainsKey(passthrough))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        RequestHandlers[passthrough].Invoke(message);
                    }
                    else
                    {
                        Logger.Value?.Warn($"can't find request parser, uri={uri}, sequenceId={sequenceId}, passthrough={passthrough}");
                    }
                    RequestHandlers.Remove(passthrough);
                }
                if (RequestMetadataHandlers.ContainsKey(passthrough))
                {
                    var parser = ProtokitUtil.Instance.GetParser(uri);
                    if (parser != null)
                    {
                        var message = parser.ParseFrom(data);
                        sb.AppendFormat(", body:{0}", message);
                        Dictionary<string, string> metadata = null;
                        if (ResponseMetadataMap.ContainsKey(sequenceId))
                            metadata = ResponseMetadataMap[sequenceId];
                        RequestMetadataHandlers[passthrough].Invoke(message, metadata);
                    }
                    else
                        Logger.Value?.Warn($"can't find request parser, uri={uri}, sequenceId={sequenceId}, passthrough={passthrough}");
                    RequestMetadataHandlers.Remove(passthrough);
                }
                if (uri.Equals(ErrorResponse.Descriptor.FullName))
                {
                    ErrorResponse errmsg = ErrorResponse.Parser.ParseFrom(data);
                    sb.AppendFormat(", body:{0}", errmsg);
                    if (!errmsg.LogicException)
                    {
                        RecvCommonError(sequenceId, passthrough, errmsg);
                    }
                }
                else if (uri.Equals(HandshakeResp.Descriptor.FullName))
                {
                    HandshakeResp resp = HandshakeResp.Parser.ParseFrom(data);
                    sb.AppendFormat(", body:{0}", resp);
                    RecvHandShakeResp(resp);
                }
                if (EnableProtoLog)
                    Logger.Value?.Debug(sb.ToString());
                Evt_RecvMsg?.Invoke(sequenceId, passthrough, uri, data);
            }
            catch (Exception e)
            {
                Logger.Value?.Error($"ProtokiClient RecvMsg catch exception : {e.Message}, sequenceId={sequenceId}, passthrough={passthrough}, uri={uri}, stack:{e.StackTrace}");
            }
        }

        /// <summary>
        /// 消息队列收取完成
        /// </summary>
        /// <param name="sequenceId"></param>
        public void FinishRecvSequence(int sequenceId)
        {
            ClearRequestHandler(sequenceId);
            Evt_RecvPackFinish?.Invoke(sequenceId);
        }

        /// <summary>
        /// 收到底层错误
        /// </summary>
        /// <param name="sequenceId"></param>
        /// <param name="passthrough"></param>
        /// <param name="msg"></param>
        private void RecvCommonError(int sequenceId, string passthrough, ErrorResponse msg)
        {
            string ErrReqUri = string.Empty;
            if (RequestBatchRecordMap.ContainsKey(sequenceId))
            {
                if (RequestBatchRecordMap[sequenceId].Requests.ContainsKey(passthrough))
                    ErrReqUri = RequestBatchRecordMap[sequenceId].Requests[passthrough];
            }
            ClearRequestHandler(sequenceId);
            Logger.Value?.Warn($"receive common error. sequenceId={sequenceId}, passthrough={passthrough}, errorCode={msg.Code}, errorMsg={msg.Message}, errorReq={ErrReqUri}");
        }

        private void ClearRequestHandler(int sequenceId)
        {
            if (RequestBatchRecordMap.ContainsKey(sequenceId))
            {
                var batch = RequestBatchRecordMap[sequenceId];
                var e = batch.Requests.GetEnumerator();
                while (e.MoveNext())
                {
                    RequestHandlers.Remove(e.Current.Key);
                }
                RequestBatchRecordMap.Remove(sequenceId);
                ProtokitUtil.Instance.RecycleRequestBatch(batch);
            }
        }
    }
}