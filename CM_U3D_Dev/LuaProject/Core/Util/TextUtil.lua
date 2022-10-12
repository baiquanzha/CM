--文本工具
TextUtil = { }

--字符串转化为整形数组
function TextUtil.StrToIntArr(str, splitStr)
    if str == nil or str == "" then
        return nil;
    end
    if splitStr == nil then
        splitStr = ',';
    end

    local paramStrs = string.split(str, splitStr);
    local value = {}
    for i = 1, #paramStrs do
        value[i] = tonumber(paramStrs[i]);
    end
    return value;
end

--字符串转化为2维整形数组
function TextUtil.StrToIntArr2(str, splitStr1, splitStr2)
    if str == nil or str == "" then
        return nil;
    end
    if splitStr1 == nil then
        splitStr1 = '|';
    end
    if splitStr2 == nil then
        splitStr2 = ',';
    end

    local paramStrs = string.split(str, splitStr1);
    local value = {};
    for i = 1, #paramStrs do
        value[i] = TextUtil.StrToIntArr(paramStrs[i], splitStr2);
    end
    return value;
end

function TextUtil.StrToFloatArr(str, splitStr)
    if str == nil or str == "" then
        return nil;
    end
    if splitStr == nil then
        splitStr = ',';
    end
    local paramStrs = string.split(str, splitStr);
    local value = {};
    for i = 1, #paramStrs do
        value[i] = tonumber(paramStrs[i]);
    end
    return value;
end

function TextUtil.StrToFloatArr2(str, splitStr1, splitStr2)
    if str == nil or str == "" then
        return nil;
    end
    if splitStr1 == nil then
        splitStr1 = '|';
    end
    if splitStr2 == nil then
        splitStr2 = ',';
    end
    local paramStrs = string.split(str, splitStr1);
    local value = {};
    for i = 1, #paramStrs do
        value[i] = TextUtil.StrToFloatArr(paramStrs[i], splitStr2);
    end
    return value;
end

--将(秒)转换成时分秒 格式：00:00:00
function TextUtil.GetTimeStr(time, isHour)
    if isHour == nil then
        isHour = false;
    end
    if time > 0 then
        local s = math.modf(time % 60); --剩余秒数
        local temp = math.modf(time / 60)
        local m = temp % 60;    --剩余分钟
        temp = math.modf(time / 60 / 60)
        local h = temp % 24;    --剩余小时
        if h > 0 or isHour then
            return string.format("%.2d:%.2d:%.2d", h, m, s);
        else
            return string.format("%.2d:%.2d", m, s);
        end
    else
        if isHour then
            return "00:00:00";
        else
            return "00:00";
        end
    end
end

--将(秒)转换成时分秒 格式：00:00:00
function TextUtil.GetMinuteStr(time)
    if time > 0 then
        local s = math.modf(time % 60); --剩余秒数
        local m = math.modf(time / 60)
        return string.format("%.2d:%.2d", m, s);
    else
        return "00:00";
    end
end

--获取时间的天时分秒
function TextUtil.TimeToDHMSTb(time)
    local day = math.floor(time / 86400)
    local hour = math.floor((time % 86400) / 3600)
    local min = math.floor((time % 3600) / 60)
    local sec = time % 60
    return {day = day, hour = hour , min = min, sec = sec}
end

--时分秒转时间
function TextUtil.HMSToTime(hour, min, sec)
    return TextUtil.DHMSToTime(0, hour, min, sec)
end

--天时分秒转时间
function TextUtil.DHMSToTime(day, hour, min, sec)
    return day * 86400 + hour * 3600 + min * 60 + sec
end

function TextUtil.ToBase64String(bytes)
    return CS.System.Convert.ToBase64String(bytes);
end

function TextUtil.FromBase64String(string)
    return CS.System.Convert.FromBase64String(string)
end

return TextUtil