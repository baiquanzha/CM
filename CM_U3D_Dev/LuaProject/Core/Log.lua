local tableLayerNum = 0
local Log = CS.UnityEngine.Debug.Log
local LogWarning = CS.UnityEngine.Debug.LogWarning
local LogError = CS.UnityEngine.Debug.LogError

local BLACK_LIST_KEY = {
    ["__parent"] = true,
    ["__index"] = true,
    ["__cls"] = true,
}

local function GenerateStr(data, ...)
    if tableLayerNum > 10 then
        return "..."
    end

    if data == nil then
        return "nil"
    elseif type(data) == "string" then
        local argsTb = {...}
        if #argsTb == 0 then
            return "\"" .. data .. "\""
        else
            --多参数但是并不存在%s %d %f等 跳到尾部的后部补充拼接
            if string.find(data, "%%") then
                return string.format("\"" .. data .. "\"", ...)
            end
        end
    elseif type(data) == "function" then
        return "[func]"
    elseif type(data) == "table" then
        local argsTb = {...}
        --多参数的话 直接跳到尾部的后部补充拼接
        if #argsTb == 0 then
            local str = "{"
            tableLayerNum = tableLayerNum + 1
            local spaceStr = ""
            for i = 1, tableLayerNum do
                spaceStr = spaceStr .. "    "
            end
            for k, v in pairs(data) do
                local keyStr = tostring(k)
                if type(k) == "string" then
                    keyStr = "\"" .. keyStr .. "\""
                end
                if BLACK_LIST_KEY[k] then
                    str = str .. "\n" .. spaceStr .. "[" .. keyStr .. "] = " .. tostring(v)
                else
                    str = str .. "\n" .. spaceStr .. "[" .. keyStr .. "] = " .. GenerateStr(v)
                end
            end
            tableLayerNum = tableLayerNum - 1
            spaceStr = ""
            for i = 1, tableLayerNum do
                spaceStr = spaceStr .. "    "
            end
            if tableLayerNum == 0 then
                str = "[table]\n" .. str
            end
            str = str .. "\n" .. spaceStr .. "}"
            return str
        end
    end

    --多参数尾部补充拼接
    local argsTb = {...}
    local str = tostring(data)
    for _, v in ipairs(argsTb) do
        str = str .. " " .. tostring(v)
    end
    return str
end

_G.Debug = function(data, ...)
    Log(GenerateStr(data, ...).. "\n" .. debug.traceback())
end

_G.Warning = function(data, ...)
    LogWarning(GenerateStr(data, ...).. "\n" .. debug.traceback())
end

_G.Error = function(data, ...)
    LogError(GenerateStr(data, ...) .. "\n" .. debug.traceback())
end

_G.UDebug = function(msg, obj)
    Log(msg, obj)
end

_G.UWarning = function(msg, obj)
    LogWarning(msg, obj)
end

_G.UError = function(msg, obj)
    LogError(msg, obj)
end

_G.PCALL = function(func)
    local succ, errorInfo = pcall(func)
    if not succ then
        Error(errorInfo .. "\n" .. debug.traceback())
    end
end

-- if not GameDebugMode then
--     _G.Debug = function() end
--     _G.Warning = function() end
--     _G.Error = function(data)
--         LogError(data .. "\n" .. debug.traceback())
--     end
-- end