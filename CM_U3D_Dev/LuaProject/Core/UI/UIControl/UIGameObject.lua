local UIGameObject = UIControl("UIGameObject")
UIGameObject.baseTypeName = "UIGameObject"

function UIGameObject:Show()
    self:GetGameObject():SetActive(true)
end

function UIGameObject:Hide()
    self:GetGameObject():SetActive(false)
end

function UIGameObject:SetActive(active)
    if self:IsActive() ~= active then
        self:GetGameObject():SetActive(active)
    end
end

function UIGameObject:IsActive()
    return self:GetGameObject().activeSelf
end

function UIGameObject:SetName(name)
    self:GetGameObject().name = name
end

function UIGameObject:GetName()
    return self:GetGameObject().name
end

function UIGameObject:GetComponent(compName)
    return self:GetGameObject():GetComponent(compName)
end

function UIGameObject:SetAsFirstSibling()
    self:GetTransform():SetAsFirstSibling()
end

function UIGameObject:SetAsLastSibling()
    self:GetTransform():SetAsLastSibling()
end

--注意 下标从0开始
function UIGameObject:SetSiblingIndex(index)
    self:GetTransform():SetSiblingIndex(index)
end

function UIGameObject:GetSiblingIndex()
    return self:GetTransform():GetSiblingIndex()
end

function UIGameObject:SetRectSize(width, height)
    GameObjectUtil.SetRectSize(self:GetGameObject(), width, height)
end

function UIGameObject:GetRectHeight()
    return GameObjectUtil.GetRectHeight(self:GetGameObject())
end

function UIGameObject:SetLocalScale(scaleX, scaleY, scaleZ)
    if scaleY == nil then
        scaleY = scaleX
    end
    if scaleZ == nil then
        scaleZ = scaleX
    end
    GameObjectUtil.SetLocalScale(self:GetGameObject(), scaleX, scaleY, scaleZ)
end

function UIGameObject:GetWorldPosition()
    return self:GetTransform().position
end

function UIGameObject:GetLocalScale()
    return self:GetTransform().localScale
end

function UIGameObject:SetLocalPosition(posX, posY, posZ)
    GameObjectUtil.SetLocalPosition(self:GetGameObject(), posX, posY, posZ or 0)
end

function UIGameObject:SetAnchoredPosition(posX, posY)
    GameObjectUtil.SetAnchoredPosition(self:GetGameObject(), posX, posY)
end

function UIGameObject:SetAnchoredPosX(posX)
    GameObjectUtil.SetAnchoredPosX(self:GetGameObject(), posX)
end

function UIGameObject:SetAnchoredPosY(posY)
    GameObjectUtil.SetAnchoredPosY(self:GetGameObject(), posY)
end

function UIGameObject:SetLocalRotation(rotX, rotY, rotZ)
    GameObjectUtil.SetLocalRotation(self:GetGameObject(), rotX, rotY, rotZ)
end

function UIGameObject:SetLayoutElementPreferredWidth(width)
    GameObjectUtil.SetLayoutElementPreferredWidth(self:GetGameObject(), width)
end

function UIGameObject:SetAnchorType(type)
    local minX, minY, maxX, maxY
    if type == EnumAnchorType.BOTTOM_LEFT then
        minX, minY, maxX, maxY = 0, 0, 0, 0
    elseif type == EnumAnchorType.BOTTOM_RIGHT then
        minX, minY, maxX, maxY = 1, 0, 1, 0
    elseif type == EnumAnchorType.CENTER_BOTTOM then
        minX, minY, maxX, maxY = 0.5, 0.5, 0, 0
    else
        minX, minY, maxX, maxY = 0.5, 0.5, 0.5, 0.5
    end
    GameObjectUtil.SetAnchors(self:GetGameObject(), minX, minY, maxX, maxY)
end

function UIGameObject:DOScale(scale, duration)
    return self:GetTransform():DOScale(scale, duration)
end

function UIGameObject:DOAnchorPosY(posY, duration)
    return self:GetRectTransform():DOAnchorPosY(posY, duration)
end

local tempV2 = Vector2.zero

function UIGameObject:DOAnchorPos(posX, posY, duration)
    tempV2.x, tempV2.y = posX, posY
    return self:GetRectTransform():DOAnchorPos(tempV2, duration)
end