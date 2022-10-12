local GameScene = GameScene("GameScene")
GameScene.sceneFileName = "GameLua"

local window;
function GameScene:OnEnter()
    Debug("进入Scene Game")

    window = UIMgr:GetWindowCls("UILoadingWindow")
    window:SetProcess(20);
end

function GameScene:GameLogin()
    lobbyWindow = OpenWindow("UILobbyWindow")
    lobbyWindow:SetEnterPreShow()
    CS.System.GC.Collect();
end

function GameScene:OnExit()
    Debug("离开Scene Game")
end

return GameScene;