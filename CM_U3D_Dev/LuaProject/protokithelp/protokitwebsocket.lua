local protokitwebsocket = {}

local msgQueue = {};
local handlerQueue = {};

local messageHandler = {};
local reqeustHandler = {};
local batchRequests = {};

local isInit = false;
local WebSocketClient;

---@param MsgBatchLimit number @Queue模式最大消息合并数量 
local MsgBatchLimit = 10;

---@param EnableBatch boolean @是否使用Queue模式合并消息 
local EnableBatch = true;

function protokitwebsocket.Init()
    if not isInit then
        WebSocketClient = CS.ProtokitHelper.ProtokitWebsocketClient.Instance;
        WebSocketClient:Evt_OnWebSocketOpen('+', protokitwebsocket.OnWebSocketOpen);
        WebSocketClient:Evt_OnWebSocketClose('+', protokitwebsocket.OnWebSocketClose);
        WebSocketClient:Evt_RecvMsg('+', protokitwebsocket.RecvMessage);
        WebSocketClient:Evt_RecvPackFinish('+', protokitwebsocket.FinishRecv);
        WebSocketClient:Evt_Update('+', protokitwebsocket.Update);
        WebSocketClient:Init(EnableBatch, MsgBatchLimit, CS.ProtokitHelper.WebSocketType.AllPlatformSync);
        isInit = true;
    end
end

function protokitwebsocket.Connect(address)
    WebSocketClient:Connect(address);
end

function protokitwebsocket.SendMsg(msg, handler)
    if msg.getMessageName then
        local index = #msgQueue + 1;
        msgQueue[index] = msg;
        if handler ~= nil then
            handlerQueue[index] = handler;
        end
    end
end

function protokitwebsocket.Close()
    WebSocketClient:CloseConnect();
end

function protokitwebsocket.Update()
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
                    NetworkTrace:debug(string.format("[WebSocket][Send] %s", v.uri));
                end
                WebSocketClient:SendInternal(bytes);
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
                    NetworkTrace:debug(string.format("[WebSocket][Send] %s", v.uri));
                end
                WebSocketClient:SendInternal(bytes);
            end
            msgQueue = {};
            handlerQueue = {};
        end
    end
end

function protokitwebsocket.OnWebSocketOpen()
    print("WebSocket On Open")
end

function protokitwebsocket.OnWebSocketClose()
    print("WebSocket On Close")
end

function protokitwebsocket.RegisterMessageHandler(uri, handler)
    if handler == nil then
        return
    end
    if messageHandler[uri] == nil then
        messageHandler[uri] = {};
    end
    table.insert(messageHandler[uri], handler);
end

function protokitwebsocket.RemoveMessageHandler(uri, handler)
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

function protokitwebsocket.RecvMessage(sequenceId, passThrough, uri, bytes)
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
            protokitwebsocket.RecvCommonError(sequenceId, passThrough);
        end
    end
end

function protokitwebsocket.RecvCommonError(sequenceId, passThrough)
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

function protokitwebsocket.FinishRecv(sequenceId)
    if batchRequests[sequenceId] ~= nil then
        for k, v in pairs(batchRequests[sequenceId]) do
            if reqeustHandler[v.passThrough] ~= nil then
                reqeustHandler[v.passThrough] = nil;
            end
        end
        batchRequests[sequenceId] = nil;
    end
end

return protokitwebsocket;