local UIText = UIControl("UIText")
UIText.baseTypeName = "UIText"

local textColor = Color.white

function UIText:OnLoaded()
    self.__textComp = GameObjectUtil.GetTextComp(self:GetGameObject())
end

function UIText:OnUnloaded()
    self.__textComp = nil
    self.__text = nil
end

function UIText:SetText(text, counterType, formatStr)
    if text == nil then
        Error("UIText:SetText() text is nil")
        text = ""
    end

    if counterType ~= nil then
        if self.__text == nil then
            if formatStr == nil then
                self.__textComp.text = text
            else
                self.__textComp.text = CS.System.String.Format(formatStr, text)
            end
        else
            self:NumTo(text, counterType, formatStr)
        end
        self.__text = text
    else
        self.__textComp.text = text
    end
end

function UIText:GetText()
    return self.__textComp.text
end

function UIText:SetColor(colorTb)
    textColor.r = colorTb[1]
    textColor.g = colorTb[2]
    textColor.b = colorTb[3]
    textColor.a = colorTb[4] or 1
    self.__textComp.color = textColor
end

function UIText:SetAlignment(_alignment)
    self.__textComp.alignment = _alignment
end

function UIText:SetOutlineColor(colorTb)
    GameObjectUtil.SetOutlineColor(self:GetGameObject(), colorTb[1], colorTb[2], colorTb[3], colorTb[4] or 1)
end

function UIText:SetShadowColor(colorTb)
    GameObjectUtil.SetShadowColor(self:GetGameObject(), colorTb[1], colorTb[2], colorTb[3], colorTb[4] or 1)
end

function UIText:NumTo(num, counterType, formatStr)
    if num == self.__text then
        return
    end

    local tweenTime, delayTime
    if counterType == EnumMoneyCounterType.Collect then
        tweenTime = 0.7
        delayTime = 1.6
    -- elseif counterType == EnumMoneyCounterType.TaskReward then
    --     tweenTime = 0.7
    --     delayTime = 0.8
    else
        if counterType == EnumMoneyCounterType.NormalFly then
            delayTime = 1.25
        end
        tweenTime = math.abs(num - self.__text) * 0.15
        if tweenTime > 0.75 then
            tweenTime = 0.75
        end
    end

    if delayTime ~= nil then
        local lastNum = self.__text
        local tweenTimer
        tweenTimer = self:AddSingleTimer(delayTime, function()
            self.__textComp:DOCounter(lastNum, num, tweenTime, false, nil, formatStr)
            tweenTimer:Destroy()
            tweenTimer = nil
        end)
    else
        self.__textComp:DOCounter(self.__text, num, tweenTime, false, nil, formatStr)
    end
end

function UIText:DOCounter(numStart, numEnd, duration)
    return self.__textComp:DOCounter(numStart, numEnd, duration)
end

function UIText:DOFade(alpha, duration)
    return self.__textComp:DOFade(alpha, duration)
end

function UIText:GetHeight()
    return self.__textComp.preferredHeight;
end

function UIText:GetWidth()
    return self.__textComp.preferredWidth;
end

function UIText:GetLineCount()
    return self.__textComp.cachedTextGenerator.lineCount;
end

function UIText:GetTextComp()
    return self.__textComp
end