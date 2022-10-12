local UIImage = UIControl("UIImage")
UIImage.baseTypeName = "UIImage"
local imageColor = Color.white

function UIImage:OnLoaded()
    self.__imageComp = GameObjectUtil.GetImageComp(self:GetGameObject())
end

function UIImage:OnUnloaded()
    self.__imageComp = nil
    self.__tweenTimer = nil
    self.__fillAmount = nil
end

function UIImage:SetImage(imageName, isReset)
    self.__imageComp.sprite = UIAtlasMgr:GetUISprite(imageName)
    if isReset ~= nil and isReset then
        self.__imageComp:SetNativeSize()
    end
end

function UIImage:SetImageFromMapItem(imageName)
    self.__imageComp.sprite = UIAtlasMgr:GetMapItemSprite(imageName)
end

function UIImage:SetTexture(resPath, isReset)
    self.__imageComp.sprite = UIAtlasMgr:GetTexture(resPath)
    if isReset ~= nil and isReset then
        self.__imageComp:SetNativeSize()
    end
end

function UIImage:SetFillAmount(value, counterType)
    if value > 1 then
        value = 1
    end
    if counterType ~= nil then
        if self.__fillAmount == nil then
            self.__imageComp.fillAmount = value
        else
            if value == self.__fillAmount then
                return
            end

            local tweenTime, delayTime = 0.25, nil
            if counterType == EnumMoneyCounterType.NormalFly then
                delayTime = 1.25
            -- elseif counterType == EnumMoneyCounterType.TaskReward then
            --     tweenTime = 0.7
            --     delayTime = 0.8
            end

            if delayTime ~= nil then
                local lastNum = self.__fillAmount
                if self.__tweenTimer == nil then
                    self.__tweenTimer = self:CreateTimer()
                end
                self.__tweenTimer:SetSingleTimer(delayTime, function()
                    self.__imageComp:DOFillAmount(value, tweenTime)
                end)
            else
                self.__imageComp:DOFillAmount(value, tweenTime)
            end
        end
        self.__fillAmount = value
    else
        self.__imageComp.fillAmount = value
    end
end

function UIImage:SetNativeSize()
    self.__imageComp:SetNativeSize()
end

function UIImage:SetColor(colorTb)
    imageColor.r = colorTb[1]
    imageColor.g = colorTb[2]
    imageColor.b = colorTb[3]
    imageColor.a = colorTb[4] or 1
    self.__imageComp.color = imageColor
end

function UIImage:GetAlpha()
    return self.__imageComp.color.a
end

function UIImage:FitterScale(overlayScale)
    overlayScale = overlayScale or 1
    local imgRectTrans = self:GetRectTransform()
    --X和Y的缩放
    local scaleX = 150 / imgRectTrans.sizeDelta.x
    local scaleY = 150 / imgRectTrans.sizeDelta.y
    local scale
    --取最小的缩放
    if scaleX < scaleY then
        scale = scaleX
    else
        scale = scaleY
    end
    if scale > 1 then
        --最大缩放限制为1
        scale = 1
    else
        --自适应缩放再小一点
        scale = scale - 0.05
    end
    --读取表格中的叠加缩放
    scale = scale * overlayScale
    self:SetLocalScale(scale)
end

function UIImage:GetImageComp()
    return self.__imageComp
end

function UIImage:GetSprite()
    return self.__imageComp.sprite
end

function UIImage:SetSprite(sprite)
    self.__imageComp.sprite = sprite
end

function UIImage:DOFade(endValue, duration)
    return self.__imageComp:DOFade(endValue, duration)
end

function UIImage:SetMaterial(mat)
    self.__imageComp.material = mat
end