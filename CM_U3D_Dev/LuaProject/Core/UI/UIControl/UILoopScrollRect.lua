local UILoopScrollRect = UIControl("UILoopScrollRect")
UILoopScrollRect.baseTypeName = "UILoopScrollRect"

local tempCtrl

function UILoopScrollRect:OnLoaded()
    self.__totalCount = 0
    self.__cellDataList = {}
    self.__cellCtrlList = {}
    self.__loopScrollRect = GameObjectUtil.GetLoopScrollRectComp(self:GetGameObject())
    GameObjectUtil.SetLoopScrollRectLoadCellFunc(self.__loopScrollRect, function(go, index)
        self:OnLoadCell(go, index)
    end)
end

function UILoopScrollRect:OnUnloaded()
    self:ClearData()
    GameObjectUtil.SetLoopScrollRectLoadCellFunc(self.__loopScrollRect, nil)
end

function UILoopScrollRect:ClearCells()
    self.__loopScrollRect:ClearCells()
    self:ClearData()
end

function UILoopScrollRect:RefillCells()
    self.__loopScrollRect.totalCount = self.__totalCount
    self.__loopScrollRect:RefillCells()
end

function UILoopScrollRect:RefillCellsTo(index, offer)
    self.__loopScrollRect.totalCount = self.__totalCount
    self.__loopScrollRect:RefillCells(index, false, offer)
end

function UILoopScrollRect:RefreshCells()
    self.__loopScrollRect:RefreshCells()
end

function UILoopScrollRect:AddCellData(data)
    self.__totalCount = self.__totalCount + 1
    table.insert(self.__cellDataList, data)
end

function UILoopScrollRect:GetCellCount()
    return self.__totalCount
end

function UILoopScrollRect:GetAllCellData()
    return self.__cellDataList
end

function UILoopScrollRect:GetCellData(index)
    if (index <= 0) or (index > self.__totalCount) then
        Error(string.format("UILoopScrollRect:GetCellData cannot get data of index[%s]", index))
        return nil
    end
    return self.__cellDataList[index]
end

function UILoopScrollRect:GetAllCellCtrl()
    return self.__cellCtrlList
end

function UILoopScrollRect:GetCellCtrl(index)
    if (index <= 0) or (index > self.__totalCount) then
        Error(string.format("UILoopScrollRect:GetCellCtrl cannot get cell of index[%s]的", index))
        return nil
    end
    return self.__cellCtrlList[index]
end

function UILoopScrollRect:OnLoadCell(go, index)
    --go.name = "Cell_" .. (index + 1)
    tempCtrl = self.__cellCtrlList[index + 1]
    if tempCtrl == nil then
        local cellType = (self.__loadConfig.cell or {}).type
        tempCtrl = self:NewSingleCtrl(cellType, cellType, go)
        self.__cellCtrlList[index + 1] = tempCtrl
    else
        tempCtrl.__gameObject = go
    end
    
    if tempCtrl ~= nil then
        tempCtrl:LoadCtrl(self, self.__loadConfig.cell)
        tempCtrl:SetCellData(self.__cellDataList[index + 1], index + 1)
    end
    tempCtrl = nil
end

function UILoopScrollRect:ClearData()
    for i = 1, self.__totalCount do
        tempCtrl = self.__cellCtrlList[i]
        if tempCtrl ~= nil then
            tempCtrl:UnloadCtrl()
            tempCtrl:ClearGameObject()
        end
    end
    tempCtrl = nil
    self.__totalCount = 0
    self.__cellDataList = {}
    self.__cellCtrlList = {}
end

function UILoopScrollRect:GetComp()
    return self.__loopScrollRect
end

function UILoopScrollRect:SrollToCellWithinTime(index, time)
    self.__loopScrollRect:SrollToCellWithinTime(index - 1, time or 0)
end