--模拟类
local BaseCls = {}
BaseCls.base = BaseCls
BaseCls.__index = BaseCls

function BaseCls:New(tb)
    tb = tb or {}
    setmetatable(tb, self)
    tb:Ctor()
    return tb
end

--构造方法（暂存）
function BaseCls:Ctor() end

_G.ExtendCls = function(baseCls, tb)
    if tb == nil then
        tb = baseCls
        baseCls = BaseCls
    end
    tb = tb or {}
    setmetatable(tb, baseCls)
    tb.__index = tb
    return tb
end