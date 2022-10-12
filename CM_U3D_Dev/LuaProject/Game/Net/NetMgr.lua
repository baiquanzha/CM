NetMgr = {
    waitEvents = IList:new(),
    pingTime = 0             --ping时间
}
local PingCoolTime = 2 * 60;    --2分钟上传一次

local token = "GIFOVPOfQQsgeNh3098IviYmWx6XpX3zxAebDCqsE1IJpYwMeSpZC9qktAWjycNH";
local url = "http://localhost:8079/";

local privateKey;   --私钥
local publicKey;    --公钥

local function GetNetUtilName(key)
    return netutils.MetadataKeyString(key)
end

function NetMgr:Init()
    local alice = CS.MTool.Core.Security.Cryptography.X25519KeyAgreement.GenerateKeyPair();
    privateKey = alice.PrivateKey;
    publicKey = TextUtil.ToBase64String(alice.PublicKey);

    protokithttp.Init()

    self.pingTime = TimerMgr.curTime;

    -- if Booter.isEditor and GameWorld.Get().isReleaseNet == false then
    --     --内网
    --     url = "http://mergeworld-master-new-game-online.auto.centurygame.io:80/api/v1";
    -- else
    --     --使用lighthouse配置的地址
    --     local serverUrl = AppUpdaterManager.AppUpdaterGetServerUrl()
    --     local index, _ = string.find(serverUrl, "lighthouse");
    --     print("------------NetMgr index = "..tostring(index));
    --     if index ~= nil then
    --         url = string.gsub(serverUrl,"lighthouse","api/v1", 1)
    --     else
    --         url = string.format("%s/%s", serverUrl, "api/v1")
    --     end

    --     netutils.defaultMetadata:set(GetNetUtilName(netutils.MetadataKey_AppVersion),AppUpdaterManager.AppUpdaterGetAppInfoManifest().version)
    --     --netutils.defaultMetadata:set(GetNetUtilName(netutils.MetadataKey_Language),AppUpdaterManager.AppUpdaterGetLanguage())
    --     netutils.defaultMetadata:set(GetNetUtilName(netutils.MetadataKey_Channel),AppUpdaterManager.AppUpdaterGetChannel())
    -- end
    ILog.Debug("-----net url = "..url);

    self.waitEvents:Clear();
end

function NetMgr:Update()
    if self.waitEvents.Count > 0 then
        NetMgr:SendStatisticReq(self.waitEvents);
        self.waitEvents:Clear();
    end

    if TimerMgr.curTime >= self.pingTime + PingCoolTime then
        NetMgr:SendPingReq()
    end
end

function NetMgr:GetErrorCode(uri, bytes)
    if uri == netutils.ErrorResponse:getMessageName() then
        local e = netutils.ErrorResponse:newFromBytes(bytes)
        print("error code: "..e.code)
        print("error message = "..e.message);
        if e.code == 1003 then
            --session过期
            CGAccount.Instance:Logout();
        end
        return e.code
--        local language = TableLoader:GetItem("Language", tostring(e.code))
--        local  errMsg = e.code
--        if language then
--            errMsg = language.Value
--        end
--        return true, errMsg, e.code
    end
    return 0
end

function NetMgr:SendReq(req, callBack)
    protokithttp.PostMsg(url, req, function(uri, bytes)
        local code = NetMgr:GetErrorCode(uri, bytes)
        if code == 0 then
            if callBack then
                callBack(0, bytes)
            end
        else
            if callBack then
                callBack(code, nil)
            end
		end
	end);
end

function NetMgr:SendAuthorizeReq(sessionKey, callBack)
    print("------------SendAuthorizeReq sessionKey = "..sessionKey);
    local req = msg.AuthorizeReq:new();
    req.AccessToken = sessionKey;
	req.PublicKey = publicKey;
    req.Version = ClientVersion;
    req.ClientTime = TimeUtil.GetServerTime();

    if PlatformUtil.GetPlatformName() == "Android" then
        req.Platform = "google";
    elseif PlatformUtil.GetPlatformName() == "iOS" then
        req.Platform = "ios";
    else
        req.Platform = "";
    end

    --获取时区
    local dateTime = CS.System.DateTime.Now;
    local dateTimeUtc = CS.System.DateTime.UtcNow;
    local span = dateTime - dateTimeUtc;
    req.timezone = tostring(span.Hours);

	protokithttp.PostMsg(url, req, function(uri, bytes)
        local code = NetMgr:GetErrorCode(uri, bytes)
        if code == 0 then
            local rsp = msg.Authorize:newFromBytes(bytes);
            ILog.Debug("Post Success, response msg:"..rsp:getMessageName());
		end
	end);
end

function NetMgr:SendPingReq()
    local pingReq = msg.PingReq:new();
    pingReq.Uid = UserInfo.id;
    pingReq.Duration = 0

    protokithttp.PostMsg(url, pingReq, function(uri, bytes)
        local code = NetMgr:GetErrorCode(uri, bytes)
        if code == 0 then
            
        end
    end);

    self.pingTime = TimerMgr.curTime;
end

return NetMgr;

