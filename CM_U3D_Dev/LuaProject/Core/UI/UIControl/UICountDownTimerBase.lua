local UICountDownTimerBase = UIControl("UICountDownTimerBase", "UIText")
function UICountDownTimerBase:OnLoaded()
    UICountDownTimerBase.__base.OnLoaded(self)
    self.context.isRunning = false
end

function UICountDownTimerBase:OnUnloaded()
    self:BaseStop()
    UICountDownTimerBase.__base.OnUnloaded(self)
end

function UICountDownTimerBase:BaseStart(endTime, formatStr, strType)
    self.context.formatStr = formatStr
    self.context.strType = strType
    self.context.endTime = endTime
    self.context.lastTime = TimerMgr.curTime
    self.context.isSendEnd = false
    self:BaseGenerateStr(true)
    self.context.isRunning = true
end

function UICountDownTimerBase:BaseStop()
    self.context.isRunning = false
end

function UICountDownTimerBase:BaseGenerateStr(isInit)
    local remainTime = self.context.endTime - self.context.lastTime
    if remainTime < 0 then
        remainTime = 0
    end

    local timeStr = ""
    local timeData = TextUtil.TimeToDHMSTb(remainTime)
    if self.context.strType == EnumCountDownStrType.BundleGift then
        if timeData.day > 0 then
            timeStr = string.format("%dd %dh", timeData.day, timeData.hour)
        elseif timeData.hour > 0 then
            timeStr = string.format("%dh %dm", timeData.hour, timeData.min)
        else
            timeStr = string.format("%dm %ds", timeData.min, timeData.sec)
        end
    else
        if timeData.day > 0 then
            timeStr = string.format("%dday %dh", timeData.day, timeData.hour)
        elseif (timeData.hour > 0) or (self.context.strType == EnumCountDownStrType.AlwaysHour) then
            timeStr = string.format("%.2d:%.2d:%.2d", timeData.hour, timeData.min, timeData.sec)
        else
            timeStr = string.format("%.2d:%.2d", timeData.min, timeData.sec)
        end
    end

    if self.context.formatStr ~= nil then
        self:SetText(string.format(self.context.formatStr, timeStr))
    else
        self:SetText(timeStr)
    end

    if not isInit then
        self:PostMessage("OnTimerUpdate", remainTime)
    end

    if remainTime <= 0 then
        self.context.isSendEnd = true
        self:BaseStop()
    end
end

function UICountDownTimerBase:Update()
    if self.context.isRunning then
        if TimerMgr.curTime - self.context.lastTime >= 1 then
            self.context.lastTime = TimerMgr.curTime
            self:BaseGenerateStr()
        end
    else
        if self.context.isSendEnd then
            self.context.isSendEnd = false
            self:PostMessage("OnTimerEnd")
        end
    end
end