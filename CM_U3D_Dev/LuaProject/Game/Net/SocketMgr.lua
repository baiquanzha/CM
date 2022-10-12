SocketMgr = {

}

local network

function SocketMgr.Init()
    protokitclient.Init()
    network = GameLauncher.Network;
    network:onConnectSuccess('+', SocketMgr.onConnectSuccess);
	network:onConnectFailed('+', SocketMgr.onConnectFailed);
	network:onDisconnect('+', SocketMgr.onDisconnect);
end

function SocketMgr.ConnectToServer()
	--LuaLogger.LogDebug("login. user:"..this.userName..", password:"..this.pwd);
	network:StartTcpConnect("192.168.0.103", 8990);
end

function SocketMgr.onExit()
	network:onConnectSuccess('-', SocketMgr.onConnectSuccess);
	network:onConnectFailed('-', SocketMgr.onConnectFailed);
	network:onDisconnect('-', SocketMgr.onDisconnect);
end

function SocketMgr.Update()
    
end

function SocketMgr.onConnectSuccess()
	ILog.Debug("connect success");
	local authorize = msg.Authorize:new();
	authorize.PlayerID = 123411;
	protokitclient.SendMsg(authorize, SocketMgr.onLogin);
end

function SocketMgr.onConnectFailed()
	ILog.Debug("connect failed");
end

function SocketMgr.onDisconnect(reason)
	ILog.Debug("onDisconnect, reason:"..reason);
end

function SocketMgr.onLogin(uri, bytes)
	if uri == msg.Authorize:getMessageName() then
		local authorize = msg.Authorize:newFromBytes(bytes);
		ILog.Debug("onLogin, player"..authorize.PlayerID);
	elseif uri == netutils.ErrorResponse:getMessageName() then
		local errRsp = netutils.ErrorResponse:newFromBytes(bytes);
		warn("request authorize response error. code:"..errRsp.code..", message:"..errRsp.message);
	else
		warn("request authorize response unexcepect proto. uri:"..uri);
	end
end


return SocketMgr;

