local UILoadingWindow = UIWindow("UILoadingWindow")
UILoadingWindow:Config({
    prefabPath = "Loading/UILoadingWindow",
    windowType = UI_WINDOW_TYPE.TOP_WINDOW,
    --preloadAtlas = {"ui_loading"},
    ctrls = {
         {
            path = "goRoot",
            name = "goRoot",
        },
        {
            path = "image",
            name = "imgBg",
        },
        {
            type = "UIText",
            path = "goRoot/img_bar/txt_process",
            name = "txtProcess",
        },
        {
            type = "UIText",
            path = "goRoot/img_bar/txt_desc",
            name = "txtDesc",
        },
        {
            type = "UISlider",
            path = "goRoot/img_bar/img_bar_slider",
            name = "barSlider",
        },
        {
            type = "UIButton",
            path = "btnPlay",
            name = "btnPlay",
            events = {
                OnClick = "OnPlayClick",
            },
        },
    }
})

local processFuncList = IList:new();
local waitFuncList = IList:new();
local waitProcessFunc = nil;
local downloadService = nil

local mProgressData = nil
local totalDownloadSizeStr
local totalDownloadSize = 0
local lastDownSize = 0
local localdownFileSize = 0
local iType = 0

local downFileTime = 0;         --下载文件时间
local downSpeed = 200 * 1024;   --下载速度
local downFileSize = 0;         --当前文件下载

function UILoadingWindow:OnOpen()
    --MainUpdate:AddUpdateFunc("UILoadingWindow", self.Update, self)
    self.context.isClose = false;
    self.context.curProcess = 0;
    self.context.maxProcess = 0;
    self.context.isStart = false;
    self.ctrls.txtDesc:SetText("");
    self:UpdateTimeUI();

    processFuncList:Clear();
    waitProcessFunc = nil;

    local scale = 1;
    if LGameAdapt.matchWidthOrHeight == 1 then
        local width = LGameAdapt.pixelScale * 2336;
        if Screen.width > width then
            scale = Screen.width * 1 / width;
        end
    else
        local height = LGameAdapt.pixelScale * 1080;
        if Screen.height > height then
            scale = Screen.height * 1 / height;
        end
    end
    self.ctrls.imgBg:GetTransform().localScale = Vector3.one * scale;
end

function UILoadingWindow:ResetInit()
    self.context.curProcess = 0;
    self.context.maxProcess = 0;
    self.ctrls.txtProcess:SetText("0%");
    self.ctrls.barSlider:SetValue(0);
end

function UILoadingWindow:Update()
    if mProgressData ~= nil then
        if iType == 0 then
            if mProgressData.TotalDownloadSize > 0 then
                initProcess = 1;
                if mProgressData.TotalDownloadSize > 100 * 1024 then
                    iType = 1;
                elseif mProgressData.TotalDownloadSize > 100 then
                    iType = 2;
                else
                    iType = 3;
                end
                totalDownloadSizeStr = self:GetSizeStr(iType, mProgressData.TotalDownloadSize);
                totalDownloadSize = mProgressData.TotalDownloadSize;
                lastDownSize = mProgressData.CurrentDownloadSize;
                downFileSize = 0;
            end
        else
            self:UpdateDownLoad();
        end
        --mProgressData:Update()
    end

    if self.context.curProcess < self.context.maxProcess then
        self.context.curProcess = self.context.curProcess + Time.deltaTime * 80;
        if self.context.curProcess > self.context.maxProcess then
            self.context.curProcess = self.context.maxProcess;
        end
        self:UpdateTimeUI();
    else
        --执行完所有方法关闭界面
        if self.context.curProcess >= 100 and waitProcessFunc == nil and
                processFuncList.Count == 0 and waitFuncList.Count == 0 then
            --print("---------self.context.finishFunc = "..tostring(self.context.finishFunc));
            if self.context.finishFunc ~= nil then
                self.context.finishFunc();
                self.context.finishFunc = nil;
            end

            if self.context.isClose then
                self:CloseSelfWindow()
            end
        end
    end

    if waitFuncList.Count > 0 then
        waitFuncList:Get(1)()
        waitFuncList:RemoveAt(1)
    end

    if waitProcessFunc ~= nil then
        if self.context.curProcess >= waitProcessFunc[1] then
            waitProcessFunc[2]();
            waitProcessFunc = nil;
        end
    else
        if processFuncList.Count > 0 then
            waitProcessFunc = processFuncList:Get(1);
            processFuncList:RemoveAt(1);
        end
    end
end

function UILoadingWindow:UpdateTimeUI()
    self.ctrls.txtProcess:SetText(math.floor(self.context.curProcess).."%");
    self.ctrls.barSlider:SetValue(self.context.curProcess / 100);
end

function UILoadingWindow:SetProcess(process)
    self.context.maxProcess = process;
end

--设置进度方法
function UILoadingWindow:SetProcessFunc(process, func)
    processFuncList:Add({process, func});
end

--设置进度方法
function UILoadingWindow:SetFinishFunc(func)
    self.context.finishFunc = func;
end

--增加等待方法
function UILoadingWindow:AddWaitFunc(func)
    waitFuncList:Add(func);
end

function UILoadingWindow:CloseWindow()
    self.context.isClose = true;
end


function UILoadingWindow:OnClose()
    --MainUpdate:RemoveUpdateFunc("UILoadingWindow")
    ThinkingData.TraceEvent("gamestart_entergame", nil, false);
end

function UILoadingWindow:OnPlayClick()
    AdjustData.TraceEvent("4vypnh")
    FireBaseAnalytics.TraceEvent("PLAY")
    ThinkingData.TraceEvent("gamestart_clickplay", nil);
    self.ctrls.btnPlay:GetGameObject():SetActive(false);
    self.ctrls.goRoot:GetGameObject():SetActive(true);
    ChangeScene("GameScene");
end

function UILoadingWindow:SetDownResDir(resPaths, endFunc)
    GameWorld.Get().mDemo:SetAppUpdateResDir(resPaths);
    AppUpdaterManager.AppUpdaterStartDownloadPartialDataRes();
    mProgressData = AppUpdaterManager.AppUpdaterGetAppUpdaterProgressData();
    GameWorld.Get().mDemo:SetAppUpdaterCompleted(function()
        print("------------SetDownResDir SetAppUpdaterCompleted")
        if endFunc ~= nil then
            endFunc()
        end
        mProgressData = nil;
        self:SetProcess(100)
        self:CloseWindow();
    end)
--    mProgressData = GameFileDownloadService()
--    mProgressData:StartUpdateFiles(resPath, 
--    function(diffCount)
--    end, 
--    function()
--        self.ctrls.txtDesc:SetText("");
--        mProgressData = nil
--        if endFunc ~= nil then
--            endFunc()
--        end
--    end)
end

--更新下载中的进度
function UILoadingWindow:UpdateDownLoad()
    if totalDownloadSize ~= mProgressData.TotalDownloadSize then
        totalDownloadSizeStr = self:GetSizeStr(type, mProgressData.TotalDownloadSize);
        totalDownloadSize = mProgressData.TotalDownloadSize;
    end
    if mProgressData.CurrentDownloadSize > lastDownSize then
        --下载完成
        lastDownSize = mProgressData.CurrentDownloadSize;
        downFileSize = 0;
        downFileTime = 0;
        downSpeed = mProgressData.CurrentDownloadSize / mProgressData.CurrentDownloadFileTotalTime;
    else
        if mProgressData.CurrentDownloadSize < totalDownloadSize then
            downFileTime = downFileTime + Time.deltaTime
            downFileSize = downSpeed * downFileTime
            if downFileSize > mProgressData.CurrentDownloadingFileSize then
                downFileSize = mProgressData.CurrentDownloadingFileSize
            end
        end
    end
    local curSize = mProgressData.CurrentDownloadSize + downFileSize;
    local currentDownloadSizeStr = self:GetSizeStr(iType, curSize);
    local value = curSize / mProgressData.TotalDownloadSize;
    if value > 1 then
        value = 1;
    end
    self.context.maxProcess = math.floor(value * 100)
--    txtProcess.text = math.floor(value * 100) .. "%";
    self.ctrls.txtDesc:SetText(string.format(TextLocal.GetText("#UILoadingWindow1"), currentDownloadSizeStr, totalDownloadSizeStr));
end

function UILoadingWindow:GetSizeStr(_type, _size)
    if _type == 1 then
        return string.format("%.2fM", (_size / 1024 / 1024))
    elseif _type == 2 then
        return string.format("%.2fK", (_size / 1024))
    else
        return string.format("%dB", _size)
    end
end