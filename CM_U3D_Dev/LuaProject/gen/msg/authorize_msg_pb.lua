-- Code generated by protokitgo. DO NOT EDIT.
-- source: msg/authorize_msg.proto

local pb = require "pb"
local protoc = require "protoc"
local protoStr = [==[syntax = "proto3";
package msg;
option go_package = "gitlab-sh.diandian.info/koala/koala-conf/gen/golang/msg";
option csharp_namespace = "gen.msg";

// 认证
message AuthorizeReq {
    string AccessToken 	= 1; 		// 平台的访问凭证
	string PublicKey	= 2;		// 客户端公钥
    int32 Version       = 3;        // 客户端版本号
    int64 ClientTime    = 4;        // 客户端utc时间
    string DistinctId   = 5;        // 游客id
    string Platform     = 6;        // 平台
    string Country      = 7;        // 国家
    string Device       = 8;        // 设备型号
    string NetworkOperator = 9;     // 网络运营商
    string timezone        = 10;    // 时区
    int32  NoticeStatus  = 11;      // 通知开启状态
}

message AuthorizeRsp {
	string PublicKey = 2;	    // 服务端公钥
    string SecretKey = 3;       // 存档的秘钥
    string Token     = 4;       // token
}
]==]
assert(protoc:load(protoStr,"msg/authorize_msg.proto"))

---@class AuthorizeReq
---@field public AccessToken string
---@field public PublicKey string
---@field public Version number
---@field public ClientTime number
---@field public DistinctId string
---@field public Platform string
---@field public Country string
---@field public Device string
---@field public NetworkOperator string
---@field public timezone string
---@field public NoticeStatus number
local AuthorizeReq={}
AuthorizeReq.__index = AuthorizeReq
---@return AuthorizeReq
function AuthorizeReq:new(data) return setmetatable(data or {},AuthorizeReq)  end
---@return AuthorizeReq
function AuthorizeReq:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},AuthorizeReq)  end
---@return string
function AuthorizeReq:getMessageName() return "msg.AuthorizeReq" end
---@return string
function AuthorizeReq:marshal()  return pb.encode(self:getMessageName(),self) end
msg.AuthorizeReq = AuthorizeReq

---@class AuthorizeRsp
---@field public Player Player
---@field public PublicKey string
---@field public SecretKey string
---@field public Token string
---@field public BeginnerSignIn BeginnerSignIn
---@field public BundleList BundleList[]
---@field public MergeContests MergeContest[]
---@field public Group number
---@field public Timezone string
---@field public TotalPay number
---@field public MergeContestId number
local AuthorizeRsp={}
AuthorizeRsp.__index = AuthorizeRsp
---@return AuthorizeRsp
function AuthorizeRsp:new(data) return setmetatable(data or {},AuthorizeRsp)  end
---@return AuthorizeRsp
function AuthorizeRsp:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},AuthorizeRsp)  end
---@return string
function AuthorizeRsp:getMessageName() return "msg.AuthorizeRsp" end
---@return string
function AuthorizeRsp:marshal()  return pb.encode(self:getMessageName(),self) end
msg.AuthorizeRsp = AuthorizeRsp

---@class MergeContest
---@field public Id number
---@field public BeginTime number
---@field public IsReward boolean
---@field public Round number
local MergeContest={}
MergeContest.__index = MergeContest
---@return MergeContest
function MergeContest:new(data) return setmetatable(data or {},MergeContest)  end
---@return MergeContest
function MergeContest:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},MergeContest)  end
---@return string
function MergeContest:getMessageName() return "msg.MergeContest" end
---@return string
function MergeContest:marshal()  return pb.encode(self:getMessageName(),self) end
msg.MergeContest = MergeContest

---@class BeginnerSignIn
---@field public IsSignedInToday boolean
---@field public BeginnerSignInDays number
local BeginnerSignIn={}
BeginnerSignIn.__index = BeginnerSignIn
---@return BeginnerSignIn
function BeginnerSignIn:new(data) return setmetatable(data or {},BeginnerSignIn)  end
---@return BeginnerSignIn
function BeginnerSignIn:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},BeginnerSignIn)  end
---@return string
function BeginnerSignIn:getMessageName() return "msg.BeginnerSignIn" end
---@return string
function BeginnerSignIn:marshal()  return pb.encode(self:getMessageName(),self) end
msg.BeginnerSignIn = BeginnerSignIn

---@class BundleList
---@field public BundleId number
---@field public Limit number
---@field public BeginTime number
---@field public TotalBuyTimes number
local BundleList={}
BundleList.__index = BundleList
---@return BundleList
function BundleList:new(data) return setmetatable(data or {},BundleList)  end
---@return BundleList
function BundleList:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},BundleList)  end
---@return string
function BundleList:getMessageName() return "msg.BundleList" end
---@return string
function BundleList:marshal()  return pb.encode(self:getMessageName(),self) end
msg.BundleList = BundleList
