SceneBase = {}
SceneBase.name = ""
SceneBase.__index = SceneBase

function SceneBase:OnEnter() end

function SceneBase:OnExit() end

function SceneBase:GetSceneName()
    return self.name
end