Config = {}

local function LoadConfig(fileName)
    return require(string.format("gen/conf/rawdata/%s.lua", fileName))
end

function Config.GetConfigs(fileName)
    return LoadConfig(fileName)
end

function Config.InitConfig()
    
end

local tempCount
local tempCof