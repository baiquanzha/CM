using System;
using System.Collections.Generic;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.LoggerModule.Runtime;

namespace MTool.AppUpdaterLib.Runtime.Configs
{
    public sealed class LighthouseConfig
    {

        #region Inner  class

        public interface IFillByJsonObject
        {
            void Parse(JSONObject jsonObject);
        }

        public class MetaAutoGen : IFillByJsonObject
        {
            public string lighthouseId { get; private set; }

            public void Parse(JSONObject jsonObject)
            {
                this.lighthouseId = jsonObject["id"].str;
            }
        }

        public class UpdateDataInfo : IFillByJsonObject
        {
            /// <summary>
            /// 更新版本号
            /// </summary>
            public string Versoin { get; private set; }

            /// <summary>
            /// 安装包的下载地址(如果没有商店的话，从这里下载)
            /// </summary>
            public string PackageUrl { get; private set; }

            /// <summary>
            /// 商店地址，对应各个渠道的商店
            /// </summary>
            public string AppStoreUrl { get; private set; }

            /// <summary>
            /// 版本更新描述
            /// </summary>
            public string DescUrl { get; private set; }

            /// <summary>
            /// 可选，标题描述
            /// </summary>
            public string TitleDesc { get; private set; }

            /// <summary>
            /// 必填，建议更新一天显示几次面板，如果>0，当前版本号小于新版本版本号，则提示建议更新
            /// </summary>
            public long DisplayPerDay { get; private set; }

            public void Parse(JSONObject jsonObject)
            {
                this.Versoin = jsonObject["version"]?.str;
                this.PackageUrl = jsonObject["url_package"]?.str;
                this.AppStoreUrl = jsonObject["url_store"]?.str;
                this.DescUrl = jsonObject["url_desc"]?.str;
                this.TitleDesc = jsonObject["title_desc"]?.str;
                if (jsonObject["display_per_day"])
                    this.DisplayPerDay = jsonObject["display_per_day"].i;
            }
        }

        /// <summary>
        /// 与维护相关的信息
        /// </summary>
        public class MaintenanceInfo : IFillByJsonObject
        {
            /// <summary>
            /// 是否开启维护
            /// </summary>
            public bool IsOpen { private set; get; }

            /// <summary>
            /// 维护公告地址
            /// </summary>
            public string UrlPattern { private set; get; }

            /// <summary>
            /// 是否需要强更
            /// </summary>
            public bool ForceUpdate { get; private set; }

            public void Parse(JSONObject jsonObject)
            {
                var maintenance = jsonObject["maintenance"];
                if (maintenance)
                    IsOpen = maintenance.b;

                var urlPattern = jsonObject["maintenance_url_pattern"];
                UrlPattern = urlPattern?.str;

                var forceUpdate = jsonObject["force_update"];
                if (forceUpdate)
                    ForceUpdate = forceUpdate.b;
            }
        }

        /// <summary>
        /// 服务器信息
        /// </summary>
        public class Server : IFillByJsonObject
        {
            public string Name { set; get; }

            public string Url { set; get; }

            public string VersionMin { set; get; }

            public string VersionMax { set; get; }

            public List<string> FallbackUrlList { set; get; } = new List<string>();

            public MaintenanceInfo MaintenanceInfo { set; get; } = new MaintenanceInfo();

            public void Parse(JSONObject jsonObject)
            {
                this.Name = jsonObject["branch"]?.str;
                this.Url = jsonObject["url"]?.str;
                this.VersionMin = jsonObject["version_min"].str;

                var versionMaxObj = jsonObject["version_max"];
                if (versionMaxObj)
                    this.VersionMax = versionMaxObj.str;

                var fallbackListObj = jsonObject["fallback"];
                if (fallbackListObj)
                {
                    for (int i = 0; i < fallbackListObj.Count; i++)
                    {
                        var serveUrl = fallbackListObj[i].str;
                        this.FallbackUrlList.Add(serveUrl);
                    }
                }

                this.MaintenanceInfo.Parse(jsonObject);
            }


            public bool CanBeUseForVersion(string versionStr)
            {
                var version = new Runtime.Version(versionStr);
                Runtime.Version minVersion = new Runtime.Version(this.VersionMin);
                var compareResult = minVersion.CompareTo(version);

                bool result = compareResult <= Runtime.Version.VersionCompareResult.Equal;

                if (result && !string.IsNullOrEmpty(this.VersionMax))
                {
                    var maxVersion = new Runtime.Version(this.VersionMax);
                    compareResult = maxVersion.CompareTo(version);
                    result = compareResult > Version.VersionCompareResult.Equal;//前闭后开，
                }
                return result;
            }
        }

        public class ServersInfo : IFillByJsonObject
        {
            public List<Server> Servers { get; private set; } = new List<Server>();
            public void Parse(JSONObject jsonObject)
            {
                if (jsonObject.type != JSONObject.Type.ARRAY)
                {
                    throw new ArgumentException("jsonObject");
                }

                int count = jsonObject.Count;
                for (int i = 0; i < count; i++)
                {
                    var serverObj = jsonObject[i];
                    Server server = new Server();
                    server.Parse(serverObj);
                    this.Servers.Add(server);
                }
            }
        }

        #endregion

        //--------------------------------------------------------------

        #region Fields

        //--------------------------------------------------------------

        private static ILogger mLogger = LoggerManager.GetLogger("LighthouseConfig");
        
        public MetaAutoGen MetaData { get; private set; } = new MetaAutoGen();

        public UpdateDataInfo UpdateData { get; private set; } = new UpdateDataInfo();

        public ServersInfo ServersData { get; private set; } = new ServersInfo();

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        public static LighthouseConfig ReadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentNullException(json);

            LighthouseConfig config = Parse(json);

            return config;
        }

        private static LighthouseConfig Parse(string json)
        {
            LighthouseConfig config = null;

            var doc = new JSONObject(json);

            //parser _meta_auto_gen_
            var metaData = doc["_meta_auto_gen_"];
            if (!metaData) {throw new ArgumentException("json");}

            var updateInfo = doc["update_info"];
            if (!updateInfo) { throw new ArgumentException("json"); }

            var servers = doc["servers"];
            if (!servers) { throw new ArgumentException("json"); }

            config = new LighthouseConfig();
            
            config.MetaData.Parse(metaData);
            config.UpdateData.Parse(updateInfo);
            config.ServersData.Parse(servers);
            return config;
        }

        public MaintenanceInfo GetMaintenanceInfo()
        {
            var serves = this.ServersData.Servers;

            var curVersion = AppVersionManager.AppInfo.version;
            for (int i = 0; i < serves.Count; i++)
            {
                var serverData = serves[i];

                if (serverData.CanBeUseForVersion(curVersion))
                {
                    return serverData.MaintenanceInfo;
                }
            }

            return null;
        }

        public Server GetCurrentServerData()
        {
            var serves = this.ServersData.Servers;

            var curVersion = AppVersionManager.AppInfo.version;
            for (int i = 0; i < serves.Count; i++)
            {
                var serverData = serves[i];

                if (serverData.CanBeUseForVersion(curVersion))
                {
                    return serverData;
                }
            }

            return null;
        }

    #endregion

    }
}
