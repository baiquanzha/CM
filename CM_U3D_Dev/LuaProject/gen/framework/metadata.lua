---@class Metadata
local Metadata = { }

---@param kvs table<string,string[]>
---@return Metadata
function newMetadata(kvs)
    return Metadata:new(kvs)
end

function Metadata:new(kvs)
    local o = { kvs = kvs or {} }
    setmetatable(o, self)
    self.__index = self
    return o
end

-- append adds the values to key k, not overwriting what was already stored at that key.
---@param key string
---@vararg string
function Metadata:append(key, ...)
    if #({ ... }) == 0 then
        return
    end
    local lowerKey = key:lower()
    for _, v in ipairs { ... } do
        if self.kvs[lowerKey] == nil then
            self.kvs[lowerKey] = {}
        end
        if type(v) == "table" then
            for _, s in ipairs(v) do
                self.kvs[lowerKey][#self.kvs[lowerKey] + 1] = tostring(s)
            end
        else
            self.kvs[lowerKey][#self.kvs[lowerKey] + 1] = tostring(v)
        end
    end
end

-- set sets the value of a given key with a slice of values.
---@param key string
---@vararg string
function Metadata:set(key, ...)
    if #({ ... }) == 0 then
        return
    end
    local lowerKey = key:lower()
    self.kvs[lowerKey] = {}
    for _,v in ipairs({ ... }) do
        self.kvs[lowerKey][#self.kvs[lowerKey]+1] = tostring(v)
    end
end

function Metadata:len()
    local cnt = 0
    for _, _ in pairs(self.kvs) do
        cnt = cnt + 1
    end
    return cnt
end

-- del delete the given key in metadata
---@param key string
function Metadata:del(key)
    if key == nil then
        return
    end
    self.kvs[key:lower()] = nil
end

---@param key string
function Metadata:getFirst(key)
    local vs = self.kvs[key:lower()]
    if vs==nil or #(vs) == 0 then
        return ""
    end
    return vs[1]
end

---@param key string
---@return string[]
function Metadata:get(key)
    local vs = self.kvs[key:lower()]
    if vs == nil or #(vs) == 0 then
        return nil
    end
    return vs
end

---@param md Metadata
---@return string[]
local function toPairs(md)
    local ret = {}
    if type(md) ~= "table" then
        return ret
    end
    for k, vs in pairs(md.kvs) do
        for _, s in ipairs(vs) do
            ret[#ret + 1] = tostring(k)
            ret[#ret + 1] = tostring(s)
        end
    end
    return ret
end

---@vararg string
---@return Metadata
local function fromPairs(...)
    local args = { ... }
    local argsLen = #(args)
    if argsLen == 1  then
        local md = Metadata:new()
        if type(args[1]) ~= "table" then
            return md
        end
        if (#(args[1])) % 2 == 1 then
            return md
        end
        local key = ""
        for i, s in ipairs(args[1]) do
            s = tostring(s)
            if i % 2 == 1 then
                key = s:lower()
            else
                md:append(key, s)
            end
        end
        return md
    elseif argsLen == 0 then
        return Metadata:new()
    end
    return fromPairs(args)
end

---@vararg Metadata
---@return Metadata
local function join(...)
    local out = newMetadata()
    for _, md in ipairs({ ... }) do
        for k, vs in pairs(md.kvs) do
            out:append(k, vs)
        end
    end
    return out
end

---@return Metadata
function Metadata:copy()
    return join(self)
end

return {
    join = join,
    fromPairs = fromPairs,
    toPairs = toPairs,
    new = newMetadata,
}