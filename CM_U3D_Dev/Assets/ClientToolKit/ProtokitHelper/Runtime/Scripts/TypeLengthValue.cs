using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Google.Protobuf;
using MTool.Core.Primitives;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace ProtokitHelper
{
    public class TypeLengthValuePacket
    {
        public byte Magic = 0xCC;
        public byte Ver = 1;
        public ushort Mask;
        public uint Sequence;
        public Dictionary<string, string> Metadatas = new Dictionary<string, string>();
        public List<TypeLengthValueRawProto> MessageList = new List<TypeLengthValueRawProto>();

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("TypeLengthValuePacket"));

        public void SetMetaData(string key, string value)
        {
            Metadatas[key] = value;
        }

        public void AddProto(TypeLengthValueRawProto proto)
        {
            MessageList.Add(proto);
        }

        /// <summary>
        /// 写入发送字节，使用小端字节序
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public bool WriteTo(Stream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
            stream.WriteByte(Magic);
            stream.WriteByte(Ver);
            byte[] maskBytes = BitConverter.GetBytes(Mask);
            if (!BitConverter.IsLittleEndian)
                TypeLengthValueUtils.Instance.ReverseBytes(maskBytes);
            stream.Write(maskBytes, 0, maskBytes.Length);
            byte[] sequenceBytes = BitConverter.GetBytes(Sequence);
            if (!BitConverter.IsLittleEndian)
                TypeLengthValueUtils.Instance.ReverseBytes(sequenceBytes);
            stream.Write(sequenceBytes, 0, sequenceBytes.Length);
            //写入Metadata数据
            int mSize = 0;
            var e = Metadatas.GetEnumerator();
            while (e.MoveNext())
            {
                //2字节保存key字符串长度
                mSize += 2;
                mSize += e.Current.Key.Length;
                //2字节保存value字符串长度
                mSize += 2;
                mSize += e.Current.Value.Length;
            }
            if (mSize > ushort.MaxValue)
            {
                s_mLogger.Value?.Error($"TypeLengthValuePacket write error: metadata overflow, size={mSize}");
                return false;
            }
            ushort metadataLength = (ushort)mSize;
            byte[] metadataLengthBytes = BitConverter.GetBytes(metadataLength);
            if (!BitConverter.IsLittleEndian)
                TypeLengthValueUtils.Instance.ReverseBytes(metadataLengthBytes);
            stream.Write(metadataLengthBytes, 0, metadataLengthBytes.Length);
            e = Metadatas.GetEnumerator();
            while (e.MoveNext())
            {
                if (e.Current.Key.Length > ushort.MaxValue)
                {
                    s_mLogger.Value?.Error($"TypeLengthValuePacket write error: metadata key overflow, key string = {e.Current.Key}");
                    return false;
                }
                if (e.Current.Value.Length > ushort.MaxValue)
                {
                    s_mLogger.Value?.Error($"TypeLengthValuePacket write error: metadata value overflow, value string = {e.Current.Value}");
                    return false;
                }
                ushort keyLength = (ushort)e.Current.Key.Length;
                byte[] keyLengthBytes = BitConverter.GetBytes(keyLength);
                if (!BitConverter.IsLittleEndian)
                    TypeLengthValueUtils.Instance.ReverseBytes(keyLengthBytes);
                stream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
                byte[] keyBytes = Encoding.UTF8.GetBytes(e.Current.Key);
                stream.Write(keyBytes, 0, keyBytes.Length);
                ushort valueLength = (ushort)e.Current.Value.Length;
                byte[] valueLengthBytes = BitConverter.GetBytes(valueLength);
                if (!BitConverter.IsLittleEndian)
                    TypeLengthValueUtils.Instance.ReverseBytes(valueLengthBytes);
                stream.Write(valueLengthBytes, 0, valueLengthBytes.Length);
                byte[] valueBytes = Encoding.UTF8.GetBytes(e.Current.Value);
                stream.Write(valueBytes, 0, valueBytes.Length);
            }
            //写入Message数据
            mSize = 0;
            for (int i = 0; i < MessageList.Count; i++)
            {
                //1字节uri长度
                mSize += 1;
                mSize += MessageList[i].MsgUri.Length;
                //1字节passthrough长度
                mSize += 1;
                mSize += MessageList[i].Passthrough.Length;
                //2字节payload长度
                mSize += 2;
                mSize += MessageList[i].Payload.Length;
            }
            if (mSize > ushort.MaxValue)
            {
                s_mLogger.Value?.Error($"TypeLengthValuePacket write error: metadata overflow, size={mSize}");
                return false;
            }
            ushort messageLength = (ushort)mSize;
            byte[] messageLengthBytes = BitConverter.GetBytes(messageLength);
            if (!BitConverter.IsLittleEndian)
                TypeLengthValueUtils.Instance.ReverseBytes(messageLengthBytes);
            stream.Write(messageLengthBytes, 0, messageLengthBytes.Length);
            for (int i = 0; i < MessageList.Count; i++)
            {
                TypeLengthValueRawProto rawProto = MessageList[i];
                if (rawProto.MsgUri.Length <= byte.MaxValue + 1)
                {
                    byte uriLegnth = (byte)rawProto.MsgUri.Length;
                    stream.WriteByte(uriLegnth);
                    byte[] uriBytes = Encoding.UTF8.GetBytes(rawProto.MsgUri);
                    stream.Write(uriBytes, 0, uriBytes.Length);
                }
                else
                {
                    s_mLogger.Value?.Error($"TypeLengthValuePacket write error: proto uri overflow, uri={rawProto.MsgUri}");
                    return false;
                }
                if (rawProto.Passthrough.Length <= byte.MaxValue + 1)
                {
                    byte pLength = (byte)rawProto.Passthrough.Length;
                    stream.WriteByte(pLength);
                    byte[] passthroughBytes = Encoding.UTF8.GetBytes(rawProto.Passthrough);
                    stream.Write(passthroughBytes, 0, passthroughBytes.Length);
                }
                else
                {
                    s_mLogger.Value?.Error($"TypeLengthValuePacket write error: proto passthrough overflow, passthrough={rawProto.Passthrough}");
                    return false;
                }
                if (rawProto.Payload.Length <= ushort.MaxValue + 1)
                {
                    ushort payloadLength = (ushort)rawProto.Payload.Length;
                    byte[] payloadLengthBytes = BitConverter.GetBytes(payloadLength);
                    if (!BitConverter.IsLittleEndian)
                        TypeLengthValueUtils.Instance.ReverseBytes(payloadLengthBytes);
                    stream.Write(payloadLengthBytes, 0, payloadLengthBytes.Length);
                    stream.Write(rawProto.Payload, 0, rawProto.Payload.Length);
                }
                else
                {
                    s_mLogger.Value?.Error($"TypeLengthValuePacket write error: proto payload overflow, size={rawProto.Payload.Length}, uri={rawProto.MsgUri}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 读取收到的字节，使用小端字节序
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static TypeLengthValuePacket ReadFrom(byte[] data)
        {
            TypeLengthValuePacket packet = new TypeLengthValuePacket();
            int readPos = 0;
            packet.Magic = data[readPos];
            readPos++;
            packet.Ver = data[readPos];
            readPos++;
            packet.Mask = BitConverter.ToUInt16(data, readPos);
            if (!BitConverter.IsLittleEndian)
                packet.Mask = TypeLengthValueUtils.Instance.ReverseUshort(packet.Mask);
            readPos += sizeof(ushort);
            packet.Sequence = BitConverter.ToUInt32(data, readPos);
            if (!BitConverter.IsLittleEndian)
                packet.Sequence = TypeLengthValueUtils.Instance.ReverseUint(packet.Sequence);
            readPos += sizeof(uint);
            //读取metadata数据
            ushort metadataLength = BitConverter.ToUInt16(data, readPos);
            if (!BitConverter.IsLittleEndian)
                metadataLength = TypeLengthValueUtils.Instance.ReverseUshort(metadataLength);
            readPos += sizeof(ushort);
            ushort metadataReadLength = 0;
            while (metadataReadLength < metadataLength)
            {
                ushort keyLength = BitConverter.ToUInt16(data, readPos);
                if (!BitConverter.IsLittleEndian)
                    keyLength = TypeLengthValueUtils.Instance.ReverseUshort(keyLength);
                readPos += sizeof(ushort);
                metadataReadLength += sizeof(ushort);
                string key = Encoding.UTF8.GetString(data, readPos, keyLength);
                readPos += keyLength;
                metadataReadLength += keyLength;
                ushort valueLength = BitConverter.ToUInt16(data, readPos);
                if (!BitConverter.IsLittleEndian)
                    valueLength = TypeLengthValueUtils.Instance.ReverseUshort(valueLength);
                readPos += sizeof(ushort);
                metadataReadLength += sizeof(ushort);
                string value = Encoding.UTF8.GetString(data, readPos, valueLength);
                readPos += valueLength;
                metadataReadLength += valueLength;
                packet.Metadatas.Add(key, value);
            }
            //读取message数据
            ushort messageLength = BitConverter.ToUInt16(data, readPos);
            if (!BitConverter.IsLittleEndian)
                messageLength = TypeLengthValueUtils.Instance.ReverseUshort(messageLength);
            readPos += sizeof(ushort);
            ushort messageReadlength = 0;
            while (messageReadlength < messageLength)
            {
                TypeLengthValueRawProto rawProto = new TypeLengthValueRawProto();
                byte uriLength = data[readPos];
                readPos += sizeof(byte);
                messageReadlength += sizeof(byte);
                rawProto.MsgUri = Encoding.UTF8.GetString(data, readPos, uriLength);
                readPos += uriLength;
                messageReadlength += uriLength;
                byte passthroughLength = data[readPos];
                readPos += sizeof(byte);
                messageReadlength += sizeof(byte);
                rawProto.Passthrough = Encoding.UTF8.GetString(data, readPos, passthroughLength);
                readPos += passthroughLength;
                messageReadlength += passthroughLength;
                ushort payloadLength = BitConverter.ToUInt16(data, readPos);
                if (!BitConverter.IsLittleEndian)
                    payloadLength = TypeLengthValueUtils.Instance.ReverseUshort(payloadLength);
                readPos += sizeof(ushort);
                messageReadlength += sizeof(ushort);
                rawProto.Payload = new byte[payloadLength];
                Buffer.BlockCopy(data, readPos, rawProto.Payload, 0, payloadLength);
                readPos += payloadLength;
                messageReadlength += payloadLength;
                packet.MessageList.Add(rawProto);
            }
            return packet;
        }
    }

    public class TypeLengthValueRawProto
    {
        public string MsgUri;
        public byte[] Payload;
        public string Passthrough;
        public string ProtoLog;
        public Dictionary<string, string> Metadata;
    }

    public class TypeLengthValueUtils : Singleton<TypeLengthValueUtils>
    {
        private readonly DateTime StartTime = new DateTime(1970, 1, 1);
        private TimeSpan timeSpan = new TimeSpan();
        private uint requestCount = 0;
        private uint sequenceCount = 0;
        private const byte COMPRESS_GZIP = 0x40;
        private const byte COMPRESS_SNAPPY = 0x80;
        private const byte SERIALIZE_JSON = 0x10;
        private const byte SERIALIZE_PROTOBUF = 0x20;
        private TypeLengthValueCompressType compressType = TypeLengthValueCompressType.NoCompress;
        private TypeLengthValueSerializeType serializeType = TypeLengthValueSerializeType.Protobuf;
        
        //public void SetCompress(TypeLengthValueCompressType compress)
        //{
        //    compressType = compress;
        //}

        //public void SetSerialize(TypeLengthValueSerializeType serialize)
        //{
        //    serializeType = serialize;
        //}

        public TypeLengthValueRawProto GenTypeLengthValueRawProto(IMessage msg, Dictionary<string, string> metadata = null)
        {
            using (var stream = new MemoryStream())
            {
                msg.WriteTo(stream);
                var rawProto = new TypeLengthValueRawProto
                {
                    MsgUri = msg.Descriptor.FullName,
                    Payload = stream.ToArray(),
                    Passthrough = NextPassthrough()
                };
                if (metadata != null && metadata.Count > 0)
                {
                    rawProto.Metadata = metadata;
                    rawProto.ProtoLog = $"name:{msg.Descriptor.FullName}, body:{msg}, metadata content:{GetMetadataString(metadata)}";
                }
                else
                    rawProto.ProtoLog = $"name:{msg.Descriptor.FullName}, body:{msg}";
                return rawProto;
            }
        }

        private StringBuilder sb = new StringBuilder();
        private string GetMetadataString(Dictionary<string, string> metadata)
        {
            sb.Length = 0;
            sb.Append("{");
            sb.Append($"count is {metadata.Count}");
            var e = metadata.GetEnumerator();
            while (e.MoveNext())
            {
                sb.Append(", k-v pair:[");
                sb.Append($"{e.Current.Key}:{e.Current.Value}");
                sb.Append("]");
            }
            sb.Append("}");
            return sb.ToString();
        }

        public string NextPassthrough()
        {
            timeSpan = (DateTime.UtcNow - StartTime);
            return string.Concat(timeSpan.ToString(), "_", NextRequestId().ToString());
        }

        public uint NextRequestId()
        {
            if (requestCount >= int.MaxValue)
                requestCount = 1;
            requestCount++;
            return requestCount;
        }

        public uint GetSequenceId()
        {
            if (sequenceCount >= int.MaxValue)
                sequenceCount = 1;
            sequenceCount++;
            return sequenceCount;
        }

        public TypeLengthValuePacket GenTypeLengthValuePacket()
        {
            TypeLengthValuePacket packet = new TypeLengthValuePacket();
            if (compressType == TypeLengthValueCompressType.Gzip)
                packet.Mask |= COMPRESS_GZIP;
            else if (compressType == TypeLengthValueCompressType.Snappy)
                packet.Mask |= COMPRESS_SNAPPY;
            if (serializeType == TypeLengthValueSerializeType.Json)
                packet.Mask |= SERIALIZE_JSON;
            else if (serializeType == TypeLengthValueSerializeType.Protobuf)
                packet.Mask |= SERIALIZE_PROTOBUF;
            packet.Sequence = GetSequenceId();
            return packet;
        }

        /// <summary>
        /// 翻转byte数组字节序
        /// </summary>
        /// <param name="bytes"></param>
        public void ReverseBytes(byte[] bytes)
        {
            byte tmp;
            int len = bytes.Length;
            for (int i = 0; i < len / 2; i++)
            {
                tmp = bytes[len - 1 - i];
                bytes[len - 1 - i] = bytes[i];
                bytes[i] = tmp;
            }
        }

        /// <summary>
        /// 翻转ushort字节序
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ushort ReverseUshort(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        /// <summary>
        /// 翻转uint字节序
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public uint ReverseUint(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 | (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        /// <summary>
        /// 翻转ulong字节序
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ulong ReverseUlong(ulong value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }
    }

    public enum TypeLengthValueCompressType
    {
        NoCompress,
        Gzip,
        Snappy,
    }

    public enum TypeLengthValueSerializeType
    {
        Json,
        Protobuf,
    }

    public enum ProtoPacketType
    {
        RawPacket,
        TypeLengthValue,
    }
}