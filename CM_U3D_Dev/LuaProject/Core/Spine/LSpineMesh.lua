--JSON工具
LSpineMesh = {
    camera = nil,
    planeMesh = nil,
    renderTexture = nil,
    --sortGroup = nil,
    renders = nil,

    sortingLayerName = "map_item",
    sortingOrder = 0,
}

--构造体
function LSpineMesh:new(o)
    o = o or {};
    setmetatable(o, self);
	self.__index = self;
    return o;
end

function LSpineMesh:SetActive(_isAc)
    if self.planeMesh ~= nil then
        self.planeMesh.gameObject:SetActive(_isAc)
    end
    if self.camera ~= nil then
        self.camera.gameObject:SetActive(_isAc)
    end
end

local render
function LSpineMesh:SetLayer(sortLayerName, order)
    self.sortingLayerName = sortLayerName
    self.sortingOrder = order
    if self.renders ~= nil then
        for i = 0, self.renders.Length - 1 do
            render = self.renders[i]
            render.sortingLayerName = sortLayerName;
            render.sortingOrder = order;
        end
    end
end

function LSpineMesh:SetSortLayerName(sortLayerName)
    self.sortingLayerName = sortLayerName
    if self.renders ~= nil then
        for i = 0, self.renders.Length - 1 do
            render = self.renders[i]
            render.sortingLayerName = sortLayerName;
        end
    end
end

function LSpineMesh:SetSortingOrder(order)
    self.sortingOrder = order
    if self.renders ~= nil then
        for i = 0, self.renders.Length - 1 do
            render = self.renders[i]
            render.sortingOrder = order;
        end
    end
end

function LSpineMesh:DestroyMesh()
    if self.renderTexture ~= nil then
        GameObject.Destroy(self.renderTexture);
        self.renderTexture = nil;
    end
    if self.camera ~= nil then
        self.camera.targetTexture = nil
        GameObject.Destroy(self.camera.gameObject);
        self.camera = nil;
    end
    if self.planeMesh ~= nil then
        GameObject.Destroy(self.planeMesh.gameObject);
        self.planeMesh = nil;
    end
end

