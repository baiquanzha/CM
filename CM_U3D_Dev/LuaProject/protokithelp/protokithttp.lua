protokithttp = {}

local msgQueue = {};
local handlerQueue = {};

local messageHandler = {};
local reqeustHandler = {};
local batchRequests = {};
--local reqMsgList = nil;

local isInit = false;

---@param MsgBatchLimit number @Queue模式最大消息合并数量 
local MsgBatchLimit = 10;

local HttpClient;

---@param EnableBatch boolean @是否使用Queue模式合并消息 
local EnableBatch = false;

---@param AutoRetry boolean @请求失败后是否自动重试
local AutoRetry = true;

local SecretKey;
local SecretUid;


function protokithttp.Init()
    --reqMsgList = IDictionary:new()
    if not isInit then
        HttpClient = CS.ProtokitHelper.ProtokitHttpClient.Instance;
        HttpClient:Init(EnableBatch, AutoRetry, MsgBatchLimit);
        HttpClient:Evt_RecvMsg('+', protokithttp.RecvMsg);
        HttpClient:Evt_RecvPackFinish('+', protokithttp.FinishRecv);
        HttpClient:Evt_Update('+', protokithttp.Update);
        isInit = true;
        --应该由业务层在合适的地方监听下面的事件
        --请求返回错误状态
        HttpClient:Evt_RspFailed('+', protokithttp.ResponseFailed);
        --单次请求开始
        HttpClient:Evt_ReqBegin('+', protokithttp.RequestBegin);
        --单次请求结束
        HttpClient:Evt_ReqEnd('+', protokithttp.RequestEnd);
        --请求达到重试次数上限仍未成功
        HttpClient:Evt_ReqFailed('+', protokithttp.RequestFailed);
        --超出预期请求时间
        HttpClient:Evt_ReqTimeOutExpect('+', protokithttp.RequestTimeOutExpect);
    end
end

function protokithttp.SetToken(token)
    HttpClient:SetHttpToken(token)
    netutils.defaultMetadata:set("Authorization",token)
end 

function protokithttp.SetSecretKey(secretKey,uid)
    SecretKey = secretKey
    SecretUid = uid
end


function protokithttp.PostMsg(url, msg, handler)
    if msg.getMessageName then
        local index = #msgQueue + 1;
        msgQueue[index] = {};
        msgQueue[index].url = url;
        msgQueue[index].msg = msg;
        if handler ~= nil then
            handlerQueue[index] = handler;
        end
    end
end

function protokithttp.Encrypt(mm,md)
    local packet = netutils.RawPacket:new()
    -- single message
    if mm.getMessageName then
        mm = {mm}
    else
        -- message table
        if not netutils.EnableQueue then
            error(string_format("mm is message list, but queue not enabled"))
        end
    end

    local rawAnyValid = false
    for _, m in ipairs(mm) do
        if m.getMessageName then
            local any = netutils.RawAny:new()
            any.uri = m:getMessageName()
            local bytes = m:marshal()
            if m.uri ~= msg.PingReq.getMessageName() then
                any.raw = Aes.Encrypt(bytes,SecretKey);
            else
                any.raw = bytes
            end
            any.passThrough = netutils:nextPassThrough()
            if netutils.EnableQueue then
                if packet.rawAny == nil then
                    packet.rawAny = {}
                end
                table.insert(packet.rawAny,any)
                rawAnyValid = true
            else
                packet.rawAny = any
                rawAnyValid = true
                break
            end
        end
    end
    if not rawAnyValid then
        error(string_format("missing message"))
    end

    local mdLocal = netutils.defaultMetadata
    if md then
        mdLocal = netutils.metadata.join(mdLocal,md)
    end
    mdLocal:set("x_create_time", os.time())
    packet.metadata = netutils.metadata.toPairs(mdLocal)
    packet.SequenceID = netutils:nextSequenceID()
    packet.Version = netutils.VersionCode

    return packet, packet:marshal()
end

function protokithttp.Update()
    if EnableBatch then
        if #msgQueue > 0 then
            local sendIndex = 1;
            while sendIndex <= #msgQueue do
                local sendMsgs = {};
                local handlers = {};
                local httpUrl;
                while #sendMsgs < MsgBatchLimit and sendIndex <= #msgQueue do
                    local index = #sendMsgs + 1;
                    if index == 1 then
                        httpUrl = msgQueue[sendIndex].url;
                    else
                        if msgQueue[sendIndex].url ~= httpUrl then
                            break;
                        end
                    end
                    table.insert(sendMsgs, index, msgQueue[sendIndex].msg);
                    if handlerQueue[sendIndex] ~= nil then
                        handlers[index] = handlerQueue[sendIndex];
                    end
                    sendIndex = sendIndex + 1;
                end
                local packet, bytes
                if SecretKey ~= nil then
                    local encrgyMeta = netutils.metadata.new()
                    encrgyMeta:append("sandwich_encrypt",SecretUid)
                    packet, bytes = protokithttp.Encrypt(sendMsgs,encrgyMeta)
                else
                    packet, bytes = netutils:marshalRawPacket(sendMsgs);    
                end
                batchRequests[packet.SequenceID] = {};
                for k, v in ipairs(packet.rawAny) do
                    if handlers[k] ~= nil then
                        reqeustHandler[v.passThrough] = handlers[k];
                        local tbl = {};
                        tbl.passThrough = v.passThrough;
                        tbl.uri = v.uri;
                        table.insert(batchRequests[packet.SequenceID], tbl);
                    end
                    NetworkTrace:debug(string.format("[HTTP][Send] %s", v.uri));
                end
                HttpClient:PostRequestBytes(httpUrl, bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    else
        if #msgQueue > 0 then
            for i = 1, #msgQueue do
                local req = msgQueue[i];
                local packet, bytes
                if SecretKey ~= nil then
                    local encrgyMeta = netutils.metadata.new()
                    encrgyMeta:append("sandwich_encrypt",SecretUid)
                    packet, bytes = protokithttp.Encrypt(req.msg,encrgyMeta)
                else
                    packet, bytes = netutils:marshalRawPacket(req.msg);
                end
                batchRequests[packet.SequenceID] = {};
                for k, v in ipairs(packet.rawAny) do
                    if handlerQueue[i] ~= nil then
                        reqeustHandler[v.passThrough] = handlerQueue[i];
                        local tbl = {};
                        tbl.passThrough = v.passThrough;
                        tbl.uri = v.uri;
                        table.insert(batchRequests[packet.SequenceID], tbl);
                    end
                    NetworkTrace:debug(string.format("[HTTP][Send] %s", v.uri));
                end
--                if reqMsgList ~= nil then
--                    if req.msg:getMessageName() == "msg.StatisticReq" then
--                        reqMsgList:Add(packet.SequenceID, req.msg)
--                    end
--                end
                HttpClient:PostRequestBytes(req.url, bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    end
end

---@type fun(uri:string, handler:function) @注册对某一类型消息的监听函数
---@param uri string @消息名称
---@param handler function @消息处理回调
function protokithttp.RegisterMessageHandler(uri, handler)
    if handler == nil then
        return
    end
    if messageHandler[uri] == nil then
        messageHandler[uri] = {};
    end
    table.insert(messageHandler[uri], handler);
end

---@type fun(uri:string, handler:function) @移除对某一类型消息的监听函数
---@param uri string @消息名称
---@param handler function @消息处理回调
function protokithttp.RemoveMessageHandler(uri, handler)
    if handler == nil then
        return
    end
    if messageHandler[uri] ~= nil then
        local index = 0;
        for i = 1, #messageHandler[uri] do
            if messageHandler[uri][i] == handler then
                index = i;
                break;
            end
        end
        if index ~= 0 then
            table.remove(messageHandler[uri], index);
        end
    end
end

function protokithttp.RecvMsg(sequenceId, passThrough, uri, bytes)
--    if reqMsgList ~= nil then
--        reqMsgList:Remove(sequenceId)
--    end

    if SecretKey ~= nil and uri ~= "netutils.ErrorResponse" then
        if string.len(bytes) > 0 then
            bytes = Aes.Decrypt(bytes,SecretKey);
        end
    end
    
    if messageHandler[uri] ~= nil then
        for k,v in pairs(messageHandler[uri]) do
            v(bytes);
        end
    end
    if reqeustHandler[passThrough] ~= nil then
        reqeustHandler[passThrough](uri, bytes);
        reqeustHandler[passThrough] = nil;
    end
    if uri == netutils.ErrorResponse:getMessageName() then
        local errorMsg = netutils.ErrorResponse:newFromBytes(bytes);
        if not errorMsg.LogicException then
            protokithttp.RecvCommonError(sequenceId, passThrough);
        end
    end
end

function protokithttp.RecvCommonError(sequenceId, passThrough)
    if batchRequests[sequenceId] ~= nil then
        for k, v in pairs(batchRequests[sequenceId]) do
            if reqeustHandler[v.passThrough] ~= nil then
                reqeustHandler[v.passThrough] = nil;
            end
            if v.passThrough == passThrough then
                NetworkTrace:warn("Common error caused by "..v.uri);
            end
        end
        batchRequests[sequenceId] = nil;
    end
end

function protokithttp.FinishRecv(sequenceId)
    if batchRequests[sequenceId] ~= nil then
        for k, v in pairs(batchRequests[sequenceId]) do
            if reqeustHandler[v.passThrough] ~= nil then
                reqeustHandler[v.passThrough] = nil;
            end
        end
        batchRequests[sequenceId] = nil;
    end
end

function protokithttp.ResponseFailed(statusCode, statusName)
    --Http返回错误状态
    print("ResponseFailed statusCode = "..tostring(statusCode).." statusName = "..tostring(statusName));
--    OpenWindow("UIConfirmWindow", {"Error", "Http error code = "..tostring(statusCode), function()
--        protokithttp.Reset()
--    end, nil, false})
end

function protokithttp.RequestBegin()
    --单次请求开始
    --print("RequestBegin");
end

function protokithttp.RequestEnd()
    --单次请求结束
    --print("RequestEnd");
end

function protokithttp.RequestFailed()
    --请求达到重试次数上限仍未成功，可弹窗提示用户确认网络状况后手动重试(调用HttpClient:Retry)，或者停止HttpClient的所有请求(调用HttpClient:Stop)后退回登录界面
    print("RequestFailed ChangeScheme");
    --切换请求方式
    protokithttp.ChangeScheme()
    --重新请求
    HttpClient:Retry();
end

function protokithttp.RequestTimeOutExpect()
    --超出预期请求时间，可能需要显示转圈界面，当请求结束时关闭转圈界面
    print("RequestTimeOutExpect");
end

function protokithttp.ChangeScheme()
    if HttpClient.Scheme == HttpScheme.HttpClient then
        HttpClient:SwitchSchemeUnityWebRequest(GameWorld.Get())
    else
        HttpClient:SwitchSchemeHttpClient();
    end
end

function protokithttp.Reset()
    HttpClient:Stop();
    HttpClient:Init(EnableBatch, AutoRetry, MsgBatchLimit);
end

function protokithttp.Reset()
    HttpClient:Stop();
    HttpClient:Init(EnableBatch, AutoRetry, MsgBatchLimit);
end

--保存还未发送成功的消息列表
function protokithttp.SaveReqMsgList()
--    if reqMsgList == nil or reqMsgList.tableCount == 0 then
--        return;
--    end
--    GameRecord.SaveReqMsgList(reqMsgList)
end

return protokithttp;