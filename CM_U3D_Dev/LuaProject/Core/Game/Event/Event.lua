Event = {
    __eventName = nil,
    __func = nil,
    __cls = nil,
}
Event.__index = Event

function Event:New(eventName, func, cls)
    return setmetatable({
        __eventName = eventName,
        __func = func,
        __cls = cls,
    }, self)
end

function Event:Destroy()
    EventMgr:RemoveEvent(self.__eventName, self)
end