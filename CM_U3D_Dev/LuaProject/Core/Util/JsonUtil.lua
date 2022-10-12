--JSON工具
JsonUtil = { }

local rapidjson = require('rapidjson')

function JsonUtil.JsonToStr(_json)
    return rapidjson.encode(_json);
end

function JsonUtil.StrToJson(str)
    return rapidjson.decode(str)
end

function JsonUtil.IsNull(value)
    return value == rapidjson.null
end

return JsonUtil