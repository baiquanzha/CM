// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: msg/gate.proto
// </auto-generated>
#pragma warning disable 1591, 0612, 3021
#region Designer generated code

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;
namespace gen.msg {

  /// <summary>Holder for reflection information generated from msg/gate.proto</summary>
  public static partial class GateReflection {

    #region Descriptor
    /// <summary>File descriptor for msg/gate.proto</summary>
    public static pbr::FileDescriptor Descriptor {
      get { return descriptor; }
    }
    private static pbr::FileDescriptor descriptor;

    static GateReflection() {
      byte[] descriptorData = global::System.Convert.FromBase64String(
          string.Concat(
            "Cg5tc2cvZ2F0ZS5wcm90bxIDbXNnIisKDU1hcEZpZWxkRW50cnkSCwoDa2V5",
            "GAEgASgJEg0KBXZhbHVlGAIgASgJIv8BCglIYW5kc2hha2USGgoDQ21kGAEg",
            "ASgOMg0ubXNnLkdDbWRUeXBlEhoKA1NyYxgCIAEoDjINLm1zZy5HU3JjVHlw",
            "ZRITCgtTZXJ2aWNlTmFtZRgDIAEoCRIQCghTZXJ2ZXJJZBgEIAEoCRIQCghT",
            "ZXJ2ZXJJUBgFIAEoCRISCgpTZXJ2ZXJQb3J0GAYgASgFEhIKCkNsaWVudEFk",
            "ZHIYByABKAkSDgoGVXNlcklEGAggASgJEhEKCUNsaWVudFZlchgJIAEoCRIU",
            "CgxDbGllbnRSZXNWZXIYCiABKAkSIAoETWV0YRgLIAMoCzISLm1zZy5NYXBG",
            "aWVsZEVudHJ5IjwKDUhhbmRzaGFrZVJlc3ASHQoEQ29kZRgBIAEoDjIPLm1z",
            "Zy5HRXJyb3JDb2RlEgwKBERlc2MYAiABKAkqIQoIR0NtZFR5cGUSCAoER05F",
            "VxAAEgsKB0dSRUNPTk4QASoiCghHU3JjVHlwZRILCgdHQ0xJRU5UEAASCQoF",
            "R0dBVEUQASqEAQoKR0Vycm9yQ29kZRIMCghHU3VjY2VzcxAAEhIKDUdVbmF1",
            "dGhvcml6ZWQQkQMSDwoKR0ZvcmJpZGRlbhCTAxIOCglHTm90Rm91bmQQlAMS",
            "GQoUR0ludGVybmFsU2VydmVyRXJyb3IQ9AMSGAoTR1NlcnZpY2VVbmF2YWls",
            "YWJsZRD3A0I+WjJiaXRidWNrZXQub3JnL2Z1bnBsdXMvZ2F0ZS9wcm90b2Nv",
            "bC9nZW4vZ29sYW5nL21zZ6oCB2dlbi5tc2diBnByb3RvMw=="));
      descriptor = pbr::FileDescriptor.FromGeneratedCode(descriptorData,
          new pbr::FileDescriptor[] { },
          new pbr::GeneratedClrTypeInfo(new[] {typeof(global::gen.msg.GCmdType), typeof(global::gen.msg.GSrcType), typeof(global::gen.msg.GErrorCode), }, null, new pbr::GeneratedClrTypeInfo[] {
            new pbr::GeneratedClrTypeInfo(typeof(global::gen.msg.MapFieldEntry), global::gen.msg.MapFieldEntry.Parser, new[]{ "Key", "Value" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::gen.msg.Handshake), global::gen.msg.Handshake.Parser, new[]{ "Cmd", "Src", "ServiceName", "ServerId", "ServerIP", "ServerPort", "ClientAddr", "UserID", "ClientVer", "ClientResVer", "Meta" }, null, null, null, null),
            new pbr::GeneratedClrTypeInfo(typeof(global::gen.msg.HandshakeResp), global::gen.msg.HandshakeResp.Parser, new[]{ "Code", "Desc" }, null, null, null, null)
          }));
    }
    #endregion

  }
  #region Enums
  public enum GCmdType {
    /// <summary>
    /// new connect
    /// </summary>
    [pbr::OriginalName("GNEW")] Gnew = 0,
    /// <summary>
    /// re connect
    /// </summary>
    [pbr::OriginalName("GRECONN")] Greconn = 1,
  }

  public enum GSrcType {
    /// <summary>
    /// client direct
    /// </summary>
    [pbr::OriginalName("GCLIENT")] Gclient = 0,
    /// <summary>
    /// gate forward
    /// </summary>
    [pbr::OriginalName("GGATE")] Ggate = 1,
  }

  public enum GErrorCode {
    [pbr::OriginalName("GSuccess")] Gsuccess = 0,
    [pbr::OriginalName("GUnauthorized")] Gunauthorized = 401,
    [pbr::OriginalName("GForbidden")] Gforbidden = 403,
    [pbr::OriginalName("GNotFound")] GnotFound = 404,
    [pbr::OriginalName("GInternalServerError")] GinternalServerError = 500,
    [pbr::OriginalName("GServiceUnavailable")] GserviceUnavailable = 503,
  }

  #endregion

  #region Messages
  public sealed partial class MapFieldEntry : pb::IMessage<MapFieldEntry> {
    private static readonly pb::MessageParser<MapFieldEntry> _parser = new pb::MessageParser<MapFieldEntry>(() => new MapFieldEntry());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<MapFieldEntry> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::gen.msg.GateReflection.Descriptor.MessageTypes[0]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapFieldEntry() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapFieldEntry(MapFieldEntry other) : this() {
      key_ = other.key_;
      value_ = other.value_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public MapFieldEntry Clone() {
      return new MapFieldEntry(this);
    }

    /// <summary>Field number for the "key" field.</summary>
    public const int KeyFieldNumber = 1;
    private string key_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Key {
      get { return key_; }
      set {
        key_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "value" field.</summary>
    public const int ValueFieldNumber = 2;
    private string value_ = "";
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Value {
      get { return value_; }
      set {
        value_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as MapFieldEntry);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(MapFieldEntry other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Key != other.Key) return false;
      if (Value != other.Value) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Key.Length != 0) hash ^= Key.GetHashCode();
      if (Value.Length != 0) hash ^= Value.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Key.Length != 0) {
        output.WriteRawTag(10);
        output.WriteString(Key);
      }
      if (Value.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Value);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Key.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Key);
      }
      if (Value.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Value);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(MapFieldEntry other) {
      if (other == null) {
        return;
      }
      if (other.Key.Length != 0) {
        Key = other.Key;
      }
      if (other.Value.Length != 0) {
        Value = other.Value;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 10: {
            Key = input.ReadString();
            break;
          }
          case 18: {
            Value = input.ReadString();
            break;
          }
        }
      }
    }

  }

  public sealed partial class Handshake : pb::IMessage<Handshake> {
    private static readonly pb::MessageParser<Handshake> _parser = new pb::MessageParser<Handshake>(() => new Handshake());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<Handshake> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::gen.msg.GateReflection.Descriptor.MessageTypes[1]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Handshake() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Handshake(Handshake other) : this() {
      cmd_ = other.cmd_;
      src_ = other.src_;
      serviceName_ = other.serviceName_;
      serverId_ = other.serverId_;
      serverIP_ = other.serverIP_;
      serverPort_ = other.serverPort_;
      clientAddr_ = other.clientAddr_;
      userID_ = other.userID_;
      clientVer_ = other.clientVer_;
      clientResVer_ = other.clientResVer_;
      meta_ = other.meta_.Clone();
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public Handshake Clone() {
      return new Handshake(this);
    }

    /// <summary>Field number for the "Cmd" field.</summary>
    public const int CmdFieldNumber = 1;
    private global::gen.msg.GCmdType cmd_ = global::gen.msg.GCmdType.Gnew;
    /// <summary>
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::gen.msg.GCmdType Cmd {
      get { return cmd_; }
      set {
        cmd_ = value;
      }
    }

    /// <summary>Field number for the "Src" field.</summary>
    public const int SrcFieldNumber = 2;
    private global::gen.msg.GSrcType src_ = global::gen.msg.GSrcType.Gclient;
    /// <summary>
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::gen.msg.GSrcType Src {
      get { return src_; }
      set {
        src_ = value;
      }
    }

    /// <summary>Field number for the "ServiceName" field.</summary>
    public const int ServiceNameFieldNumber = 3;
    private string serviceName_ = "";
    /// <summary>
    /// Service name（new connection OR stateless service）
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServiceName {
      get { return serviceName_; }
      set {
        serviceName_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ServerId" field.</summary>
    public const int ServerIdFieldNumber = 4;
    private string serverId_ = "";
    /// <summary>
    /// Used when reconnecting stateful services
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServerId {
      get { return serverId_; }
      set {
        serverId_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ServerIP" field.</summary>
    public const int ServerIPFieldNumber = 5;
    private string serverIP_ = "";
    /// <summary>
    /// Backend ip
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ServerIP {
      get { return serverIP_; }
      set {
        serverIP_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ServerPort" field.</summary>
    public const int ServerPortFieldNumber = 6;
    private int serverPort_;
    /// <summary>
    /// Backend port
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int ServerPort {
      get { return serverPort_; }
      set {
        serverPort_ = value;
      }
    }

    /// <summary>Field number for the "ClientAddr" field.</summary>
    public const int ClientAddrFieldNumber = 7;
    private string clientAddr_ = "";
    /// <summary>
    /// The address of the client when the gate forward
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientAddr {
      get { return clientAddr_; }
      set {
        clientAddr_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "UserID" field.</summary>
    public const int UserIDFieldNumber = 8;
    private string userID_ = "";
    /// <summary>
    /// Userid used for actor
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string UserID {
      get { return userID_; }
      set {
        userID_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ClientVer" field.</summary>
    public const int ClientVerFieldNumber = 9;
    private string clientVer_ = "";
    /// <summary>
    /// client version
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientVer {
      get { return clientVer_; }
      set {
        clientVer_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "ClientResVer" field.</summary>
    public const int ClientResVerFieldNumber = 10;
    private string clientResVer_ = "";
    /// <summary>
    /// client resource version
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string ClientResVer {
      get { return clientResVer_; }
      set {
        clientResVer_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    /// <summary>Field number for the "Meta" field.</summary>
    public const int MetaFieldNumber = 11;
    private static readonly pb::FieldCodec<global::gen.msg.MapFieldEntry> _repeated_meta_codec
        = pb::FieldCodec.ForMessage(90, global::gen.msg.MapFieldEntry.Parser);
    private readonly pbc::RepeatedField<global::gen.msg.MapFieldEntry> meta_ = new pbc::RepeatedField<global::gen.msg.MapFieldEntry>();
    /// <summary>
    /// custom meta
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public pbc::RepeatedField<global::gen.msg.MapFieldEntry> Meta {
      get { return meta_; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as Handshake);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(Handshake other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Cmd != other.Cmd) return false;
      if (Src != other.Src) return false;
      if (ServiceName != other.ServiceName) return false;
      if (ServerId != other.ServerId) return false;
      if (ServerIP != other.ServerIP) return false;
      if (ServerPort != other.ServerPort) return false;
      if (ClientAddr != other.ClientAddr) return false;
      if (UserID != other.UserID) return false;
      if (ClientVer != other.ClientVer) return false;
      if (ClientResVer != other.ClientResVer) return false;
      if(!meta_.Equals(other.meta_)) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Cmd != global::gen.msg.GCmdType.Gnew) hash ^= Cmd.GetHashCode();
      if (Src != global::gen.msg.GSrcType.Gclient) hash ^= Src.GetHashCode();
      if (ServiceName.Length != 0) hash ^= ServiceName.GetHashCode();
      if (ServerId.Length != 0) hash ^= ServerId.GetHashCode();
      if (ServerIP.Length != 0) hash ^= ServerIP.GetHashCode();
      if (ServerPort != 0) hash ^= ServerPort.GetHashCode();
      if (ClientAddr.Length != 0) hash ^= ClientAddr.GetHashCode();
      if (UserID.Length != 0) hash ^= UserID.GetHashCode();
      if (ClientVer.Length != 0) hash ^= ClientVer.GetHashCode();
      if (ClientResVer.Length != 0) hash ^= ClientResVer.GetHashCode();
      hash ^= meta_.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Cmd != global::gen.msg.GCmdType.Gnew) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Cmd);
      }
      if (Src != global::gen.msg.GSrcType.Gclient) {
        output.WriteRawTag(16);
        output.WriteEnum((int) Src);
      }
      if (ServiceName.Length != 0) {
        output.WriteRawTag(26);
        output.WriteString(ServiceName);
      }
      if (ServerId.Length != 0) {
        output.WriteRawTag(34);
        output.WriteString(ServerId);
      }
      if (ServerIP.Length != 0) {
        output.WriteRawTag(42);
        output.WriteString(ServerIP);
      }
      if (ServerPort != 0) {
        output.WriteRawTag(48);
        output.WriteInt32(ServerPort);
      }
      if (ClientAddr.Length != 0) {
        output.WriteRawTag(58);
        output.WriteString(ClientAddr);
      }
      if (UserID.Length != 0) {
        output.WriteRawTag(66);
        output.WriteString(UserID);
      }
      if (ClientVer.Length != 0) {
        output.WriteRawTag(74);
        output.WriteString(ClientVer);
      }
      if (ClientResVer.Length != 0) {
        output.WriteRawTag(82);
        output.WriteString(ClientResVer);
      }
      meta_.WriteTo(output, _repeated_meta_codec);
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Cmd != global::gen.msg.GCmdType.Gnew) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Cmd);
      }
      if (Src != global::gen.msg.GSrcType.Gclient) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Src);
      }
      if (ServiceName.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServiceName);
      }
      if (ServerId.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServerId);
      }
      if (ServerIP.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ServerIP);
      }
      if (ServerPort != 0) {
        size += 1 + pb::CodedOutputStream.ComputeInt32Size(ServerPort);
      }
      if (ClientAddr.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientAddr);
      }
      if (UserID.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(UserID);
      }
      if (ClientVer.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientVer);
      }
      if (ClientResVer.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(ClientResVer);
      }
      size += meta_.CalculateSize(_repeated_meta_codec);
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(Handshake other) {
      if (other == null) {
        return;
      }
      if (other.Cmd != global::gen.msg.GCmdType.Gnew) {
        Cmd = other.Cmd;
      }
      if (other.Src != global::gen.msg.GSrcType.Gclient) {
        Src = other.Src;
      }
      if (other.ServiceName.Length != 0) {
        ServiceName = other.ServiceName;
      }
      if (other.ServerId.Length != 0) {
        ServerId = other.ServerId;
      }
      if (other.ServerIP.Length != 0) {
        ServerIP = other.ServerIP;
      }
      if (other.ServerPort != 0) {
        ServerPort = other.ServerPort;
      }
      if (other.ClientAddr.Length != 0) {
        ClientAddr = other.ClientAddr;
      }
      if (other.UserID.Length != 0) {
        UserID = other.UserID;
      }
      if (other.ClientVer.Length != 0) {
        ClientVer = other.ClientVer;
      }
      if (other.ClientResVer.Length != 0) {
        ClientResVer = other.ClientResVer;
      }
      meta_.Add(other.meta_);
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Cmd = (global::gen.msg.GCmdType) input.ReadEnum();
            break;
          }
          case 16: {
            Src = (global::gen.msg.GSrcType) input.ReadEnum();
            break;
          }
          case 26: {
            ServiceName = input.ReadString();
            break;
          }
          case 34: {
            ServerId = input.ReadString();
            break;
          }
          case 42: {
            ServerIP = input.ReadString();
            break;
          }
          case 48: {
            ServerPort = input.ReadInt32();
            break;
          }
          case 58: {
            ClientAddr = input.ReadString();
            break;
          }
          case 66: {
            UserID = input.ReadString();
            break;
          }
          case 74: {
            ClientVer = input.ReadString();
            break;
          }
          case 82: {
            ClientResVer = input.ReadString();
            break;
          }
          case 90: {
            meta_.AddEntriesFrom(input, _repeated_meta_codec);
            break;
          }
        }
      }
    }

  }

  public sealed partial class HandshakeResp : pb::IMessage<HandshakeResp> {
    private static readonly pb::MessageParser<HandshakeResp> _parser = new pb::MessageParser<HandshakeResp>(() => new HandshakeResp());
    private pb::UnknownFieldSet _unknownFields;
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pb::MessageParser<HandshakeResp> Parser { get { return _parser; } }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public static pbr::MessageDescriptor Descriptor {
      get { return global::gen.msg.GateReflection.Descriptor.MessageTypes[2]; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    pbr::MessageDescriptor pb::IMessage.Descriptor {
      get { return Descriptor; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HandshakeResp() {
      OnConstruction();
    }

    partial void OnConstruction();

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HandshakeResp(HandshakeResp other) : this() {
      code_ = other.code_;
      desc_ = other.desc_;
      _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public HandshakeResp Clone() {
      return new HandshakeResp(this);
    }

    /// <summary>Field number for the "Code" field.</summary>
    public const int CodeFieldNumber = 1;
    private global::gen.msg.GErrorCode code_ = global::gen.msg.GErrorCode.Gsuccess;
    /// <summary>
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public global::gen.msg.GErrorCode Code {
      get { return code_; }
      set {
        code_ = value;
      }
    }

    /// <summary>Field number for the "Desc" field.</summary>
    public const int DescFieldNumber = 2;
    private string desc_ = "";
    /// <summary>
    /// success or err desc
    /// </summary>
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public string Desc {
      get { return desc_; }
      set {
        desc_ = pb::ProtoPreconditions.CheckNotNull(value, "value");
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override bool Equals(object other) {
      return Equals(other as HandshakeResp);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public bool Equals(HandshakeResp other) {
      if (ReferenceEquals(other, null)) {
        return false;
      }
      if (ReferenceEquals(other, this)) {
        return true;
      }
      if (Code != other.Code) return false;
      if (Desc != other.Desc) return false;
      return Equals(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override int GetHashCode() {
      int hash = 1;
      if (Code != global::gen.msg.GErrorCode.Gsuccess) hash ^= Code.GetHashCode();
      if (Desc.Length != 0) hash ^= Desc.GetHashCode();
      if (_unknownFields != null) {
        hash ^= _unknownFields.GetHashCode();
      }
      return hash;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public override string ToString() {
      return pb::JsonFormatter.ToDiagnosticString(this);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void WriteTo(pb::CodedOutputStream output) {
      if (Code != global::gen.msg.GErrorCode.Gsuccess) {
        output.WriteRawTag(8);
        output.WriteEnum((int) Code);
      }
      if (Desc.Length != 0) {
        output.WriteRawTag(18);
        output.WriteString(Desc);
      }
      if (_unknownFields != null) {
        _unknownFields.WriteTo(output);
      }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public int CalculateSize() {
      int size = 0;
      if (Code != global::gen.msg.GErrorCode.Gsuccess) {
        size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) Code);
      }
      if (Desc.Length != 0) {
        size += 1 + pb::CodedOutputStream.ComputeStringSize(Desc);
      }
      if (_unknownFields != null) {
        size += _unknownFields.CalculateSize();
      }
      return size;
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(HandshakeResp other) {
      if (other == null) {
        return;
      }
      if (other.Code != global::gen.msg.GErrorCode.Gsuccess) {
        Code = other.Code;
      }
      if (other.Desc.Length != 0) {
        Desc = other.Desc;
      }
      _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    public void MergeFrom(pb::CodedInputStream input) {
      uint tag;
      while ((tag = input.ReadTag()) != 0) {
        switch(tag) {
          default:
            _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
            break;
          case 8: {
            Code = (global::gen.msg.GErrorCode) input.ReadEnum();
            break;
          }
          case 18: {
            Desc = input.ReadString();
            break;
          }
        }
      }
    }

  }

  #endregion

}

#endregion Designer generated code
