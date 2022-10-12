UIControlBase = {}
UIControlBase.__index = UIControlBase
setmetatable(UIControlBase, UIBase)

UIControlBase.__ctrlName = ""
UIControlBase.__typeName = ""
UIControlBase.__parent = nil
UIControlBase.__loadConfig = nil
UIControlBase.__updateIndex = nil
UIControlBase.__timerList = {}
UIControlBase.__coroutineList = {}
UIControlBase.__messageFunc = {}

--实例化一个ControlCls
function UIControlBase:New(name, gameObject, type, parent)
    if gameObject ~= nil then
        return setmetatable({
            __ctrlName = name,
            __typeName = type,
            __gameObject = gameObject,
            __parent = parent,
            __timerList = {},
            __coroutineList = {},
            __messageFunc = {},
        }, self)
    else
        return
    end
end

--加载Control
function UIControlBase:LoadCtrl(parentCls, config)
    --加载配置表
    self.__loadConfig = config
    
    --绑定子控件
    self:BindCtrls()

    --绑定EventHandler
    self:BindEventHandler(parentCls, config)

    --绑定对象自身的EventHandler
    self:BindRootEventHandler()

    --注册事件
    self:RegisterEvents()

    --加载完成
    self:OnLoaded()

    --Update注册
    self:RegisterUpdate()

    --局变量存储表
    self.context = {}
end

--销毁Control
function UIControlBase:UnloadCtrl()
    --移除Update
    self:DestroyUpdate()

    --移除子控件
    self:UnBindCtrls()

    --移除EventHandler
    self:UnBindEventHandler()

    --移除事件
    self:DestroyEvents()

    --移除指定消息的回调
    self:ClearAllMessageFunc()

    --移除计时器
    self:DestroyTimerList()

    --销毁完成
    self:OnUnloaded()

    --清空局变量存储表
    self.context = {}
end

--加载完成（预留）
function UIControlBase:OnLoaded() end

--销毁完成（预留）
function UIControlBase:OnUnloaded() end

--获取TypeName
function UIControlBase:GetUIBaseName()
    return self.__typeName
end

--获取父级UIBase
function UIControlBase:GetParent()
    return self.__parent
end

--获取所在的Window
function UIControlBase:GetWindow()
    local parent = self.__parent
    while true do
        if parent.__isWindow then
            return parent
        else
            parent = parent.__parent 
        end
    end
end

--绑定EventHandler
function UIControlBase:BindEventHandler(parentCls, config)
    UIControlEventBind:BindEvent(parentCls, config, self)
end

--移除EventHandler
function UIControlBase:UnBindEventHandler()
    UIControlEventBind:UnBindEvent(self)
end

--绑定对象自身的EventHandler 自身只可以绑定AnyType中的类型 所以不需要做移除处理
function UIControlBase:BindRootEventHandler()
    UIControlEventBind:BindEvent(self, {events = self.__config.eventHandler, type = self.baseTypeName}, self)
end

--注册Update
function UIControlBase:RegisterUpdate()
    if self.Update ~= nil then
        self.__updateIndex = MainUpdate:AddCtrlUpdateFunc(self.Update, self)
    end
end

--移除Update
function UIControlBase:DestroyUpdate()
    if self.__updateIndex ~= nil then
        MainUpdate:RemoveCtrlUpdateFunc(self.__updateIndex)
        self.__updateIndex = nil
    end
end

--发送消息到父类的events中
function UIControlBase:PostMessage(messageName, ...)
    if (self.__config.messages ~= nil) and self.__config.messages[messageName] then
        local parent = self.__parent
        --针对列表类型的组件做特殊处理 因为列表类型不需要接收消息 所以直接向上传送
        if (parent.baseTypeName == "UILayoutGroup") or (parent.baseTypeName == "UILoopScrollRect") then
            parent = parent.__parent
        end
        if (self.__loadConfig ~= nil) and (self.__loadConfig.events ~= nil) and (self.__loadConfig.events[messageName] ~= nil) then
            parent[self.__loadConfig.events[messageName]](parent, ...)
        end
    else
        Error("Message[%s] in [%s] is not exist", messageName, self:GetUIBaseName())
    end

    if self.__messageFunc[messageName] ~= nil then
        for _, func in ipairs(self.__messageFunc[messageName]) do
            func(...)
        end
    end
end

--注册指定消息的回调
function UIControlBase:SetMessageFunc(messageName, func)
    if self.__messageFunc[messageName] == nil then
        self.__messageFunc[messageName] = {}
    end
    table.insert(self.__messageFunc[messageName], func)
end

function UIControlBase:ClearMessageFunc(messageName)
    self.__messageFunc[messageName] = nil
end

function UIControlBase:ClearAllMessageFunc()
    self.__messageFunc = {}
end