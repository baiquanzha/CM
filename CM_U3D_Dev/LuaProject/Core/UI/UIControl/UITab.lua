local UITab = UIControl("UITab")
UITab.baseTypeName = "UITab"
UITab:Config({
    messages = {
        OnTabClick = true,
    },
    ctrls = {
        {
            path = "goBG",
            name = "goBG",
        },

        {
            nilable = true,
            type = "UIText",
            path = "goBG/txtUnselect",
            name = "txtUnselect",
        },

        {
            nilable = true,
            type = "UIImage",
            path = "goBG/imgIcon",
            name = "imgUnselectIcon",
        },

        {
            path = "goSelect",
            name = "goSelect",
        },

        {
            nilable = true,
            type = "UIText",
            path = "goSelect/txtSelect",
            name = "txtSelect",
        },

        {
            nilable = true,
            type = "UIImage",
            path = "goSelect/imgIcon",
            name = "imgSelectIcon",
        },

        {
            nilable = true,
            path = "imgRedHint",
            name = "imgRedHint",
        },

        {
            nilable = true,
            path = "goBG/imgRedHint",
            name = "imgUnSelectRedHint",
        },

        {
            nilable = true,
            path = "goSelect/imgRedHint",
            name = "imgSelectRedHint",
        },

        {
            type = "UIButton",
            path = "btnSelect",
            events = {
                OnClick = "OnSelectButtonClick",
            },
        },
    },
})

function UITab:OnLoaded()
    self.ctrls.goBG:Show()
    self.ctrls.goSelect:Hide()

    if self.ctrls.imgRedHint ~= nil then
        self.ctrls.imgRedHint:Hide()
    end
    
    if self.ctrls.imgUnSelectRedHint ~= nil then
        self.ctrls.imgUnSelectRedHint:Hide()
    end
    
    if self.ctrls.imgSelectRedHint ~= nil then
        self.ctrls.imgSelectRedHint:Hide()
    end
end

function UITab:InitData(index)
    self.context.index = index
end

function UITab:SetTabStrData(str, selStr)
    if self.ctrls.txtUnselect ~= nil then
        self.ctrls.txtUnselect:SetText(str)
    end

    if selStr ~= nil then
        str = selStr
    end
    if self.ctrls.txtSelect ~= nil then
        self.ctrls.txtSelect:SetText(str)
    end
end

function UITab:SetTabData(str, selStr)
    if self.ctrls.imgUnselectIcon ~= nil then
        self.ctrls.imgUnselectIcon:SetImage(str, true);
    end
    
    if selStr ~= nil then
        str = selStr
    end
    self.context.name = str
    if self.ctrls.imgSelectIcon ~= nil then
        self.ctrls.imgSelectIcon:SetImage(str, true);
    end
end

function UITab:SetRedHintShow(isShow)
    if self.ctrls.imgRedHint ~= nil then
        self.ctrls.imgRedHint:SetActive(isShow)
    end
    if self.ctrls.imgUnSelectRedHint ~= nil then
        self.ctrls.imgUnSelectRedHint:SetActive(isShow)
    end
    if self.ctrls.imgSelectRedHint ~= nil then
        self.ctrls.imgSelectRedHint:SetActive(isShow)
    end
end

function UITab:SetSelect(isSelect)
    self.ctrls.goBG:SetActive(not isSelect)
    self.ctrls.goSelect:SetActive(isSelect)
end

function UITab:IsRedHintShow()
    if self.ctrls.imgRedHint ~= nil then
        return self.ctrls.imgRedHint:IsActive()
    elseif self.ctrls.imgUnSelectRedHint ~= nil then
        return self.ctrls.imgUnSelectRedHint:IsActive()
    elseif self.ctrls.imgSelectRedHint ~= nil then
        return self.ctrls.imgSelectRedHint:IsActive()
    else
        return false
    end
end

function UITab:GetName()
    return self.context.name
end

function UITab:OnSelectButtonClick()
    CS.SoundManager.Get():PlaySound("popup_tabs", false)
    self:PostMessage("OnTabClick", self.context.index)
end