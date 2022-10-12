--UIControl的CtrlCfg中event绑定
UIControlEventBind = {}

--绑定EventHandler
local EVENT_HANDLER_BIND_MAPPING = {
    ["AnyType"] = {
        -- ["OnBeginDrag"] = {
        --     func = GameObjectHelper.AddBeginDragHandler,
        -- },
        -- ["OnDrag"] = {
        --     func = GameObjectHelper.AddDragHandler,
        -- },
        -- ["OnEndDrag"] = {
        --     func = GameObjectHelper.AddEndDragHandler,
        -- },
        -- ["OnPointerEnter"] = {
        --     func = GameObjectHelper.AddPointerEnterHandler,
        -- },
        -- ["OnPointerExit"] = {
        --     func = GameObjectHelper.AddPointerExitHandler,
        -- },
        -- ["OnPointerClick"] = {
        --     func = GameObjectHelper.AddPointerClickHandler,
        -- },
        -- ["OnPointerDown"] = {
        --     func = GameObjectHelper.AddPointerDownHandler,
        -- },
        -- ["OnPointerUp"] = {
        --     func = GameObjectHelper.AddPointerUpHandler,
        -- },
    },
    -- ["UIButton"] = {
    --     ["OnClick"] = {
    --         func = GameObjectUtil.AddClickFunc,
    --     },
    -- },
    -- ["UIToggle"] = {
    --     ["OnValueChanged"] = {
    --         func =  GameObjectHelper.AddToggleValueChangedFunc,
    --         argCount = 1,
    --     },
    -- },
    ["UIInputField"] = {
        ["OnValueChanged"] = {
            func = GameObjectUtil.AddInputValueChangedFunc,
            argCount = 1,
        },
        ["OnEndEdit"] = {
            func = GameObjectUtil.AddInputEndEditFunc,
            argCount = 1,
        },
    },
    ["UISlider"] = {
        ["OnValueChanged"] = {
            func = GameObjectUtil.AddSliderValueChangedFunc,
            argCount = 1,
        },
    }
}

local function BindEventKey(funcData, parentCls, funcName, gameObject)
    if (funcData.argCount == nil) or (funcData.argCount == 0) then
        funcData.func(gameObject, function()
            parentCls[funcName](parentCls)
        end)
    elseif funcData.argCount == 1 then
        funcData.func(gameObject, function(arg)
            parentCls[funcName](parentCls, arg)
        end)
    elseif funcData.argCount == 2 then
        funcData.func(gameObject, function(arg1, arg2)
            parentCls[funcName](parentCls, arg1, arg2)
        end)
    elseif funcData.argCount == 3 then
        funcData.func(gameObject, function(arg1, arg2, arg3)
            parentCls[funcName](parentCls, arg1, arg2, arg3)
        end)
    elseif funcData.argCount == 4 then
        funcData.func(gameObject, function(arg1, arg2, arg3, arg4)
            parentCls[funcName](parentCls, arg1, arg2, arg3, arg4)
        end)
    end
end

function UIControlEventBind:BindEvent(parentCls, config, control)
    if (config ~= nil) and (config.events ~= nil) then
        for type, keyTable in pairs(EVENT_HANDLER_BIND_MAPPING) do
            if (type == "AnyType") or (type == config.type) then
                for key, funcData in pairs(keyTable) do
                    if config.events[key] ~= nil then
                        local funcName = config.events[key]
                        if parentCls[funcName] ~= nil then
                            BindEventKey(funcData, parentCls, funcName, control:GetGameObject())
                        else
                            Error("Control[%s] in [%s], its event[%s] has no function[%s]", config.__name or control:GetGameObject().name, parentCls:GetUIBaseName(), key, funcName)
                        end
                    end
                end
            end
        end
    end
end

local function BindNewEvent(funcData, func, gameObject)
    if (funcData.argCount == nil) or (funcData.argCount == 0) then
        funcData.func(gameObject, function()
            func()
        end)
    elseif funcData.argCount == 1 then
        funcData.func(gameObject, function(arg)
            func(arg)
        end)
    elseif funcData.argCount == 2 then
        funcData.func(gameObject, function(arg1, arg2)
            func(arg1, arg2)
        end)
    elseif funcData.argCount == 3 then
        funcData.func(gameObject, function(arg1, arg2, arg3)
            func(arg1, arg2, arg3)
        end)
    elseif funcData.argCount == 4 then
        funcData.func(gameObject, function(arg1, arg2, arg3, arg4)
            func(arg1, arg2, arg3, arg4)
        end)
    end
end

--代码中添加
function UIControlEventBind:BindNewEvent(func, control, eventName)
    if EVENT_HANDLER_BIND_MAPPING["AnyType"][eventName] ~= nil then
        BindNewEvent(EVENT_HANDLER_BIND_MAPPING["AnyType"][eventName], func, control:GetGameObject())
    elseif (EVENT_HANDLER_BIND_MAPPING[control.typeName] ~= nil) and (EVENT_HANDLER_BIND_MAPPING[control.typeName][eventName] ~= nil) then
        BindNewEvent(EVENT_HANDLER_BIND_MAPPING[control.typeName][eventName], func, control:GetGameObject())
    end
end

local EVENT_HANDLER_UNBIND_MAPPING = {
    -- ["UIButton"] = {
    --     GameObjectUtil.ClearClickFunc,
    -- },
    -- ["UIToggle"] = {
    --     GameObjectHelper.ClearToggleValueChangedFunc,
    -- },
    ["UIInputField"] = {
        GameObjectUtil.ClearInputValueChangedFunc,
        GameObjectUtil.ClearInputEndEditFunc,
    },
    ["UISilder"] = {
        GameObjectUtil.ClearSliderValueChangedFunc,
    },
}

function UIControlEventBind:UnBindEvent(control)
    if EVENT_HANDLER_UNBIND_MAPPING[control.typeName] ~= nil then
        for _, func in ipairs(EVENT_HANDLER_UNBIND_MAPPING[control.typeName]) do
            func(control:GetGameObject())
        end
    end
end