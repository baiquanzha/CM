local UICountDownTimer = UIControl("UICountDownTimer", "UICountDownTimerBase")
UICountDownTimer:Config({
    messages = {
        OnTimerUpdate = true,
        OnTimerEnd = true,
    }
})

function UICountDownTimer:Start(time, formatStr, strType)
    self:BaseStart(TimeUtil.GetServerTime() + time, formatStr, strType)
end

function UICountDownTimer:StartToEndTime(time, formatStr, strType)
    self:BaseStart(time, formatStr, strType)
end

function UICountDownTimer:Stop()
    self:BaseStop()
end