--ui适配工具
LGameAdapt = {
    gameWidth = 1280,
    gameHeight = 720,
    pixelScale = 1,
    fullScale = 1,
    standardRate = 1,   --标准高宽比
    screenRate = 1,     --实际高宽比
    matchWidthOrHeight = 1, --适配方式，0-宽度适配，1-高度适配

    adjustLeft = 0,
    adjustRight = 0,

    gameWidthHalf = 0,
    gameHeightHalf = 0,
}

local DESIGN_SCREEN_WIDTH = 1920;
local DESIGN_SCREEN_HEIGHT = 1080;

--ui适配初始化
function LGameAdapt.Init()
    LGameAdapt.adjustLeft = GameWorld.Get().adjustLeft;
    LGameAdapt.adjustRight = GameWorld.Get().adjustRight;
    print("-----LGameAdapt.adjustLeft = "..LGameAdapt.adjustLeft.."  LGameAdapt.adjustRight = "..LGameAdapt.adjustRight);

    LGameAdapt.standardRate = DESIGN_SCREEN_WIDTH / DESIGN_SCREEN_HEIGHT;
    LGameAdapt.screenRate = Screen.width / Screen.height;

    local canvasScaler = UICanvas.Get().gameObject:GetComponent("CanvasScaler");
    if LGameAdapt.screenRate >= LGameAdapt.standardRate then
        canvasScaler.matchWidthOrHeight = 1
        LGameAdapt.matchWidthOrHeight = 1

        LGameAdapt.pixelScale = Screen.height / DESIGN_SCREEN_HEIGHT;
        LGameAdapt.fullScale = Screen.width / (DESIGN_SCREEN_WIDTH * LGameAdapt.pixelScale)
        LGameAdapt.gameWidth = DESIGN_SCREEN_HEIGHT / Screen.height * Screen.width;
        LGameAdapt.gameHeight = DESIGN_SCREEN_HEIGHT;
    elseif LGameAdapt.screenRate < LGameAdapt.standardRate then
        canvasScaler.matchWidthOrHeight = 0
        LGameAdapt.matchWidthOrHeight = 0

        LGameAdapt.pixelScale = Screen.width / DESIGN_SCREEN_WIDTH;
        LGameAdapt.fullScale = Screen.height / (DESIGN_SCREEN_HEIGHT * LGameAdapt.pixelScale)
        LGameAdapt.gameWidth = DESIGN_SCREEN_WIDTH;
        LGameAdapt.gameHeight = DESIGN_SCREEN_WIDTH / Screen.width * Screen.height;
    end

    LGameAdapt.gameWidthHalf = LGameAdapt.gameWidth / 2;
    LGameAdapt.gameHeightHalf = LGameAdapt.gameHeight / 2;
end