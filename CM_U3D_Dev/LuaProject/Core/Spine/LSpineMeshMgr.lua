require "Core/Spine/LSpineMesh"

--JSON工具
LSpineMeshMgr = {
    spineMeshList = IList:new(),
    num = 0,
}

local self = LSpineMeshMgr;

function LSpineMeshMgr.InitSpineMesh()
    local spineMesh = nil;
    local initCount = 5 - self.spineMeshList.Count
    if initCount > 0 then
        for i = 1, initCount do
            spineMesh = LSpineMeshMgr.CreateSpineMesh()
            LSpineMeshMgr.CacheSpineMesh(spineMesh);
        end
    end
end

function LSpineMeshMgr.CreateRawImage(effObj)
    local spineMesh = nil;
    if self.spineMeshList.Count > 0 then
        spineMesh = self.spineMeshList:Get(1);
        self.spineMeshList:RemoveAt(1);
        spineMesh.camera.gameObject:SetActive(true);
    else
        spineMesh = LSpineMeshMgr.CreateSpineMesh()
    end
    
    spineMesh.planeMesh.gameObject:SetActive(false);
    local renderTexture = CS.UnityEngine.RenderTexture(512, 512, 0)
    spineMesh.renderTexture = renderTexture
    spineMesh.camera.targetTexture = renderTexture
    effObj.layer = 12;
    local trans = effObj.transform;
    for i = 1, trans.childCount do
        trans:GetChild(i - 1).gameObject.layer = 12;
    end
    
    trans:SetParent(spineMesh.camera.transform);
    trans.localScale = Vector3(-1, 1, 1);
    trans.localRotation = Quaternion.identity;
    trans.localPosition = Vector3.zero;
    return spineMesh;
end

function LSpineMeshMgr.CreateSpinePlane(spineObj, isModel)
    local spineMesh = nil;
    if self.spineMeshList.Count > 0 then
        spineMesh = self.spineMeshList:Get(1);
        self.spineMeshList:RemoveAt(1);
        spineMesh.camera.gameObject:SetActive(true);
        spineMesh.planeMesh.gameObject:SetActive(true);
    else
        spineMesh = LSpineMeshMgr.CreateSpineMesh()
    end

    local renderTexture
    if isModel then
        renderTexture = CS.UnityEngine.RenderTexture(1024, 1024, 16)
    else
        renderTexture = CS.UnityEngine.RenderTexture(512, 512, 0)
    end
    spineMesh.renderTexture = renderTexture
    spineMesh.camera.targetTexture = renderTexture
    spineMesh.planeMesh.material:SetTexture("_MainTex", renderTexture)
    spineObj.layer = 12;
    for i = 1, spineObj.transform.childCount do
        spineObj.transform:GetChild(i - 1).gameObject.layer = 12;
    end
    
    local trans = spineObj.transform;
    trans:SetParent(spineMesh.camera.transform);
    trans.localScale = Vector3(-1, 1, 1);
    trans.localRotation = Quaternion.identity;
    trans.localPosition = Vector3.zero;
    return spineMesh;
end

function LSpineMeshMgr.CreateSpineMesh()
    -- local renderTexture = CS.UnityEngine.RenderTexture(1024, 1024, 0);
    local prefab = ResMgr.LoadPrefabs("World/spine_camera");
    local cameraObj = GameObjectUtil.Instantiate(prefab, nil);
    local cameraTrans = cameraObj.transform;
    cameraTrans.position = Vector3(self.num * 30 + 30, 0, 0);
    local camera = cameraObj:GetComponent("Camera");
    -- camera.targetTexture = renderTexture;

    prefab = ResMgr.LoadPrefabs("World/spine_plane");
    local planeObj = GameObjectUtil.Instantiate(prefab, nil);
    local planeMesh = planeObj:GetComponent("MeshRenderer");
--    local mpb = MaterialPropertyBlock();
--    mpb:SetTexture("_MainTex", renderTexture);
--    planeMesh:SetPropertyBlock(mpb);
    -- planeMesh.material:SetTexture("_MainTex", renderTexture);

    local spineMesh = LSpineMesh:new();
    spineMesh.camera = camera;
    spineMesh.planeMesh = planeMesh;
    --spineMesh.sortGroup = planeObj:GetComponent("SortingGroup");
    spineMesh.renders = GameObjectUtil.GetComponentsType(planeObj, typeof(CS.UnityEngine.Renderer));

    self.num = self.num + 1;

    return spineMesh;
end

function LSpineMeshMgr.CacheSpineMesh(spineMesh)
    if self.spineMeshList.Count >= 5 then
        --最多缓存5个
        spineMesh:DestroyMesh()
    else
        if spineMesh.renderTexture ~= nil then
            GameObject.Destroy(spineMesh.renderTexture)
            spineMesh.renderTexture = nil
        end
        
        spineMesh.camera.targetTexture = nil
        spineMesh.camera.gameObject:SetActive(false);

        if spineMesh.planeMesh ~= nil then
            spineMesh.planeMesh.transform:SetParent(nil);
            spineMesh.planeMesh.gameObject:SetActive(false);
            local mpb = MaterialPropertyBlock();
            spineMesh.planeMesh:GetPropertyBlock(mpb); 
            mpb:SetColor("_Color", Color.white);
            mpb:SetFloat("_IsGray", 0)
            spineMesh.planeMesh:SetPropertyBlock(mpb);
        end

        spineMesh.parent = nil;
        self.spineMeshList:Add(spineMesh);
    end
end

function LSpineMeshMgr.Clear()
    local spineMesh;
    for i = 1, self.spineMeshList.Count do
        spineMesh = self.spineMeshList:Get(i);
        spineMesh:DestroyMesh()
    end
    self.spineMeshList:Clear();
end

return LSpineMeshMgr