local UICanvasGroup = UIControl("UICanvasGroup")

function UICanvasGroup:OnLoaded()
    self.__canvasGroupComp = self:GetGameObject():GetComponent("CanvasGroup")
end

function UICanvasGroup:OnUnloaded()
    self.__canvasGroupComp = nil
end

function UICanvasGroup:GetAlpha()
    return self.__canvasGroupComp.alpha
end

function UICanvasGroup:SetAlpha(alpha)
    self.__canvasGroupComp.alpha = alpha
end

function UICanvasGroup:GetCanvasGroupComp()
    return self.__canvasGroupComp
end

function UICanvasGroup:DOFade(endValue, duration)
    return self.__canvasGroupComp:DOFade(endValue, duration)
end

function UICanvasGroup:DOKill()
    return self.__canvasGroupComp:DOKill()
end