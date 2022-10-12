local UISlider = UIControl("UISlider")

UI_SLIDER_TWEEN_TIME = 0.25

function UISlider:OnLoaded()
    self.__sliderComp = GameObjectUtil.GetSliderComp(self:GetGameObject())
end

function UISlider:OnUnloaded()
    self.__sliderComp = nil
    self.__tweenTimer = nil
    self.__value = nil
end

function UISlider:SetValue(value, counterType)
    if counterType ~= nil then
        if self.__value == nil then
            self.__sliderComp.value = value
        else
            if value == self.__value then
                return
            end

            local delayTime
            if counterType == EnumMoneyCounterType.NormalFly then
                delayTime = 1
            end

            if delayTime ~= nil then
                local lastNum = self.__value
                if self.__tweenTimer ~= nil then
                    self.__tweenTimer:SetSingleTimer(delayTime, function()
                        self.__sliderComp:DOValue(value, UI_SLIDER_TWEEN_TIME)
                    end)
                else
                    self.__tweenTimer = self:AddSingleTimer(delayTime, function()
                        self.__sliderComp:DOValue(value, UI_SLIDER_TWEEN_TIME)
                    end)
                end
            else
                self.__sliderComp:DOValue(value, UI_SLIDER_TWEEN_TIME)
            end
        end
        self.__value = value
    else
        self.__sliderComp.value = value
    end
end

function UISlider:DOValue(value, duration)
    return self.__sliderComp:DOValue(value, duration)
end