EventMgr = {}

local eventDic = {}

--添加新的事件
function EventMgr:AddEvent(eventName, func, cls)
    if eventDic[eventName] == nil then
        eventDic[eventName] = {}
    end
    local event = Event:New(eventName, func, cls)
    --执行是倒序的 为保证先注册的事件先执行 所以添加的时候插入到最前面
    table.insert(eventDic[eventName], 1, event)
    return event
end

--移除事件
function EventMgr:RemoveEvent(eventName, event)
    if eventDic[eventName] ~= nil then
        for i = 1, #eventDic[eventName] do
            if eventDic[eventName][i] == event then
                table.remove(eventDic[eventName], i)
                return
            end
        end
    end
end

--执行事件
function EventMgr:FireEvent(eventName, ...)
    if eventDic[eventName] ~= nil then
        local length = #eventDic[eventName]
        local index = length
        while index > 0 do
            local eventData = eventDic[eventName][index]
            if eventData.__cls == nil then
                eventData.__func(...)
            else
                eventData.__func(eventData.__cls, ...)
            end
            index = index - 1
            --如果长度不一致 说明有新增或销毁的事件
            if length ~= #eventDic[eventName] then
                if length < #eventDic[eventName] then
                    --新增 列表第一位新增 索引向后一位
                    index = index + 1
                else
                    --减少 如果前一个位置和当前位置相同 说明前面减少了一个 导致当前位置前移 索引向前一位
                    if eventDic[eventName][index] == eventData then
                        index = index - 1
                    end
                end
                length = #eventDic[eventName]
            end
        end
    end
end

--注册事件 推荐用于Mgr
_G.RegisterEvent = function(eventName, func, cls)
    return EventMgr:AddEvent(eventName, func, cls)
end

--移除事件
_G.RemoveEvent = function(eventName, event)
    EventMgr:RemoveEvent(eventName, event)
end

--执行事件
_G.FireEvent = function(eventName, ...)
    EventMgr:FireEvent(eventName, ...)
end