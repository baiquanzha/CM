local pb = require 'pb'

netutils = netutils or {}
netutils.requestIndex = 1
netutils.sequenceID = 1
netutils.EnableQueue = false
netutils.VersionCode = 1
netutils.metadata = require(GEN_PACKAGE_NAME .. ".framework.metadata")
netutils.defaultMetadata = netutils.metadata.new()

function netutils:nextSequenceID()
	if self.sequenceID >= 2147483647 then
		self.sequenceID = 1
	end
	self.sequenceID = self.sequenceID + 1
	return self.sequenceID
end

function netutils:nextPassThrough()
	if self.requestIndex >= 2147483647 then
		self.requestIndex = 1
	end
	self.requestIndex = self.requestIndex + 1
	return os.time() .. '_' .. self.requestIndex
end

---@param mm table table of messages or just message has method:getMessageName and marshal
---@param md Metadata|nil
---@return RawPacket,string actually is bytes
function netutils:marshalRawPacket(mm,md)
	local packet = self.RawPacket:new()

	-- single message
	if mm.getMessageName then
		mm = {mm}
	else
		-- message table
		if not self.EnableQueue then
			error(string_format("mm is message list, but queue not enabled"))
		end
	end

	local rawAnyValid = false
	for _, msg in ipairs(mm) do
		if msg.getMessageName then
			local any = self.RawAny:new()
			any.uri = msg:getMessageName()
			any.raw = msg:marshal()
			any.passThrough = self:nextPassThrough()
			if self.EnableQueue then
				if packet.rawAny == nil then
					packet.rawAny = {}
				end
				table.insert(packet.rawAny,any)
				rawAnyValid = true
			else
				packet.rawAny = any
				rawAnyValid = true
				break
			end
		end
	end
	if not rawAnyValid then
		error(string_format("missing message"))
	end

	local mdLocal = self.defaultMetadata
	if md then
		mdLocal = self.metadata.join(mdLocal,md)
	end
	mdLocal:set("x_create_time", os.time())
	packet.metadata = self.metadata.toPairs(mdLocal)
	packet.SequenceID = self:nextSequenceID()
	packet.Version = self.VersionCode

	return packet, packet:marshal()
end

---@param bytes string actually is bytes
---@return RawPacket
function netutils:unmarshalRawPacket(bytes)
	local packet = self.RawPacket:newFromBytes(bytes)
	if self.EnableQueue then
		for index, v in ipairs(packet.rawAny) do
			packet.rawAny[index].message = pb.decode(v.uri, v.raw)
		end
	else
		packet.rawAny.message = pb.decode(packet.rawAny.uri, packet.rawAny.raw)
	end
	packet.metadataObj = self.metadata.fromPairs(packet.metadata)
	return packet
end