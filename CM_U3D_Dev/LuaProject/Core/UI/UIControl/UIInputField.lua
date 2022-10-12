local UIInputField = UIControl("UIInputField")

-- ["OnValueChanged"] = {
--     func = GameObjectUtil.AddInputValueChangedFunc,
--     argCount = 1,
-- },
-- ["OnEndEdit"] = {
--     func = GameObjectUtil.AddInputEndEditFunc,
--     argCount = 1,
-- },

function UIInputField:OnLoaded()
    self.__inputField = self:GetGameObject():GetComponent("InputField")
end

function UIInputField:OnUnloaded()
    self.__inputField = nil
end

function UIInputField:SetText(text)
    self.__inputField.text = text
end

function UIInputField:GetText()
    return self.__inputField.text
end