local UIButton = UIControl("UIButton")
UIButton.baseTypeName = "UIButton"
UIButton:Config({
    messages = {
        OnClick = true,
    }
})

--点击间隔
local clickInterval = 0.4

function UIButton:OnLoaded()
    self.__lastClickTime = 0
    self.__tweener = nil
    local clickFuncName = self.__loadConfig.events["OnClick"]
    if self.__parent[clickFuncName] == nil then
        Error("Control[%s] in [%s], its event[OnClick] has no function[%s]", self.__loadConfig.name or self:GetGameObject().name, self.__parent:GetUIBaseName(), clickFuncName)
    else
        GameObjectUtil.AddClickFunc(self:GetGameObject(), function()
            self:OnClick()
        end)
    end
end

function UIButton:OnUnloaded()
    self.lastClickTime = nil
    self.__tweener = nil
    GameObjectUtil.ClearClickFunc(self:GetGameObject())
end

function UIButton:SetInteractable(interactable)
    GameObjectUtil.SetButtonInteractable(self:GetGameObject(), interactable)
end

function UIButton:ShowByAnim()
    if self.__tweener ~= nil then
        self.__tweener:Kill()
    end
    self:Show()
    self:SetLocalScale(0)
    self.__tweener = self:GetTransform():DOScale(Vector3.one, 0.3):OnComplete(function()
        self.__tweener = nil
    end)
end

function UIButton:HideByAnim()
    if self.__tweener ~= nil then
        self.__tweener:Kill()
    end
    self:SetLocalScale(1)
    self.__tweener = self:GetTransform():DOScale(Vector3.zero, 0.3):OnComplete(function()
        self:Hide()
        self.__tweener = nil
    end)
end

function UIButton:SetActiveByAnim(active)
    if active then
        self:ShowByAnim()
    else
        self:HideByAnim()
    end
end

function UIButton:Show()
    self:SetActive(true)
end

function UIButton:Hide()
    self:SetActive(false)
end

function UIButton:SetActive(active)
    if self.__tweener ~= nil then
        self.__tweener:Kill()
        self.__tweener = nil
    end
    if self:IsActive() ~= active then
        self:GetGameObject():SetActive(active)
    end
end

local lastGlobalClickTime = 0

function UIButton:OnClick()
    local curMTime = TimerMgr.curMilliseconds
    if curMTime > self.__lastClickTime + clickInterval then
        --如果在0.05秒内检查到按了两下相同或不同的按钮 将第二次认为无效
        local runTime = CS.UnityEngine.Time.realtimeSinceStartup
        if runTime > lastGlobalClickTime + 0.05 then
            lastGlobalClickTime = runTime
            self.__lastClickTime = curMTime
            self:PostMessage("OnClick")
        end
    end
end