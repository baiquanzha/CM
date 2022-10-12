protokitclient = {}

local msgQueue = {};
local handlerQueue = {};

local messageHandler = {};
local reqeustHandler = {};
local batchRequests = {};

local isInit = false;
---@param MsgBatchLimit number @Queue模式最大消息合并数量 
local MsgBatchLimit = 10;

local Network;
---@param EnableBatch boolean @是否使用Queue模式合并消息 
local EnableBatch = false;

function protokitclient.Init()
    if not isInit then
        CS.ProtokitHelper.ProtokitClient.Instance:Evt_RecvMsg('+', protokitclient.RecvMessage);
        CS.ProtokitHelper.ProtokitClient.Instance:Evt_RecvPackFinish('+', protokitclient.FinishRecv);
        CS.ProtokitHelper.ProtokitClient.Instance:Evt_Update('+', protokitclient.Update);
        CS.ProtokitHelper.ProtokitClient.Instance:Init(EnableBatch, MsgBatchLimit);
        Network = GameLauncher.Network;
        isInit = true;
    end
end

function protokitclient.SendMsg(msg, handler)
    if msg.getMessageName then
        local index = #msgQueue + 1;
        msgQueue[index] = msg;
        if handler ~= nil then
            handlerQueue[index] = handler;
        end
    end
end

function protokitclient.Update()
    if EnableBatch then
        if #msgQueue > 0 then
            local sendIndex = 1;
            while sendIndex <= #msgQueue do
                local sendMsgs = {};
                local handlers = {};
                while #sendMsgs < MsgBatchLimit and sendIndex <= #msgQueue do
                    local index = #sendMsgs + 1;
                    table.insert(sendMsgs, index, msgQueue[sendIndex]);
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
                    NetworkTrace:debug(string.format("[TCP][Send] %s", v.uri));
                end
                Network:SendMessage(bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    else
        if #msgQueue > 0 then
            for i = 1, #msgQueue do
                local msg = msgQueue[i];
                local packet, bytes = netutils:marshalRawPacket(msg);
                batchRequests[packet.SequenceID] = {};
                for k, v in ipairs(packet.rawAny) do
                    if handlerQueue[i] ~= nil then
                        reqeustHandler[v.passThrough] = handlerQueue[i];
                        local tbl = {};
                        tbl.passThrough = v.passThrough;
                        tbl.uri = v.uri;
                        table.insert(batchRequests[packet.SequenceID], tbl);
                    end
                    NetworkTrace:debug(string.format("[TCP][Send] %s", v.uri));
                end
                Network:SendMessage(bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    end
end

---@type fun(uri:string, handler:function) @注册对某一类型消息的监听函数
---@param uri string @消息名称
---@param handler function @消息处理回调
function protokitclient.RegisterMessageHandler(uri, handler)
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
function protokitclient.RemoveMessageHandler(uri, handler)
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

function protokitclient.RecvMessage(sequenceId, passThrough, uri, bytes)
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
            protokitclient.RecvCommonError(sequenceId, passThrough);
        end
    end
end

function protokitclient.RecvCommonError(sequenceId, passThrough)
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

function protokitclient.FinishRecv(sequenceId)
    if batchRequests[sequenceId] ~= nil then
        for k, v in pairs(batchRequests[sequenceId]) do
            if reqeustHandler[v.passThrough] ~= nil then
                reqeustHandler[v.passThrough] = nil;
            end
        end
        batchRequests[sequenceId] = nil;
    end
end

return protokitclient;

