using System;

namespace MTool.Framework.Network
{
    public enum EOptionMsgType
    {
        Connect,
        Disconnect,
    }

    public enum EConnectType
    {
        TCP,
        KCP,
    }

    public abstract class OptionMsg
    {
        public EOptionMsgType Type { get; protected set; }
    }

    public class ConnectMsg : OptionMsg
    {
        public bool Success;
        public EConnectType ConnectType;

        public ConnectMsg(bool _success, EConnectType connectType)
        {
            Type = EOptionMsgType.Connect;
            Success = _success;
            ConnectType = connectType;
        }
    }

    public enum DisconnectReason
    {
        Active,
        Passive,
        Exception,
    }

    public class DisconnectMsg : OptionMsg
    {
        public DisconnectReason Reason;
        public EConnectType ConnectType;

        public DisconnectMsg(DisconnectReason _reason, EConnectType connectType)
        {
            Type = EOptionMsgType.Disconnect;
            Reason = _reason;
            ConnectType = connectType;
        }
    }
}