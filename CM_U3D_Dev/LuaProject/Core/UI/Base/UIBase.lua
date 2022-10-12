--UI控件基类 Control和Window和WindowCell实质上都是基于此类

--[[
    UIBase为所有UI的基本类
    UIWindowBase为UI界面的基本类 继承UIBase
    UIControlBase为UI控件的基本类 继承UIBase
    UIGameObject为UI控件C#功能的基本类 继承UIControlBase(UIBase)
]]--

--[[
    关于Config中的配置
    
    prefabPath
    预制体路径 位于WindowPrefab目录下的绝对路径
    UIWindow必须要填写

    [无实装]
    layerDepth
    预制体层级 
    UIWindow必须要填写
    UIControl不需要填写

    ctrls
    {
        type = "Type",
        path = "Path",
        name = "Name",
        nilable = true / false,
        events = {},
        cell = {
            type = "CellType",
            events = {},
        },
    }
    type 是ctrl的类型 与UIControl("Type")中的Type一致 如果不存在或者不填则自动转为[UIGameObject]
    path 是ctrl的路径 如果不填或填空字符串("")则为控件自身
    name 是ctrl的名称 如果不填则自动通过下标填充
    nilable 是ctrl是否允许为空的关键字 如果此为true且不存在 记得判断一下是否为空
    events 是ctrl的事件集合 详见UIControlEventBind中的配置与控件的Message中的时间名
    cell 是针对如UILayoutGroup这种列表的子控件相关

    eventHandler
    {
        ["EventHandlerType"] = "Func",
    }
    此表为control自身的事件集合
    EventHandlerType 是事件类型 详见UIControlEventBind中的[AnyType]配置
    Func 是control中对应的方法名

    messages
    {
        ["MessageName"] = [AnyValue],
    }
    此表为control自定义向上一级发送事件的声明表
    MessageName 是自定义消息名称
    [AnyValue] 可以为任意非nil非false的值 只是用来做字典而已

    events
    {
        ["EventName"] = "Func",
    }
    此表为window/control监听发出事件的集合
    只在window开启后以及control实例了之后才会生效
    EventName 是监听的事件的名称
    Func 是window/control中对应的方法名

    [无实装]
    globalEvents
    {
        ["GlobalEventName"] = "Func",
    }
    此表为window全局监听发出时间的集合
    只在window中可用 无论window是否已经打开都会相应
    此部分事件不需要销毁
    GlobalEventName 是监听的事件的名称
    Func 是window中对应的方法名
]]--

UIBase = {}
UIBase.__index = UIBase

UIBase.__gameObject = nil
UIBase.__config = {}

UIBase.__eventList = {}
UIBase.__timerList = {}
UIBase.__coroutineList = {}

UIBase.ctrls = {}
UIBase.context = {}

--Prefab目录
local UI_PREFAB_ROOT_PATH = "UIPanel/"

--设置GameObject
function UIBase:SetGameObject(gameObject)
    self.__gameObject = gameObject
end

--获取GameObject
function UIBase:GetGameObject()
    return self.__gameObject
end

--获取Transform
function UIBase:GetTransform()
    return self.__gameObject.transform
end

--设置父节点
function UIBase:SetParent(trans)
    self:GetTransform():SetParent(trans);
end

--获取RectTransform
function UIBase:GetRectTransform()
    return GameObjectUtil.GetRectTransformComp(self.__gameObject)
end

--移除GameObject
function UIBase:ClearGameObject()
    self.__gameObject = nil
end

--加载Prefab
function UIBase:LoadPrefab(parentRoot, prefabPath)
    local prefab = ResMgr.LoadPrefabs(UI_PREFAB_ROOT_PATH .. (prefabPath or ""))
    return GameObjectUtil.Instantiate(prefab, parentRoot)
end

--移除Prefab
function UIBase:UnLoadPrefab()
    Destroy(self:GetGameObject())
end

--移除Prefab
function UIBase:CachePrefab()
    self:GetGameObject():SetActive(false);
end

--设置GameObject名称
function UIBase:SetGameObjectName(name)
    self:GetGameObject().name = name
end

--获取GameObject名称
function UIBase:GetGameObjectName()
    return self:GetGameObject().name
end

--获取UIBase名称（预留 在子类中确定具体返回值）
function UIBase:GetUIBaseName() end

--加载配置项
function UIBase:Config(config)
    if self.__config ~= nil then
        local sourceConfig = table.copy(self.__config)
        local curConfig = table.copy(config)
        for k, v in pairs(sourceConfig) do
            if type(v) == "table" then
                if k == "ctrls" then
                    sourceConfig.ctrls = table.append(v, curConfig.ctrls or {}), true
                else
                    sourceConfig[k] = table.merge(v, curConfig[k] or {}, true)
                end
            else
                if config[k] ~= nil then
                    sourceConfig[k] = config[k]
                end
            end
            curConfig[k] = nil
        end
        for k, v in pairs(curConfig) do
            sourceConfig[k] = v
        end
        self.__config = sourceConfig
    else
        self.__config = config
    end
end

--获取PrefabPath
function UIBase:GetPrefabPath()
    return self.__config.prefabPath
end

--获取Message列表
function UIBase:GetMessageList()
    return self.__config.messages
end

--注册事件
function UIBase:RegisterEvents()
    if self.__config.events ~= nil then
        self.__eventList = {}
        for eventName, funcName in pairs(self.__config.events) do
            local func = self[funcName]
            if func ~= nil then
                table.insert(self.__eventList, EventMgr:AddEvent(eventName, func, self))
            else
                Error("Event[%s] in [%s] has no function [%s]", eventName, self:GetUIBaseName(), funcName)
            end
        end
    end
end

--移除事件
function UIBase:DestroyEvents()
    for index, event in ipairs(self.__eventList) do
        event:Destroy()
    end
    self.__eventList = {}
end

--创建空计时器
function UIBase:CreateTimer()
    local timer = TimerMgr:CreateTimer()
    table.insert(self.__timerList, timer)
    return timer
end

--添加普通计时器
function UIBase:AddTimer(repeatCount, repeatInterval, callBackFunc)
    local timer = TimerMgr:AddTimer(repeatCount, repeatInterval, callBackFunc)
    table.insert(self.__timerList, timer)
    return timer
end

--添加无限计时器
function UIBase:AddLoopTimer(repeatInterval, callBackFunc)
    local timer = TimerMgr:AddLoopTimer(repeatInterval, callBackFunc)
    table.insert(self.__timerList, timer)
    return timer
end

--添加单次计时器
function UIBase:AddSingleTimer(waitTime, callBackFunc)
    local timer = TimerMgr:AddSingleTimer(waitTime, callBackFunc)
    table.insert(self.__timerList, timer)
    return timer
end

--移除计时器
function UIBase:DestroyTimerList()
    for index, timer in ipairs(self.__timerList) do
        timer:Destroy()
        self.__timerList[index] = nil
    end
    for index, coroutine in ipairs(self.__coroutineList) do
        MonoUtil.Get():Stop(coroutine)
        self.__coroutineList[index] = nil
    end
end

--获得子控件
function UIBase:FindChild(go, path, nilable)
    if go == nil then
        Error("UIBase:FindChild() gameObject is nil")
        return
    end

    if path == "" then
        return go
    end

    local rectTrans = go.transform:Find(path)
    if rectTrans ~= nil then
        return rectTrans.gameObject
    else
        if not nilable then
            Error("Cannot find child[%s] at [%s]", go.name, path)
        end
        return
    end
end

--实例化Control
function UIBase:NewSingleCtrl(name, type, gameObject)
    local baseCls = UIMgr:GetControlCls(type)
    if baseCls == nil then
        Error("Control[%s] of [%s] is a none-exist type[%s]. chenge it to UIGameObject", name, self:GetUIBaseName(), type)
        type = "UIGameObject"
        baseCls = UIMgr:GetUIGameObject()
    end
    return baseCls:New(name, gameObject, type, self)
end

--绑定子控件
function UIBase:BindCtrls()
    local rootGo = self:GetGameObject()
    if rootGo ~= nil then
        self.ctrls = {}
        for _, config in ipairs(self.__config.ctrls or {}) do
            config.type = config.type or "UIGameObject"
            local ctrlGo = self:FindChild(rootGo, config.path or "", config.nilable)
            if ctrlGo ~= nil then
                local ctrl = self:NewSingleCtrl(config.name or "self.gameObject", config.type, ctrlGo)
                if ctrl ~= nil then
                    ctrl:LoadCtrl(self, config)
                end
                self.ctrls[config.name or #self.ctrls] = ctrl
            end
        end
    else
        Error("[%s] has not loaded Prefab", self:GetUIBaseName() or "self.gameObject")
    end
end

--移除子控件
function UIBase:UnBindCtrls()
    for _, ctrl in pairs(self.ctrls) do
        ctrl:UnloadCtrl()
        ctrl:ClearGameObject()
    end
    self.ctrls = {}
end
