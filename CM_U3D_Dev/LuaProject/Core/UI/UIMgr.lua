--界面加载状态
EnumWindowLoadStateType = {
    None = 0,       --无
    Focus = 1,      --恢复打开
    FirstOpen = 2,  --第一次开启
    PreLoad = 3,    --预加载
}

UIMgr = {
    mapItemPanel = nil,     --地图物品界面
    isWindowAnim = false,   --当前界面是否动画中
}

local windowClsDic = {}
local controlClsDic = {}
local openWindowList = IList:new()
local UIGameObject

--延后打开界面
local delayOpenWindowList = IList:new()
--其他界面打开时需要延后打开界面
local DELAY_OPEN_WINDOW_LIST = {
    ["UIClearNpcExchangeWindow"] = true,
    ["UIBattlePassLvUpWindow"] = true,
}
--是否在开云中
local isCollectFlying = false
local isWaitOpenWindow = false

local delayNoWindowFuncList = IList:new()

local function CheckDelayOpenWindow()
    if delayOpenWindowList.Count > 0 then
        UIMgr:OpenWindow(delayOpenWindowList:Get(1).windowName, delayOpenWindowList:Get(1).args)
        delayOpenWindowList:RemoveAt(1)
        return
    end
    
    if UserInfo.CheckLevelUpShow() then
        return
    end

    if AssistantMapInfo.CheckLevelUpShow() then
        return
    end

    --当界面全部关闭后的延迟方法
    if delayNoWindowFuncList.Count > 0 then
        delayNoWindowFuncList:Get(1)()
        delayNoWindowFuncList:RemoveAt(1)
        return
    end
end

--打开Window
function UIMgr:AddDelayFunc(func)
    delayNoWindowFuncList:Add(func)
end

--打开Window
function UIMgr:OpenWindow(windowName, args, isForceOpen, _obj, isAutoOpen)
    if (not isForceOpen) and (DELAY_OPEN_WINDOW_LIST[windowName] and ((isCollectFlying == true) or (self:GetTopWindow() ~= nil))) then
        delayOpenWindowList:Add({windowName = windowName, args = args})
        return nil
    end

    local windowCls = UIMgr:GetWindowCls(windowName)
    if windowCls ~= nil then
        local isOpen, isFocus = windowCls:__loadWindow(args, _obj) 
        if isOpen then
            local window = UIMgr:GetWindowCls("UILobbyWindow")
            if window ~= nil then
                window:HideGuideArrows(true)
            end
            --关闭场景中的物品弹窗
            if windowName ~= "UITaskRewardWindow" then
                UIMgr:SetMapItemPanel(nil)
            end

            openWindowList:Insert(1, windowName)
            windowCls:PlayOpenAnim()
            windowCls:OnOpen(args, isFocus, isAutoOpen)
        end
        return windowCls
    else
        Error("Cannot open none-exist Window[%s]", windowName)
        return nil
    end
end

--关闭Window
function UIMgr:CloseWindow(windowName)
    local windowCls = UIMgr:GetWindowCls(windowName)
    if windowCls ~= nil then
        if windowCls.__isCache then
            windowCls:__closeAndCacheWindow()
        else
            windowCls:__unloadWindow()
        end
        
        local name
        for i = 1, openWindowList.Count do
            name = openWindowList:Get(i)
            if windowName == name then
                openWindowList:RemoveAt(i)
                break
            end
        end
    else
        --Error("Cannot close none-exist Window[%s]", windowName)
    end
    CheckDelayOpenWindow()

    if not GuideManager.IsNeedWait(0) then
        local window = UIMgr:GetWindowCls("UILobbyWindow")
        if window ~= nil then
            window:HideGuideArrows(false)
        end
    end
end

function UIMgr:OnApplicationQuit()
    local name
    for i = 1, openWindowList.Count do
        name = openWindowList:Get(i)
        UIMgr:GetWindowCls(name):OnQuit()
    end
end

--关闭最顶层Window白名单
local CLOSE_TOP_WINDOW_WHITE_LIST = {
    ["UILobbyWindow"] = true,
    ["UIFrontMaskWindow"] = true,
    ["UIToastWindow"] = true,
    ["UICutSceneWindow"] = true,
    ["UITopHighLightWindow"] = true,
    ["UIHouseWindow"] = true,
}

--关闭全部Window白名单
local CLOSE_ALL_WINDOW_WHITE_LIST = {
    ["UIFrontMaskWindow"] = true,
    ["UITopHighLightWindow"] = true,
    ["UILobbyWindow"] = true,
}

local ESC_CLOSE_TOP_WINDOW_WHITE_LIST = {
    ["UIGameGuideWindow"] = true,
    ["UILoadingWindow"] = true,
    ["UIHouseWindow"] = true,
}

--关闭最顶层Window
function UIMgr:CloseTopWindow(isEsc)
    for i = 1, openWindowList.Count do
        if (not CLOSE_TOP_WINDOW_WHITE_LIST[openWindowList:Get(i)]) then
            if isEsc then
                if not ESC_CLOSE_TOP_WINDOW_WHITE_LIST[openWindowList:Get(i)] then
                    self:CloseWindow(openWindowList:Get(i))
                    return true
                end
            else
                self:CloseWindow(openWindowList:Get(i))
                return true
            end
        end
    end
    return false
end

--关闭所有Window
function UIMgr:CloseAllWindow(filterWindow)
    local windowName
    local windowCls
    for i = openWindowList.Count, 1, -1 do
        windowName = openWindowList:Get(i)
        if not CLOSE_ALL_WINDOW_WHITE_LIST[windowName] then
            if filterWindow == nil or filterWindow ~= windowName then
                windowCls = UIMgr:GetWindowCls(windowName)
                if windowCls ~= nil then
                    windowCls:__unloadWindow()
                    openWindowList:RemoveAt(i)
                end
            end
        end
    end
end

--获取Window数据
function UIMgr:GetWindowCls(windowName)
    if windowClsDic[windowName] ~= nil then
        return windowClsDic[windowName]
    else
        Error("Failed to get Window data. [%s] is none-exist", windowName)
        return
    end
end

--加载Window的Prefab
local lastPreLoadPrefabWindowList = {}
function UIMgr:PreLoadWindowByMapId(mapId)
    local windowNameList
    if mapId == MapId.Main then
        windowNameList = {
            "UICollectWindow",
            "UIOrderWindow",
            "UIOpenBoxWindow",
            "UINormalShopWindow",
            "UIDailyTaskWindow",
        }
    else
        windowNameList = {
            "UICollectWindow",
            "UINormalShopWindow",
        }
    end

    local window
    for _, windowName in ipairs(lastPreLoadPrefabWindowList) do
        window = self:GetWindowCls(windowName)
        window.__isCache = false
        if not UICompTool.IsObjDestroy(window:GetGameObject()) then
            window:DestroyEvents()
            window:UnBindCtrls()
            window:UnLoadPrefab()
            window:SetGameObject(nil)
            window.context = {}
        end
    end
    lastPreLoadPrefabWindowList = windowNameList
    for _, windowName in ipairs(windowNameList) do
        window = self:GetWindowCls(windowName)
        if UICompTool.IsObjDestroy(window:GetGameObject()) then
            window:PreLoadAtlas()
            window:LoadWindowPrefab()
            window:GetGameObject():SetActive(false)
            window:BindCtrls()
        end
        window:__setCacheWindow()
        window.__loadState = EnumWindowLoadStateType.PreLoad
    end
end

--获取顶层WindowName
function UIMgr:GetTopWindowName()
    for i = 1, openWindowList.Count do
        if not CLOSE_TOP_WINDOW_WHITE_LIST[openWindowList:Get(i)] then
            return openWindowList:Get(i)
        end
    end
    return nil
end

--添加Window数据
function UIMgr:AddWindowCls(windowName)
    if windowClsDic[windowName] ~= nil then
        Error("Window[%s] has been Registered", windowName)
        return
    else
        windowClsDic[windowName] = setmetatable({}, UIWindowBase)
        windowClsDic[windowName].__windowName = windowName
        return windowClsDic[windowName]
    end
end

--注册所有Window的全局事件
function UIMgr:RegisterAllWindowGlobalEvents()
    for _, windowCls in pairs(windowClsDic) do
        windowCls:RegisterGlobalEvents()
    end
end

--获取ControlCls数据
function UIMgr:GetControlCls(controlName)
    if controlClsDic[controlName] ~= nil then
        return controlClsDic[controlName]
    else
        Error("Failed to get Control data. [%s] is none-exist", controlName)
        return
    end
end

--获取ControlCls预制体
function UIMgr:GetControlPrefab(controlName)
    local ctrlCls = self:GetControlCls(controlName)
    if ctrlCls ~= nil then
        return ctrlCls:GetPrefabPath()
    end
end

--添加ControlCls数据
function UIMgr:AddControlCls(controlName, baseName)
    if windowClsDic[controlName] ~= nil then
        Error("Control[%s] has been Registered", controlName)
        return
    else
        local controlCls = {}
        controlCls.__index = controlCls
        if controlName == "UIGameObject" then
            UIGameObject = controlCls
            setmetatable(controlCls, UIControlBase)
        else
            local parentCls = self:GetControlCls(baseName or "UIGameObject")
            setmetatable(controlCls, parentCls)
            controlCls.__base = parentCls
        end
        controlCls.typeName = controlName
        controlClsDic[controlName] = controlCls
        return controlCls
    end
end

--获取UIGameObject基类
function UIMgr:GetUIGameObject()
    return UIGameObject
end

--Window是否已打开
function UIMgr:IsWindowOpen(windowName)
    local windowCls = self:GetWindowCls(windowName)
    if windowCls ~= nil then
        return windowCls:IsOpen()
    end
    return false
end

--显示前置蒙版
function UIMgr:ShowFrontMask(time)
    local frontMask = UIMgr:GetWindowCls("UIFrontMaskWindow")
    frontMask:ShowMask(time)
end

--获取最顶层Window
function UIMgr:GetTopWindow()
    for i = 1, openWindowList.Count do
        if not CLOSE_TOP_WINDOW_WHITE_LIST[openWindowList:Get(i)] then
            return UIMgr:GetWindowCls(openWindowList:Get(i))
        end
    end
end

function UIMgr:IsWaitWindow()
    if isWaitOpenWindow then
        return true
    end
    if (isCollectFlying == true) or (self:GetTopWindow() ~= nil) then
        return true
    end
    return false
end

local flyCount = 0
function UIMgr:SetIsCollectFlying(isFlying)
    if isFlying == false then
        flyCount = flyCount - 1
        if flyCount <= 0 then
            isCollectFlying = false
            --有藏品飞入动画未完成 所以延时一下
            local timer
            timer = TimerMgr:AddSingleTimer(1.8, function()
                CheckDelayOpenWindow()
                timer:Destroy()
            end)
        end
    else
        flyCount = flyCount + 1
        isCollectFlying = true
    end
end

function UIMgr:GetIsCollectFlying()
    return isCollectFlying
end

--打开Window
function UIMgr:SetWaitWindow(isWait)
    isWaitOpenWindow = isWait
end

function UIMgr:IsHaveDelayOpenWindow()
    return delayOpenWindowList.Count > 0
end

local _isEnterLoading = true
function UIMgr:EnterLoadingFinish()
    _isEnterLoading = false
    FireEvent(IEventId.ENTER_LOADING_FINISH)
end

function UIMgr:IsEnterLoading()
    return _isEnterLoading
end

--设置打开的地图物品界面
function UIMgr:SetMapItemPanel(uiPanel)
    if self.mapItemPanel ~= nil and self.mapItemPanel ~= uiPanel then
        self.mapItemPanel:HideUI()
    end
    self.mapItemPanel = uiPanel
end

--Window单例
_G.UIWindow = function(windowName)
    return UIMgr:AddWindowCls(windowName)
end

--Control继承
_G.UIControl = function(controlName, baseName)
    return UIMgr:AddControlCls(controlName, baseName)
end

--打开Window
_G.OpenWindow = function(windowName, ...)
    return UIMgr:OpenWindow(windowName, ...)
end

--关闭Window
_G.CloseWindow = function(windowName)
    UIMgr:CloseWindow(windowName)
end

--关闭所有Window
_G.CloseAllWindow = function(filterWindow)
    UIMgr:CloseAllWindow(filterWindow)
end

--关闭最顶层Window
_G.CloseTopWindow = function(isEsc)
    return UIMgr:CloseTopWindow(isEsc)
end

--获取最顶层Window
_G.GetTopWindow = function()
    return UIMgr:GetTopWindow()
end