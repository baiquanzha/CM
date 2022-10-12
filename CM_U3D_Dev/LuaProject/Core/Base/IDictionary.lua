--声明，这里声明了类名还有属性，并且给出了属性的初始值。
IDictionary = {
    list = {},
    dic = {},
	tableCount = 0,
}

function IDictionary:new(o)
    o = o or {};
    setmetatable(o, self);
	self.__index = self;
    o:Clear();
    return o;
end

function IDictionary:Add(_key,  _value)
	local size = self.tableCount --table.getn(self)
    local htable;
	for i = 1, size do
		htable = self.list[i];
		if htable ~= nil and htable.key == _key then
			htable.value = _value;
            self.dic[_key] = _value;
			return;
		end
	end
	
	local htableNew = {key = _key, value = _value};
    table.insert(self.list, htableNew);
    self.dic[_key] = _value;
	self.tableCount = self.tableCount + 1
end

function IDictionary:Set(_key,  _value)
	local size = self.tableCount --table.getn(self)
    local htable;
	for i = 1, size do
		htable = self.list[i];
		if htable ~= nil and htable.key == _key then
			htable.value = _value;
            self.dic[_key] = _value;
			return;
		end
	end
	
	local htableNew = {key = _key, value = _value};
    table.insert(self.list, htableNew);
    self.dic[_key] = _value;
	self.tableCount = self.tableCount + 1
end

function IDictionary:Get(_key)
--	local size = self.tableCount --table.getn(self)
--    local htable;
--	for i = 1, size do
--		htable = self.list[i];
--		if htable ~= nil and htable.key == _key then
--			return htable.value;
--		end
--	end
    return self.dic[_key];
end

function IDictionary:ContainsKey(_key)
--	local size = self.tableCount --table.getn(self)
--    local htable;
--	for i = 1, size do
--		htable = self.list[i];
--		if htable ~= nil and htable.key == _key then
--			return true;
--		end
--	end
	if self.dic[_key] ~= nil then
		return true;
	end
    return false;
end

function IDictionary:Remove(_key)
	local size = self.tableCount --table.getn(self)
    local htable;
	for i = 1, size do
		htable = self.list[i];
		if htable ~= nil and htable.key == _key then
			table.remove(self.list, i);
            self.dic[_key] = nil;
			self.tableCount = self.tableCount - 1;
			return htable.value;
		end
	end
end

function IDictionary:RemoveIndex(_index)
	local htable = self.list[_index];
	if htable ~= nil then
		table.remove(self.list, _index);
        self.dic[htable.key] = nil;
		self.tableCount = self.tableCount - 1;
	end
end

function IDictionary:Size()
	return self.tableCount --#self
end

function IDictionary:Clear()
	self.list = {};
    self.dic = {};
	self.tableCount = 0
end

return IDictionary