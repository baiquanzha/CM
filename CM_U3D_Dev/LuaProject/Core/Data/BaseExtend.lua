string.split = function(str, ...)
    local subStrTb = {}
    local splitStrArray = {...}
    while true do
        local pos
        local subLen = 0
        for _, splitStr in ipairs(splitStrArray) do
            pos = string.find(str, splitStr)
            if pos then
                subLen = #splitStr
                break
            end
        end
        if not pos then
            subStrTb[#subStrTb + 1] = str;
            break;
        end
        local subStr = string.sub(str, 1, pos - 1)
        subStrTb[#subStrTb + 1] = subStr
        str = string.sub(str, pos + subLen, #str)
    end

    return subStrTb
end

table.copy = function(tb)
    local targetTb = {}
    for k, v in pairs(tb) do
        if type(v) == "table" then
            targetTb[k] = table.copy(v)
        else
            targetTb[k] = v
        end
    end
    return targetTb
end

table.append = function(tb1, tb2, isNew)
    local targetTb
    local appendTb = table.copy(tb2)
    if isNew then
        targetTb = table.copy(tb1)
    else
        targetTb = tb1
    end

    for _, v in ipairs(appendTb) do
        table.insert(targetTb, v)
    end

    return targetTb
end

table.merge = function(tb1, tb2, isNew)
    local targetTb
    local appendTb = table.copy(tb2)
    if isNew then
        targetTb = table.copy(tb1)
    else
        targetTb = tb1
    end

    for k, v in pairs(appendTb) do
        targetTb[k] = v
    end

    return targetTb
end

table.mergeReward = function(tb1, tb2, isNew)
    local targetTb
    local appendTb = table.copy(tb2)
    if isNew then
        targetTb = table.copy(tb1)
    else
        targetTb = tb1
    end

    for _, appendReward in ipairs(appendTb) do
        if appendReward[1] ~= nil then
            local isMerge = false
            for _, targetReward in ipairs(targetTb) do
                if targetReward[1] == appendReward[1] then
                    isMerge = true
                    targetReward[2] = targetReward[2] + appendReward[2]
                    break
                end
            end
    
            if not isMerge then
                table.insert(targetTb, appendReward)
            end
        end
    end

    return targetTb
end

table.existsValue = function(tb, value)
    for _, v in pairs(tb) do
        if v == value then
            return true
        end
    end
    return false
end

table.existsKey = function(tb, key)
    for k, _ in pairs(tb) do
        if k == key then
            return true
        end
    end
    return false
end