using System;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Protocols;

namespace MTool.AppUpdaterLib.Runtime.Interfaces
{
    public interface IAppUpdaterRequester
    {
        void Update();
        void ReqGetVersion(LighthouseConfig.Server serverData,
            string appVersion,
            string lighthouseId,
            string channel,
            FileServerType fromTo, Action<GetVersionResponseInfo> getVersionResponseInfoAction);

    }
}
