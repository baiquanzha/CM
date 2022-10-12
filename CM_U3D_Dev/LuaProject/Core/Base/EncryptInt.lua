--加密int
EncryptInt = {
	random = 0,
	value = 0,
}

function EncryptInt:new(o)
    o = o or {};
    setmetatable(o, self);
	self.__index = self;
    o:Reset();
    return o;
end

function EncryptInt:Reset()
	self.random = 0
	self.value = 0
end

function EncryptInt:SetValue(_value)
    self.random = math.random(1, 1000000);
    self.value = _value ~ self.random;
end

function EncryptInt:GetValue()
    return self.value ~ self.random
end