Timer = {
    __timerIndex = 0,
    __repeatCount = 0, --重复次数
    __repeatInterval = 0, --重复间隔
    __callBackFunc = nil, --回调

    __waitTime = 0,
    __isInfinite = false,
}
Timer.__index = Timer

function Timer:New(index, repeatCount, repeatInterval, callBackFunc)
    local timer = setmetatable({}, self)
    timer.__timerIndex = index
    timer.__repeatCount = repeatCount
    timer.__repeatInterval = repeatInterval
    timer.__callBackFunc = callBackFunc
    timer.__isInfinite = repeatCount <= 0
    return timer
end

function Timer:UpdateTimeData(timePass)
    if (not self.__isInfinite) and (self.__repeatCount <= 0) then
        return
    end
    self.__waitTime = self.__waitTime + timePass
    if self.__waitTime >= self.__repeatInterval then
        self.__waitTime = self.__waitTime - self.__repeatInterval
        self.__repeatCount = self.__repeatCount - 1
        self.__callBackFunc()
    end
end

function Timer:SetTimer(repeatCount, repeatInterval, callBackFunc)
    self.__repeatCount = repeatCount
    self.__repeatInterval = repeatInterval
    self.__callBackFunc = callBackFunc
    self.__isInfinite = repeatCount <= 0
    self.__waitTime = 0
end


function Timer:SetLoopTimer(repeatInterval, callBackFunc)
    self.__repeatCount = 0
    self.__repeatInterval = repeatInterval
    self.__callBackFunc = callBackFunc
    self.__isInfinite = true
    self.__waitTime = 0
end

function Timer:SetSingleTimer(waitTime, callBackFunc)
    self.__repeatCount = 1
    self.__repeatInterval = waitTime
    self.__callBackFunc = callBackFunc
    self.__isInfinite = false
    self.__waitTime = 0
end

function Timer:Stop()
    self.__repeatCount = 0
    self.__repeatInterval = 0
    self.__callBackFunc = nil
    self.__isInfinite = false
end

function Timer:Destroy()
    TimerMgr:RemoveTimer(self.__timerIndex)
end