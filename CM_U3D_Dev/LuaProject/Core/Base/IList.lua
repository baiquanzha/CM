--列表
IList = {
	list = {},
	Count = 0,
}

function IList:new(o)
    o = o or {};
    setmetatable(o, self);
	self.__index = self;
    o:Reset();
    return o;
end

function IList:Reset()
	self.list = {}
	self.Count = 0
end

function IList:Add(unit)
	table.insert(self.list, unit);
	self.Count = self.Count + 1
end

function IList:Insert(pos, unit)
	table.insert(self.list, pos, unit);
	self.Count = self.Count + 1
end

function IList:RemoveAt(pos)
	table.remove(self.list, pos)
	self.Count = self.Count - 1
end

function IList:Remove(item)
	for i = 1, self.Count do
		if self.list[i] == item then
			self:RemoveAt(i)
			break
		end
	end
end

function IList:GetIndex(value)
	for i = 1, self.Count do
		if self.list[i] == value then
			return 1;
		end
	end
    return -1;
end

function IList:Contains(item)
	return table.contains(self.list, item)
end

function IList:Clear()
	self.list = {}
	self.Count = 0
end

function IList:GetList()
	return self.list
end

function IList:Size()
	return self.Count
end

function IList:Get(pos)
	return self.list[pos]
end

return IList