local UITabGroup = UIControl("UITabGroup", "UILayoutGroup")
UITabGroup.baseTypeName = "UITabGroup"
UITabGroup:Config({
    messages = {
        OnTabChanged = true,
        OnSelectedTabClick = true,
    },
})

function UITabGroup:OnLoaded()
    UITabGroup.__base.OnLoaded(self)
    self.context.selectedIndex = nil
    if self.__loadConfig.cell == nil then
        self.__loadConfig.cell = {
            type = "UITab",
            events = {
                OnTabClick = "OnTabClick",
            }
        }
    end

    if self.__loadConfig.tabs ~= nil then
        self:SetTabList(self.__loadConfig.tabs, self.__loadConfig.selTabs)
    end
end

function UITabGroup:SetTabCount(count)
    self:ClearAllChild()
    for i = 1, count do
        local ctrl = self:AddCtrlChild()
        ctrl:InitData(i)
    end
    self.context.selectedIndex = nil
end

function UITabGroup:SetTabStrList(tabList, selTabList)
    self:ClearAllChild()
    for i = 1, #tabList do
        local ctrl = self:AddCtrlChild()
        ctrl:InitData(i)
        if selTabList ~= nil then
            ctrl:SetTabStrData(tabList[i], selTabList[i])
        else
            ctrl:SetTabStrData(tabList[i], nil)
        end
    end
    self.context.selectedIndex = nil
end

function UITabGroup:SetTabList(tabList, selTabList)
    self:ClearAllChild()
    for i = 1, #tabList do
        local ctrl = self:AddCtrlChild()
        ctrl:InitData(i)
        if selTabList ~= nil then
            ctrl:SetTabData(tabList[i], selTabList[i])
        else
            ctrl:SetTabData(tabList[i], nil)
        end
    end
    self.context.selectedIndex = nil
end

function UITabGroup:SetSelectedTab(index)
    self:OnTabClick(index)
end

function UITabGroup:GetTab(index)
    if index > #self.__ctrlChildList then
        Error(string.format("The index [%s] is over count of all children[%s]", index, #self.__ctrlChildList))
        return
    end
    return self.__ctrlChildList[index]
end

function UITabGroup:GetAllTab()
    return self.__ctrlChildList
end

function UITabGroup:GetTabCount()
    return #self.__ctrlChildList
end

function UITabGroup:SetRedHintShow(checkFunc)
    for index, ctrl in ipairs(self.__ctrlChildList) do
        ctrl:SetRedHintShow(checkFunc(index))
    end
end

function UITabGroup:GetSelectedIndex()
    return self.context.selectedIndex
end

function UITabGroup:GetSelectedTab()
    if self.context.selectedIndex ~= nil then
        return self:GetTab(self.context.selectedIndex)
    else
        return nil
    end
end

function UITabGroup:GetSelectedTabName()
    return self:GetSelectedTab():GetName()
end

function UITabGroup:OnTabClick(index)
    if self.context.selectedIndex == index then
        self:PostMessage("OnSelectedTabClick", index)
    else
        local lastSelectedIndex = self.context.selectedIndex
        self.context.selectedIndex = index
        self:PostMessage("OnTabChanged", index)
        if lastSelectedIndex ~= nil then
            self:GetTab(lastSelectedIndex):SetSelect(false)
        end
        self:GetTab(index):SetSelect(true)
    end
end