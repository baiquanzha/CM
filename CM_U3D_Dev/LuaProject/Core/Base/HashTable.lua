--声明，这里声明了类名还有属性，并且给出了属性的初始值。
HashTable = {
    list = {},
	tableCount = 0,
}

local tf = {}
tf.__index = HashTable

function HashTable.New() 
   local temp = {};
    setmetatable(temp, tf);
    return temp;
end

function HashTable:Put(_id,  _value)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i];
		if htable ~= nil and htable.id == _id then
			htable.value = _value;
			return;
		end
	end
	
	local htableNew = {id = _id, value = _value};
    table.insert(self.list, htableNew);
	self.tableCount = self.tableCount + 1
end

function HashTable:Get(_id)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i];
		if htable ~= nil and htable.id == _id then
			return htable.value;
		end
	end
    return nil;
end

function HashTable:Contains(_id)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i];
		if htable ~= nil and htable.id == _id then
			return true;
		end
	end
    return false;
end

function HashTable:ContainsItem(_item)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i];
		if htable ~= nil and htable.value == _item then
			return true;
		end
	end
    return false;
end

function HashTable:Remove(_id)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i];
		if htable ~= nil and htable.id == _id then
			local _tmp = htable.value
			table.remove(self.list, i);
			self.tableCount = self.tableCount - 1
			return _tmp;
		end
	end
end

function HashTable:RemoveItem(_item)
	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		local htable = self.list[i]
		if htable ~= nil and htable.value == _item then
			table.remove(self.list, i)
			self.tableCount = self.tableCount - 1
			return htable
		end
	end
end

function HashTable:Size()
	return self.tableCount --#self
end

function HashTable:Clear()
--[[	local size = self.tableCount --table.getn(self)
	for i = 1, size do
		table.remove(self, 1);
	end--]]
	self.list = {};
	self.tableCount = 0
end

return HashTable