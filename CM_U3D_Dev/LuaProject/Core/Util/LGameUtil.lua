--游戏工具类
LGameUtil = { }

function LGameUtil.SetImageGray(image, isGray)
    if image ~= nil then
        if isGray then
            image.material = ResMgr.uiImgGrayMat;
        else
            image.material = ResMgr.spriteDefault;
        end
    end
end

function LGameUtil.SetSpriteRenderGray(render, isGray)
    if render ~= nil then
        local mpb = MaterialPropertyBlock();
        render:GetPropertyBlock(mpb); 
        if isGray then
            mpb:SetFloat("_IsGray", 1);
        else
            mpb:SetFloat("_IsGray", 0);
        end
        render:SetPropertyBlock(mpb);
    end
end

local tempCount
local randomIndex
local index
function LGameUtil.GetRandomIndexArr(_size)
    local arr = {};
    for i = 1, _size do
        arr[i] = i
    end

    for i = 1, _size do
        randomIndex = math.random(_size);
        index = arr[randomIndex]
        arr[randomIndex] = arr[i]
        arr[i] = index
    end
    return arr
end

function LGameUtil.RandomArr(_arr)
    tempCount = #_arr
    local indexArr = LGameUtil.GetRandomIndexArr(tempCount)
    local arr = {}
    for i = 1, tempCount do
        index = indexArr[i]
        arr[i] = _arr[index]
    end
    return arr
end