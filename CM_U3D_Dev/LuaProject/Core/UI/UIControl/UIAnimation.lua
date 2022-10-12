local UIAnimation = UIControl("UIAnimation")

function UIAnimation:Play(animName)
    GameObjectUtil.PlayAnimation(self:GetGameObject(), animName)
end

function UIAnimation:Stop()
    GameObjectUtil.StopAnimation(self:GetGameObject())
end