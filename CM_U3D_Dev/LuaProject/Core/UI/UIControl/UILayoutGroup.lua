local UILayoutGroup = UIControl("UILayoutGroup")
UILayoutGroup.baseTypeName = "UILayoutGroup"

function UILayoutGroup:OnLoaded()
    self.__childCount = 0
    self.__ctrlChildList = {}
    self.__transform = self:GetTransform()
    local childTrans
    for index = 0, self.__transform.childCount - 1 do
        childTrans = self.__transform:GetChild(index)
        childTrans.gameObject:SetActive(false)
    end
    self.__layoutState = 0
end

function UILayoutGroup:SetLayout(state, x, y, width, space)
    self.__layoutState = state
    self.__x = x
    self.__y = y
    self.__width = width
    self.__space = space
end

function UILayoutGroup:OnUnloaded()
    self:UnloadAllChild()
    self.__transform = nil
end

function UILayoutGroup:ClearAllChild()
    local childTrans
    for index = 0, self.__childCount - 1 do
        --遍历时要越过第一位，所以下标向后延一位
        childTrans = self.__transform:GetChild(index + 1)
        childTrans.gameObject:SetActive(false)
    end
    self:UnloadAllChild()
end

function UILayoutGroup:UnloadAllChild()
    for _, ctrl in ipairs(self.__ctrlChildList) do
        ctrl:UnloadCtrl()
        ctrl:ClearGameObject()
    end
    self.__childCount = 0
    self.__ctrlChildList = {}
end

function UILayoutGroup:AddCtrlChild()
    local childTrans
    --查找已存在物体时向后延一位，将第一个留出来做复制用，以防止第一个产生了特效等不可直接掌控的物体
    if self.__childCount + 1 < self.__transform.childCount then
        childTrans = self.__transform:GetChild(self.__childCount + 1)
    else
        childTrans = GameObjectUtil.Instantiate(self.__transform:GetChild(0).gameObject, self.__transform).transform
    end
    self.__childCount = self.__childCount + 1
    childTrans.gameObject:SetActive(true)

    if self.__layoutState == 1 then
        --水平布局
        childTrans.transform.anchoredPosition = Vector2(self.__x + (self.__width + self.__space) * (self.__childCount - 1), self.__y)
        local size = self.__transform.sizeDelta;
        self.__transform.sizeDelta = Vector2(self.__childCount * self.__width + (self.__childCount - 1) * self.__space, size.y)
    elseif self.__layoutState == 2 then
        --垂直布局
        childTrans.transform.anchoredPosition = Vector2(self.__x, self.__y - (self.__width + self.__space) * (self.__childCount - 1))
        local size = self.__transform.sizeDelta;
        self.__transform.sizeDelta = Vector2(size.x, self.__childCount * self.__width + (self.__childCount - 1) * self.__space)
    end

    local cellType = (self.__loadConfig.cell or {}).type
    local ctrl = self:NewSingleCtrl(cellType, cellType, childTrans.gameObject)
    if ctrl ~= nil then
        ctrl:LoadCtrl(self, self.__loadConfig.cell)
    end

    table.insert(self.__ctrlChildList, ctrl)
    return ctrl
end

function UILayoutGroup:Refresh()
    for _, ctrl in ipairs(self.__ctrlChildList) do
        if ctrl.Refresh ~= nil then
            ctrl:Refresh()
        end
    end
end

function UILayoutGroup:GetChildCount()
    return self.__childCount
end

function UILayoutGroup:GetCtrlChild(index)
    if index > self.__childCount then
        Error(string.format("The index [%s] is over count of all children[%s]", index, self.__childCount))
        return
    end
    return self.__ctrlChildList[index]
end

function UILayoutGroup:GetAllCtrlChild()
    return self.__ctrlChildList
end

function UILayoutGroup:ScrollToIndex(index)
    local scrollComp
    --包括自身及向上两个界面 寻找ScrollRect组件
    local layer = 0
    local parentTrans = self.__transform
    while layer < 3 do
        scrollComp = parentTrans.gameObject:GetComponent("ScrollRect")
        if scrollComp ~= nil then
            break
        end
        parentTrans = parentTrans.parent
        layer = layer + 1
    end

    --没找到直接结束
    if scrollComp == nil then
        return
    end

    local targetCtrl = self:GetCtrlChild(index)
    if targetCtrl ~= nil then
        --获取spacing和padding
        local spacingX, spacingY, paddingLeft, paddingTop
        if self.__layoutState == 1 then
            spacingX, spacingY = self.__space, 0
            paddingLeft, paddingTop = self.__x, self.__y
        else
            local layoutComp = self.__transform.gameObject:GetComponent("HorizontalOrVerticalLayoutGroup")
            if layoutComp ~= nil then
                spacingX, spacingY = layoutComp.spacing, layoutComp.spacing
                paddingLeft, paddingTop = layoutComp.padding.left, layoutComp.padding.top
            else
                layoutComp = self.__transform.gameObject:GetComponent("GridLayoutGroup")
                if layoutComp ~= nil then
                    spacingX, spacingY = layoutComp.spacing.x, layoutComp.spacing.y
                    paddingLeft, paddingTop = layoutComp.padding.left, layoutComp.padding.top
                end
            end
        end

        local size = targetCtrl:GetRectTransform().sizeDelta
        scrollComp.enabled = false
        --竖向定位
        if scrollComp.vertical then
            scrollComp:StopMovement()
            self.__transform.localPosition = Vector3(0, (size.y + spacingY) * (index - 1) + paddingTop, 0)
        end
        --横向定位
        if scrollComp.horizontal then
            scrollComp:StopMovement()
            self.__transform.localPosition = Vector3(-(size.x + spacingX) * (index - 1.5) - paddingLeft, 0, 0)
        end
        local timer
        timer = TimerMgr:AddSingleTimer(0.01, function()
            scrollComp.enabled = true
            timer:Destroy()
        end)
    end
end