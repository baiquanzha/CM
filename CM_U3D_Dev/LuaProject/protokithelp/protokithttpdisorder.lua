local protokithttpdisorder = {}

local msgQueue = {};
local handlerQueue = {};

local messageHandler = {};
local reqeustHandler = {};
local batchRequests = {};

local HttpClientDisorder;

local isInit = false;

---@param MsgBatchLimit number @Queue模式最大消息合并数量 
local MsgBatchLimit = 3;

---@param EnableBatch boolean @是否使用Queue模式合并消息 
local EnableBatch = true;

function protokithttpdisorder.Init()
    if not isInit then
        HttpClientDisorder = CS.ProtokitHelper.ProtoKitHttpClientDisorder.Instance;
        HttpClientDisorder:Init(EnableBatch, MsgBatchLimit);
        HttpClientDisorder:Evt_RecvMsg('+', protokithttpdisorder.RecvMsg);
        HttpClientDisorder:Evt_RecvPackFinish('+', protokithttpdisorder.FinishRecv);
        HttpClientDisorder:Evt_Update('+', protokithttpdisorder.Update);
        isInit = true;
    end
end

function protokithttpdisorder.PostMsg(url, msg, handler)
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

function protokithttpdisorder.Update()
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
                local packet, bytes = netutils:marshalRawPacket(sendMsgs);
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
                HttpClientDisorder:PostRequestBytes(httpUrl, packet.SequenceID, bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    else
        if #msgQueue > 0 then
            for i = 1, #msgQueue do
                local req = msgQueue[i];
                local packet, bytes = netutils:marshalRawPacket(req.msg);
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
                HttpClientDisorder:PostRequestBytes(req.url, packet.SequenceID, bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    end
end

---@type fun(uri:string, handler:function) @注册对某一类型消息的监听函数
---@param uri string @消息名称
---@param handler function @消息处理回调
function protokithttpdisorder.RegisterMessageHandler(uri, handler)
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
function protokithttpdisorder.RemoveMessageHandler(uri, handler)
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

function protokithttpdisorder.RecvMsg(sequenceId, passThrough, uri, bytes)
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
            protokithttpdisorder.RecvCommonError(sequenceId, passThrough);
        end
    end
end

function protokithttpdisorder.RecvCommonError(sequenceId, passThrough)
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

function protokithttpdisorder.FinishRecv(sequenceId)
    if batchRequests[sequenceId] ~= nil then
        for k, v in pairs(batchRequests[sequenceId]) do
            if reqeustHandler[v.passThrough] ~= nil then
                reqeustHandler[v.passThrough] = nil;
            end
        end
        batchRequests[sequenceId] = nil;
    end
end

return protokithttpdisorder;