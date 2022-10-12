UIWindowBase = {}
UIWindowBase.__index = UIWindowBase
setmetatable(UIWindowBase, UIBase)

UIWindowBase.__windowName = ""
UIWindowBase.__isOpen = false
UIWindowBase.__isWindow = true
UIWindowBase.__isClosing = false
UIWindowBase.__waitOpen = false
UIWindowBase.__waitOpenArgs = nil
UIWindowBase.__isCache = false
UIWindowBase.__loadState = EnumWindowLoadStateType.None

local baseTrans = UICanvas.Get().baseTrans
local normalTrans = UICanvas.Get().normalTrans
local guideTrans = UICanvas.Get().guideTrans
local topTrans = UICanvas.Get().topTrans
local cacheTrans = UICanvas.Get().cacheTrans

WINDOW_CLOSE_DURATION = 0.5
WINDOW_MASK_DURATION = 0.45
COMMON_MASK_ALPHA = 0.7

--设置界面为预加载界面
function UIWindowBase:__setCacheWindow()
    self.__isCache = true
    self:SetParent(cacheTrans)
end

--打开界面
function UIWindowBase:__loadWindow(args, _obj)
    --检查是否已开启
    if self.__isOpen then
        Debug(string.format("无法打开[%s] Window已被打开", self.__windowName))
        return false;
    end

    --如果正在关闭则停止
    if self.__isClosing then
        self.__waitOpen = true
        self.__waitOpenArgs = args
        return false
    end

    if self.__isCache and (not UICompTool.IsObjDestroy(self:GetGameObject())) then
        self:SetParent(self:GetPrefabParent())
        self:GetGameObject():SetActive(true)
        if self.__loadState == EnumWindowLoadStateType.PreLoad then
            self.__loadState = EnumWindowLoadStateType.FirstOpen
            --局变量存储表
            self.context = {}
        elseif self.__loadState == EnumWindowLoadStateType.FirstOpen then
            self.__loadState = EnumWindowLoadStateType.Focus
        end
    else
        --预加载图集
        self:PreLoadAtlas()

        --加载Prefab
        if _obj == nil then
            self:LoadWindowPrefab()
        else
            self:SetGameObject(_obj)
        end
    
        --绑定子控件
        self:BindCtrls()

        --局变量存储表
        self.context = {}

        if self.__isCache then
            self.__loadState = EnumWindowLoadStateType.FirstOpen
        else
            self.__loadState = EnumWindowLoadStateType.None
        end
    end

    --覆盖值
    self.__timerList = {}
    self.__coroutineList = {}

    --打开状态
    self.__isOpen = true

    --注册事件
    self:RegisterEvents()

    --Update注册
    self:RegisterUpdate()

    return true, self.__loadState == EnumWindowLoadStateType.Focus
end

--关闭界面
function UIWindowBase:__unloadWindow()
    if not self.__isOpen then
        Debug("无法关闭Window Window[%s]已被关闭", self.__windowName)
        return
    end

    self.__isClosing = true

    --移除Update
    self:DestroyUpdate()

    --移除Prefab
    local isCloseAnim = self:UnloadWindowPrefab()

    --移除子控件
    self:UnBindCtrls()

    --移除事件
    self:DestroyEvents()

    --移除计时器
    self:DestroyTimerList()

    --打开状态
    self.__isOpen = false

    --关闭方法
    self:OnClose()

    --清空局变量存储表
    self.context = {}

    if isCloseAnim then
        local timer
        timer = TimerMgr:AddSingleTimer(WINDOW_CLOSE_DURATION, function()
            self:__finCloseAndCheckOpen()
        end)
    else
        self:__finCloseAndCheckOpen()
    end
end

--关闭界面
function UIWindowBase:__closeAndCacheWindow()
    if not self.__isOpen then
        Debug("无法关闭Window Window[%s]已被关闭", self.__windowName)
        return
    end

    self.__isClosing = true

    --移除Update
    self:DestroyUpdate()
    --移除事件
    self:DestroyEvents()
    --移除计时器
    self:DestroyTimerList()

    --缓存Prefab
    local isCloseAnim = self:CacheWindowPrefab()

    --移除子控件
    --self:UnBindCtrls()

    --打开状态
    self.__isOpen = false

    --关闭方法
    self:OnClose()

    --清空局变量存储表
    --self.context = {}

    if isCloseAnim then
        local timer
        timer = TimerMgr:AddSingleTimer(WINDOW_CLOSE_DURATION, function()
            self:__finCloseAndCheckOpen()
        end)
    else
        self:__finCloseAndCheckOpen()
    end
end

function UIWindowBase:__finCloseAndCheckOpen()
    self.__isClosing = false
    --如果等待打开则重新执行打开
    if self.__waitOpen then
        self.__waitOpen = false
        local openArgs = self.__waitOpenArgs
        self.__waitOpenArgs = nil
        UIMgr:OpenWindow(self.__windowName, openArgs)
    end
end

--预加载图集
function UIWindowBase:PreLoadAtlas()
    if self.__config.preloadAtlas ~= nil then
        UIAtlasMgr:LoadAtlasArr(self.__config.preloadAtlas)
    end
end

function UIWindowBase:LoadWindowPrefab()
    self:SetGameObject(self:LoadPrefab(self:GetPrefabParent(), self.__config.prefabPath))
end

function UIWindowBase:UnloadWindowPrefab()
    local isCloseAnim = false
    if (self.__config.windowType == nil) or (self.__config.windowType == UI_WINDOW_TYPE.NORMAL_WINDOW) or (self.__config.windowType == UI_WINDOW_TYPE.BASE_WINDOW) then
        if not self.__config.isNoAnim then
            UIMgr.isWindowAnim = true
            CS.SoundManager.Get():PlaySound("popup_close", false);
            if self.__config.openType == UI_WINDOW_OPEN_TYPE.FROM_TOP then
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_up_out")
                isCloseAnim = true
            else
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_down_out")
                isCloseAnim = true
            end
            UIMgr:ShowFrontMask()
            local timer
            timer = TimerMgr:AddSingleTimer(WINDOW_CLOSE_DURATION, function()
                --销毁Prefab
                self:UnLoadPrefab()
                timer:Destroy()
                UIMgr.isWindowAnim = false
            end)
        else
            if self.PlayCloseAnim ~= nil then
                CS.SoundManager.Get():PlaySound("popup_close", false);
                self:PlayCloseAnim()
                UIMgr:ShowFrontMask()
                UIMgr.isWindowAnim = true
                local timer
                timer = TimerMgr:AddSingleTimer(WINDOW_CLOSE_DURATION, function()
                    --销毁Prefab
                    self:UnLoadPrefab()
                    timer:Destroy()
                    UIMgr.isWindowAnim = false
                end)
            else
                --销毁Prefab
                self:UnLoadPrefab()
            end
        end
    else
        --销毁Prefab
        self:UnLoadPrefab()
    end
    return isCloseAnim
end

--缓存界面预设
function UIWindowBase:CacheWindowPrefab()
    local isCloseAnim = false
    if (self.__config.windowType == nil) or (self.__config.windowType == UI_WINDOW_TYPE.NORMAL_WINDOW) or (self.__config.windowType == UI_WINDOW_TYPE.BASE_WINDOW)  then
        if not self.__config.isNoAnim then
            CS.SoundManager.Get():PlaySound("popup_close", false);
            if self.__config.openType == UI_WINDOW_OPEN_TYPE.FROM_TOP then
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_up_out")
                isCloseAnim = true
            else
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_down_out")
                isCloseAnim = true
            end
            UIMgr:ShowFrontMask()
            local timer
            timer = TimerMgr:AddSingleTimer(WINDOW_CLOSE_DURATION, function()
                --缓存Prefab
                self:SetParent(cacheTrans)
                self:GetGameObject():SetActive(false);
                timer:Destroy()
            end)
        else
            --缓存Prefab
            self:SetParent(cacheTrans)
            self:GetGameObject():SetActive(false);
        end
    else
        --缓存Prefab
        self:SetParent(cacheTrans)
        self:GetGameObject():SetActive(false);
    end
    return isCloseAnim
end

--设置层级
function UIWindowBase:SetCanvasDepth()
    GameObjectHelper.SetCanvasSortingOrder(self.__gameObject, self.__config.layerDepth or UI_WINDOW_DEPTH.LAYER_BOTTOM)
end

--注册全局事件 此类时间不需要移除
function UIWindowBase:RegisterGlobalEvents()
    if self.__config.globalEvents ~= nil then
        for eventName, funcName in pairs(self.__config.globalEvents) do
            local func = self[funcName]
            if func ~= nil then
                EventMgr:AddEvent(eventName, func, self)
            else
                Error("GloablEvent[%s] in [%s] has no function[%s]", eventName, self:GetUIBaseName(), funcName)
            end
        end
    end
end

--注册Update
function UIWindowBase:RegisterUpdate()
    if self.Update ~= nil then
        MainUpdate:AddUpdateFunc(self.__windowName, self.Update, self)
    end
end

--移除Update
function UIWindowBase:DestroyUpdate()
    MainUpdate:RemoveUpdateFunc(self.__windowName)
end

function UIWindowBase:PlayOpenAnim()
    if (self.__config.windowType == nil) or (self.__config.windowType == UI_WINDOW_TYPE.NORMAL_WINDOW) or (self.__config.windowType == UI_WINDOW_TYPE.BASE_WINDOW)  then
        if not self.__config.isNoAnim then
            CS.SoundManager.Get():PlaySound("popup_open", false);
            if self.__config.openType == UI_WINDOW_OPEN_TYPE.FROM_TOP then
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_up_in")
            else
                GameObjectUtil.PlayAnimation(self:GetGameObject(), "an_popup_down_in")
            end
            UIMgr:ShowFrontMask()
        end
    end
end

--打开界面方法(预留)
function UIWindowBase:OnOpen() end

--关闭界面方法(预留)
function UIWindowBase:OnClose() end

--关闭游戏方法(预留)
function UIWindowBase:OnQuit() end

--获取WindowName
function UIWindowBase:GetUIBaseName()
    return self.__windowName
end

--关闭自身Window
function UIWindowBase:CloseSelfWindow()
    CloseWindow(self.__windowName)
end

--是否是最顶层界面
function UIWindowBase:IsTopWindow()
    local topWindowName = UIMgr:GetTopWindowName()
    return topWindowName == self.__windowName
end

--是否已开启
function UIWindowBase:IsOpen()
    return self.__isOpen
end

--获取Prefab挂载节点
function UIWindowBase:GetPrefabParent()
    if (self.__config.windowType == UI_WINDOW_TYPE.POPUP_WINDOW) or (self.__config.windowType == UI_WINDOW_TYPE.TOP_WINDOW) then
        return topTrans
    elseif self.__config.windowType == UI_WINDOW_TYPE.GUIDE_WINDOW then
        return guideTrans
    elseif self.__config.windowType == UI_WINDOW_TYPE.BASE_WINDOW then
        return baseTrans
    else
        return normalTrans
    end
end