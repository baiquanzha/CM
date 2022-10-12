ILog = {
    logger
}
local self = ILog;

function ILog.Debug(message)
    if self.logger ~= nil then
        self.logger:Debug(message);
    end
end

function ILog.DebugFormat(format, ...)
    if self.logger ~= nil then
        self.logger:DebugFormat(format, ...);
    end
end

function ILog.Error(message)
    if self.logger ~= nil then
        self.logger:Error(message);
    end
end

function ILog.ErrorEx(message, exception)
    if self.logger ~= nil then
        self.logger:Error(message, exception);
    end
end

function ILog.ErrorFormat(format, ...)
    if self.logger ~= nil then
        self.logger:ErrorFormat(format, ...);
    end
end

function ILog.Fatal(message)
    if self.logger ~= nil then
        self.logger:Fatal(message);
    end
end

function ILog.FatalEx(message, exception)
    if self.logger ~= nil then
        self.logger:Fatal(message, exception);
    end
end

function ILog.FatalFormat(format, ...)
    if self.logger ~= nil then
        self.logger:FatalFormat(format, ...);
    end
end

function ILog.Info(message)
    if self.logger ~= nil then
        self.logger:Info(message);
    end
end

function ILog.InfoFormat(format, ...)
    if self.logger ~= nil then
        self.logger:InfoFormat(format, ...);
    end
end

function ILog.Warn(message)
    if self.logger ~= nil then
        self.logger:Warn(message);
    end
end

function ILog.WarnFormat(format, ...)
    if self.logger ~= nil then
        self.logger:WarnFormat(format, ...);
    end
end


