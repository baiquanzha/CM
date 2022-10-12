using System;
using System.IO;
using System.Collections.Generic;
using gen.netutils;
using MTool.Framework.Network;
using MTool.Framework.Base;

namespace ProtokitHelper
{
    public class ProtokitMsgProcessor : Singleton<ProtokitMsgProcessor>, IMsgProcesser
    {
        private static readonly int Head_Size = 4;
        private readonly byte[] HeadBuff = new byte[Head_Size];

        public void WriteSendBytes(byte[] data, Stream stream)
        {
            stream.Position = 0;
            stream.SetLength(0);
            byte[] sendHeader = BitConverter.GetBytes(data.Length);
            stream.Write(sendHeader, 0, sendHeader.Length);
            stream.Write(data, 0, data.Length);
        }

        public bool ReadRecvBytes(Stream stream, out byte[] msgData)
        {
            msgData = new byte[0];
            if (!ReadMsgHead(stream, out int msgSize))
            {
                return false;
            }
            if (stream.Length - stream.Position < msgSize)
            {
                return false;
            }
            msgData = new byte[msgSize];
            stream.Read(msgData, 0, msgData.Length);
            return true;
        }

        private bool ReadMsgHead(Stream stream, out int msgSize)
        {
            msgSize = 0;
            if (stream.Length - stream.Position < Head_Size)
                return false;
            stream.Read(HeadBuff, 0, Head_Size);
            msgSize = BitConverter.ToInt32(HeadBuff, 0);
            return true;
        }

        public void DispatchMsg(byte[] data)
        {
            if (ProtokitClient.Instance.PacketType == ProtoPacketType.RawPacket)
            {
                RawPacket rp;
                try
                {
                    rp = RawPacket.Parser.ParseFrom(data);
                }
                catch (Exception e)
                {
                    ProtokitClient.Logger.Value?.ErrorFormat("Exception throw :{0} on parsing data, stack:{1}", e.Message, e.StackTrace);
                    return;
                }
                if (rp.Metadata.Count > 0)
                {
                    if (rp.Metadata.Count % 2 == 0)
                    {
                        Dictionary<string, string> metadata = new Dictionary<string, string>();
                        for (int i = 0; i < rp.Metadata.Count; i = i + 2)
                        {
                            string key = rp.Metadata[i];
                            string value = rp.Metadata[i + 1];
                            metadata[key] = value;
                        }
                        ProtokitClient.Instance.AddResponseMetadata(rp.SequenceID, metadata);
                    }
                    else
                        ProtokitClient.Logger.Value?.Warn("metadata key-value is not in pair.");
                }
                for (int i = 0; i < rp.RawAny.Count; i++)
                {
                    string name = rp.RawAny[i].Uri;
                    byte[] rawdata = rp.RawAny[i].Raw.ToByteArray();
                    string passthrough = rp.RawAny[i].PassThrough;
                    ProtokitClient.Instance.RecvMsg(rp.SequenceID, passthrough, name, rawdata);
                }
                ProtokitClient.Instance.FinishRecvSequence(rp.SequenceID);
                ProtokitClient.Instance.RemoveResponseMetadata(rp.SequenceID);
            }
            else if (ProtokitClient.Instance.PacketType == ProtoPacketType.TypeLengthValue)
            {
                TypeLengthValuePacket packet;
                try
                {
                    packet = TypeLengthValuePacket.ReadFrom(data);
                }
                catch (Exception e)
                {
                    ProtokitClient.Logger.Value?.ErrorFormat("Exception throw :{0} on parsing data, stack:{1}", e.Message, e.StackTrace);
                    return;
                }
                int sequenceId = (int)packet.Sequence;
                ProtokitClient.Instance.AddResponseMetadata(sequenceId, packet.Metadatas);
                for (int i = 0; i < packet.MessageList.Count; i++)
                {
                    string name = packet.MessageList[i].MsgUri;
                    byte[] rawdata = packet.MessageList[i].Payload;
                    string passthrough = packet.MessageList[i].Passthrough;
                    ProtokitClient.Instance.RecvMsg(sequenceId, passthrough, name, rawdata);
                }
                ProtokitClient.Instance.FinishRecvSequence(sequenceId);
                ProtokitClient.Instance.RemoveResponseMetadata(sequenceId);
            }
        }
    }
}