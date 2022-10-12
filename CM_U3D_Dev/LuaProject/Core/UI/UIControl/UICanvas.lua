local UICanvas = UIControl("UICanvas")

function UICanvas:OnLoaded()
    self.__canvasComp = self:GetGameObject():GetComponent("Canvas")
end

function UICanvas:OnUnloaded()
    self.__canvasComp = nil
end

function UICanvas:SetSortingLayerName(sortingLayerName)
    self.__canvasComp.sortingLayerName = sortingLayerName
end

function UICanvas:SetOrderLayer(orderLayer)
    self.__canvasComp.sortingOrder = orderLayer
end

function UICanvas:SetEnable(isEnable)
    self.__canvasComp.enabled = isEnable
end