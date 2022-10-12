MainUpdate = {
    IsEditor = false,
}

--检查是否按下键鼠
local function CheckIsKeyDown()
    if Input.GetKeyDown(CS.UnityEngine.KeyCode.Escape) then
        --引导中返回键不关闭
        if GuideManager.curGuide ~= nil then
            return;
        end
        if not UIMgr:GetWindowCls("UIFrontMaskWindow"):IsShow() then
            if not CloseTopWindow(true) then
                local window = UIMgr:GetWindowCls("UIConfirmWindow")
                if UICompTool.IsObjDestroy(window:GetGameObject()) then
                    OpenWindow("UIConfirmWindow", {"Quit", "Are you sure to quit the game?", function()
                        Application.Quit();
                    end, nil, true})
                end
            end
        end
    end

    if Booter.isEditor then
        if Input.GetKeyDown("f9") then
            local window = UIMgr:GetWindowCls("UIDebugWindow")
            if window:IsOpen() then
                CloseWindow("UIDebugWindow")
            else
                OpenWindow("UIDebugWindow")
            end
        elseif Input.GetKeyDown("f10") then
            local window = UIMgr:GetWindowCls("UIChapterWindow")
            if not window:IsOpen() then
                ChapterMgr:StartChapterById(20)
            end
        elseif Input.GetKeyDown("f11") then
            -- --重置每日任务
            -- DailyTaskMgr:ResetDailyTaskByNewDay()
            -- if UIMgr:IsWindowOpen("UIDailyTaskWindow") then
            --     CloseWindow("UIDailyTaskWindow")
            -- end

            -- --获得奖励
            -- OpenWindow("UIRewardPropWindow", {
            --     rewards = {{1, 100}},
            -- })

            -- --打开刮刮乐
            -- OpenWindow("UIStrengthScratchCardWindow")
            
            -- -- 结束竞速赛
            -- local actData = ActivityMgr:GetScoreRace(EnumActScoreRaceType.Merge5To2)
            -- actData:AddPlayerScore(100)
            -- actData:RaceTimeEnd()
        end
    end
end

--Update
local updateFuncDic = IDictionary:new();
local updateFuncList = IList:new();
function MainUpdate:AddUpdateFunc(windowName, func, windowCls)
    updateFuncDic:Add(windowName, {func = func, cls = windowCls});
end

function MainUpdate:RemoveUpdateFunc(windowName)
    updateFuncDic:Remove(windowName);
end

function MainUpdate:AddCtrlUpdateFunc(func, cls)
    local funcData = {func = func, cls = cls};
    updateFuncList:Add(funcData);
    return funcData
end

function MainUpdate:RemoveCtrlUpdateFunc(func)
    updateFuncList:Remove(func);
end

function MainUpdate:RemoveAllUpdateFunc()
    updateFuncDic:Clear();
    updateFuncList:Clear();
end

local funcData;
function MainUpdate:Update()
    CheckIsKeyDown()
    TimerMgr:Update()
    for i = updateFuncDic.tableCount, 1, -1 do
        funcData = updateFuncDic.list[i].value;
        funcData.func(funcData.cls)
    end

    for i = updateFuncList.Count, 1, -1 do
        funcData = updateFuncList:Get(i);
        funcData.func(funcData.cls)
    end
end

--LateUpdate
--local lateUpdateFuncList = {}

--function MainUpdate:AddLateUpdateFunc(windowName, func, windowCls)
--    lateUpdateFuncList[windowName] = {func = func, cls = windowCls}
--end

--function MainUpdate:RemoveLateUpdateFunc(windowName)
--    lateUpdateFuncList[windowName] = nil
--end

--function MainUpdate:RemoveAllLateUpdateFunc()
--    lateUpdateFuncList = {}
--end

function MainUpdate:LateUpdate()
--    for _, funcData in pairs(lateUpdateFuncList) do
--        funcData.func(funcData.cls)
--    end
end

--FixedUpdate
--local fixedUpdateFuncList = {}

--function MainUpdate:AddFixedUpdateFunc(windowName, func, windowCls)
--    fixedUpdateFuncList[windowName] = {func = func, cls = windowCls}
--end

--function MainUpdate:RemoveFixedUpdateFunc(windowName)
--    fixedUpdateFuncList[windowName] = nil
--end

--function MainUpdate:RemoveAllFixedUpdateFunc()
--    fixedUpdateFuncList = {}
--end

function MainUpdate:FixedUpdate()
--    for _, funcData in pairs(fixedUpdateFuncList) do
--        funcData.func(funcData.cls)
--    end
end