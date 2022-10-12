local UIGraphic = UIControl("UIGraphic")
UIGraphic.baseTypeName = "UIGraphic"
local graphicColor = Color.white

function UIGraphic:OnLoaded()
    self.__graphicComp = GameObjectUtil.GetGraphicComp(self:GetGameObject())
end

function UIGraphic:OnUnloaded()
    self.__graphicComp = nil
end

function UIGraphic:SetColor(colorTb)
    graphicColor.r = colorTb[1]
    graphicColor.g = colorTb[2]
    graphicColor.b = colorTb[3]
    graphicColor.a = colorTb[4] or 1
    self.__graphicComp.color = graphicColor
end

function UIGraphic:DOFade(endValue, duration)
    return self.__graphicComp:DOFade(endValue, duration)
end