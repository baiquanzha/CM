TimerMgr = {}

local timerList = {}
local timeTriggerDic = {}

local timeSpan = TimeUtil.GetServerTimeSpan()
local timerIndex = 0
--具体时间相关
TimerMgr.curMilliseconds = timeSpan.TotalSeconds
TimerMgr.curTime = math.floor(TimerMgr.curMilliseconds)
TimerMgr.frame = 0
TimerMgr.hour = timeSpan.Hours
TimerMgr.min = timeSpan.Minutes
TimerMgr.sec = timeSpan.Seconds

TimerMgr.loginTime = 0

TimerMgr.loginServerTime = 0        --登录时服务器时间
TimerMgr.loginElapsedRealtime = 0   --登录时设备时间
--TimerMgr.localOfferTime = 0         --本地时间插值
TimerMgr.loginWday = 1              --登录时星期 1:星期日

local zoneTime = 0  --时区差值

local timePass
function TimerMgr:Update()
    --获取当前时间
    timeSpan = TimeUtil.GetServerTimeSpan()
    --只在编辑器模式下记录已运行帧数
    if Booter.isEditor then
        self.frame = self.frame + 1
    end
    --记录具体时间相关
    self.hour = timeSpan.Hours
    self.min = timeSpan.Minutes
    self.sec = timeSpan.Seconds
    --检查时间触发器
    self:CheckTimeTrigger()
    --计时器
    timePass = timeSpan.TotalSeconds - TimerMgr.curMilliseconds
    TimerMgr.curMilliseconds = timeSpan.TotalSeconds;
    self.curTime = math.floor(TimerMgr.curMilliseconds)
    math.randomseed(self.curTime)
    for i = 1, timerIndex do
        if timerList[i] ~= nil then
            timerList[i]:UpdateTimeData(timePass)
        end
    end
end

function TimerMgr:GetServerTime()
    return self.curTime;
end

function TimerMgr:GetLocalTime()
    return self.curTime + zoneTime
end

function TimerMgr:SetServerTime(_serverTime)
    TimerMgr.loginElapsedRealtime = CS.NativeReceiver.Get():GeteElapsedRealtime();
    TimerMgr.loginServerTime = _serverTime;
    TimeUtil.m_serverTimeOffset = _serverTime * 1000 - TimeUtil.GetTimeMillStamp();
    print("-----TimerMgr:SetServerTime TimeUtil.m_serverTimeOffset = "..TimeUtil.m_serverTimeOffset.."  TimerMgr.loginElapsedRealtime = "..TimerMgr.loginElapsedRealtime);
    
    --时区差值
    local time = os.time()
    local zoneCurTime = os.date("*t", time)
    local zoneZeroTime = os.date("!*t", time)
    zoneTime = (zoneCurTime.hour - zoneZeroTime.hour) * 3600 + (zoneCurTime.min - zoneZeroTime.min) * 60
    TimerMgr.loginWday = zoneCurTime.wday
end

--同步设备时间
function TimerMgr:UpdateElapsedRealtime()
    local elapsedRealtime = CS.NativeReceiver.Get():GeteElapsedRealtime();
    if elapsedRealtime > 0 and TimerMgr.loginElapsedRealtime > 0 then
        local curTime = TimerMgr.loginServerTime * 1000 + (elapsedRealtime - TimerMgr.loginElapsedRealtime);
        TimeUtil.m_serverTimeOffset = curTime - TimeUtil.GetTimeMillStamp();
    end
    print("-----TimerMgr:UpdateElapsedRealtime TimeUtil.m_serverTimeOffset = "..TimeUtil.m_serverTimeOffset.."  elapsedRealtime = "..elapsedRealtime.."  loginElapsedRealtime = "..TimerMgr.loginElapsedRealtime);
end

function TimerMgr:CreateTimer()
    timerIndex = timerIndex + 1
    local timer = Timer:New(timerIndex, 0, 0)
    timer.__isInfinite = false
    timerList[timerIndex] = timer
    return timer
end

--- 添加普通计时器
--- repeatCount 重复次数 如果为0或负数则无限
--- repeatInterval 重复间隔
--- callBack 回调
function TimerMgr:AddTimer(repeatCount, repeatInterval, callBackFunc)
    if callBackFunc == nil then
        return
    end

    timerIndex = timerIndex + 1
    local timer = Timer:New(timerIndex, repeatCount, repeatInterval, callBackFunc)
    timerList[timerIndex] = timer
    return timer
end

--添加无限计时器
function TimerMgr:AddLoopTimer(repeatInterval, callBackFunc)
    return self:AddTimer(0, repeatInterval, callBackFunc)
end

--添加单次计时器
function TimerMgr:AddSingleTimer(waitTime, callBackFunc)
    return self:AddTimer(1, waitTime, callBackFunc)
end

function TimerMgr:RemoveTimer(index)
    if timerList[index] ~= nil then
        timerList[index] = nil
    end
end

--触发器 只精确到分
function TimerMgr:AddTimeTrigger(hour, min, func)
    local key = (hour % 24) * 1000 + min
    if timeTriggerDic[key] == nil then
        timeTriggerDic[key] = IList:new();
    end
    timeTriggerDic[key]:Add(func);
end

--检查时间触发器
local lastHour, lastMin
local timeTriggerList;
local tempFunc;
function TimerMgr:CheckTimeTrigger()
    if lastMin == nil then
        lastHour, lastMin = self.hour, self.min
    elseif lastMin ~= self.min then
        --如果分钟不同则认为是有了变化
        timeTriggerList = timeTriggerDic[self.hour * 1000 + self.min];
        if timeTriggerList ~= nil then
            for i = 1, timeTriggerList.Count do
                tempFunc = timeTriggerList:Get(i);
                tempFunc()
            end 
        end
        lastHour, lastMin = self.hour, self.min
    end
end

function TimerMgr:GetTimeByHourAndMin(hour, min)
    return self.curTime - (self.hour - hour) * 3600 - (self.min - min) * 60 - self.sec
end

function TimerMgr:GetTimeStr()
    return string.format("%.2d:%.2d:%.2d", self.hour, self.min, self.sec)
end

local curDays
local tempDays
function TimerMgr:IsAfterDay(time, day)
    curDays = math.modf(TimerMgr:GetServerTime() / 3600 / 24);
    tempDays = math.modf(time / 3600 / 24);
    if curDays - tempDays >= day then
        return true
    end
    return false
end