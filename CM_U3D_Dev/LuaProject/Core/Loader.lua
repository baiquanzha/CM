--加载文件
_G.DoFile = function(filePath)
    require(filePath)
end

--递归加载文件夹下的文件
local function LoadLuaFolder(folderData, rootName)
    local folderName = rootName
    if folderData.name ~= nil then
        folderName = folderName .. folderData.name .. "/"
    end

    for _, data in ipairs(folderData) do
        if type(data) == "string" then
            DoFile(folderName .. data)
        else
            LoadLuaFolder(data, folderName)
        end
    end
end

--加载所有文件
_G.LoadLuaProject = function()
    LoadLuaFolder(GetProject(), "")
end