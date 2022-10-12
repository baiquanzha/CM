local SceneMgr = {}

local sceneClsDic = {}
local curSceneCls

function SceneMgr:PlayStartScene(sceneName)
    local sceneCls = SceneMgr:GetGameScene(sceneName)
    if sceneCls ~= nil then
        curSceneCls = sceneCls
        curSceneCls:OnEnter()
    else
        Error("Cannot enter a none-exist scene[%s]", sceneName)
    end
end

function SceneMgr:ChangeScene(sceneName)
    if (curSceneCls:GetSceneName() ~= sceneName) then
        local sceneCls = SceneMgr:GetGameScene(sceneName)
        if sceneCls ~= nil then
            CloseAllWindow("UILoadingWindow")
            curSceneCls:OnExit()
            --异步加载
            SceneUtil.ChangeScene(sceneCls.sceneFileName, function()
                curSceneCls = sceneCls
                curSceneCls:OnEnter()
            end)
        else
            Error("Cannot enter a none-exist scene[%s]", sceneName)
        end
    end
end

function SceneMgr:GetGameScene(sceneName)
    if sceneClsDic[sceneName] ~= nil then
        return sceneClsDic[sceneName]
    else
        Error("Failed to get GameScene data. [%s] is none-exist", sceneName)
    end
end

function SceneMgr:AddGameScene(sceneName)
    if sceneClsDic[sceneName] == nil then
        local sceneCls = setmetatable({
            name = sceneName
        }, SceneBase)
        sceneCls.name = sceneName
        sceneClsDic[sceneName] = sceneCls
        return sceneCls
    else
        Error("GameScene[%s] has been Registered", sceneName)
    end
end

_G.GetCurScene = function()
    return curSceneCls
end

_G.GameScene = function(sceneName)
    return SceneMgr:AddGameScene(sceneName)
end

_G.ChangeScene = function(sceneName, isForce)
    SceneMgr:ChangeScene(sceneName, isForce)
end

_G.PlayStartScene = function(sceneName)
    SceneMgr:PlayStartScene(sceneName)
end