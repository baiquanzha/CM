require "gen/init_root"
require "protokithelp/protokithttp"
require "protokithelp/protokitclient"
require "Game/Net/NetMgr"
require "Game/Net/SocketMgr"

local MainScene = GameScene("MainScene")
MainScene.sceneFileName = "Main"

local isInit = false;

function MainScene:OnEnter()
    isInit = true;
    print("-----------MainScene:OnEnter")
    gen:initAll()

    --http请求
    -- NetMgr:Init()
    -- NetMgr:SendAuthorizeReq("", function ()
    --     print("-----------SendAuthorizeReq:suc")
    -- end)

    SocketMgr.Init()
    SocketMgr.ConnectToServer()
end

function MainScene:OnExit()
    Debug("离开MainScene")
end