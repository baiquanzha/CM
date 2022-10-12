-- Code generated by protokitgo. DO NOT EDIT.
-- source: netutils/cmds.proto

local pb = require "pb"
local protoc = require "protoc"
local protoStr = [==[// ======================================
// ENGINE LEVEL PACKET. DO NOT EDIT.
// ======================================

syntax = "proto3";
package netutils;
option go_package = "bitbucket.org/funplus/sandwich/protocol/netutils";

option csharp_namespace = "gen.netutils";

// for internal use
message CmdPing {int64 timestamp = 1;}
message CmdPingAck {int64 timestamp = 1;}

// for devops checkup
message CmdCheckup {
  int32 code = 1;
  string message = 2;
  bytes CustomMeasurements = 3;
}
]==]
assert(protoc:load(protoStr,"netutils/cmds.proto"))

---@class CmdPing
---@field public timestamp number
local CmdPing={}
CmdPing.__index = CmdPing
---@return CmdPing
function CmdPing:new(data) return setmetatable(data or {},CmdPing)  end
---@return CmdPing
function CmdPing:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},CmdPing)  end
---@return string
function CmdPing:getMessageName() return "netutils.CmdPing" end
---@return string
function CmdPing:marshal()  return pb.encode(self:getMessageName(),self) end
netutils.CmdPing = CmdPing

---@class CmdPingAck
---@field public timestamp number
local CmdPingAck={}
CmdPingAck.__index = CmdPingAck
---@return CmdPingAck
function CmdPingAck:new(data) return setmetatable(data or {},CmdPingAck)  end
---@return CmdPingAck
function CmdPingAck:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},CmdPingAck)  end
---@return string
function CmdPingAck:getMessageName() return "netutils.CmdPingAck" end
---@return string
function CmdPingAck:marshal()  return pb.encode(self:getMessageName(),self) end
netutils.CmdPingAck = CmdPingAck

---@class CmdCheckup
---@field public code number
---@field public message string
---@field public CustomMeasurements string
local CmdCheckup={}
CmdCheckup.__index = CmdCheckup
---@return CmdCheckup
function CmdCheckup:new(data) return setmetatable(data or {},CmdCheckup)  end
---@return CmdCheckup
function CmdCheckup:newFromBytes(bytes) return setmetatable(pb.decode(self:getMessageName(),bytes) or {},CmdCheckup)  end
---@return string
function CmdCheckup:getMessageName() return "netutils.CmdCheckup" end
---@return string
function CmdCheckup:marshal()  return pb.encode(self:getMessageName(),self) end
netutils.CmdCheckup = CmdCheckup

