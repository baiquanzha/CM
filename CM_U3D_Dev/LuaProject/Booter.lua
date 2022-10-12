Booter = {
    appPauseTime = 0,   --进入后台时间
    hideBackTime = 0,   --后台累计时间
    isEditor = false,
}

-- gc
collectgarbage("setpause", 200)
collectgarbage("setstepmul", 200)

-- load core lua
require("Core/Log.lua")
require("Core/Loader.lua")
require("Core/CoreProject.lua")
LoadLuaProject()

-- load all lua
require("Project.lua")
LoadLuaProject()

-- clear _G.GetProject
_G.GetProject = nil

-- start
function Booter:Start()
    ILog.logger = GameLauncher.Get().logger;
    PlayStartScene("MainScene")
end

-- update
function Booter:Update()
    MainUpdate:Update()
end

function Booter:LateUpdate()
    MainUpdate:LateUpdate()
end

function Booter:FixedUpdate()
    MainUpdate:FixedUpdate()
end

function Booter:OnDisable()
   print("----------Booter.OnDisable") 
end

function Booter:OnApplicationQuit()
    print("---------Booter.OnApplicationQuit");
    
end

function Booter:OnApplicationPause(pauseStatus)
    print("---------Booter.OnApplicationPause pauseStatus = "..tostring(pauseStatus));
    
end



--return for cs
return Booter