using System;
using System.Diagnostics;
using System.IO;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.AppUpdaterLib.Runtime.Utilities;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime
{
    internal partial class AppUpdaterContext
    {
        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() => LoggerManager.GetLogger("AppUpdaterContext"));

        [Conditional("DEBUG_APP_UPDATER")]
        public void AppendInfo(string info)
        {
            var Now = DateTime.Now.ToString("HH:mm:ss ");
            this.InfoStringBuilder.AppendLine($"{Now}{info}");
        }

        public string GetStreamingAssetsPath(string path)
        {
            mTempSb.Clear();

#if UNITY_ANDROID && !UNITY_EDITOR
            mTempSb.Append(Application.streamingAssetsPath);
            mTempSb.Append("/");
#elif UNITY_IPHONE && !UNITY_EDITOR
            mTempSb.Append("file://");
            mTempSb.Append(Application.streamingAssetsPath);
            mTempSb.Append("/");
#elif UNITY_EDITOR
            mTempSb.Append("file://");
            mTempSb.Append(Application.streamingAssetsPath);
            mTempSb.Append("/");
#endif

#if APPEND_PLATFORM_NAME
            mTempSb.Append(Utility.GetPlatformName());
            mTempSb.Append("/");
#endif

            mTempSb.Append(path);
            return mTempSb.ToString();
        }

        public string GetRemoteRootUrl(FileServerType fileServerType)
        {
            var appUpdaterConfig = AppUpdaterConfigManager.AppUpdaterConfig;
            var serverUrl = (fileServerType == FileServerType.CDN) ? appUpdaterConfig.cdnUrl : appUpdaterConfig.ossUrl;
            return string.IsNullOrWhiteSpace(appUpdaterConfig.remoteRoot) ? serverUrl : $"{serverUrl}/{appUpdaterConfig.remoteRoot}";
        }

        /// <summary>
        /// 获取Lighthouse 配置的url
        /// </summary>
        /// <param name="lightHouseId">Lighthouse配置的id</param>
        /// <param name="fileServerType">文件服务器的类型</param>
        /// <returns></returns>
        public string GetLighthouseUrl(string lightHouseId, FileServerType fileServerType = FileServerType.CDN)
        {
            var curTime = TimeUtility.GetCurrentTimeSeconds() / 60;
            var appUpdaterConfig = AppUpdaterConfigManager.AppUpdaterConfig;
            //var language = GetCurrentLanguageName();

            string url;

            if (!string.IsNullOrEmpty(lightHouseId))
                url = $"{GetRemoteRootUrl(fileServerType)}/{appUpdaterConfig.channel}-lighthouse.json_{lightHouseId}?v={curTime}&t={curTime}";
            else
                url = $"{GetRemoteRootUrl(fileServerType)}/{appUpdaterConfig.channel}-lighthouse.json?v={curTime}&t={curTime}";

            return url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrentVerisonFileUrl(FileServerType fileServerType = FileServerType.CDN)
        {
            string url = $"{GetRemoteRootUrl(fileServerType)}/version_list/{GetCurrentRemoteVersionFileName()}";

            return url;
        }

        private VersionDesc GetCurrentVersionDesc()
        {
            if (this.ResUpdateTarget.CurrentResVersionIdx >= this.ResUpdateTarget.VersionDescs.Length || this.ResUpdateTarget.CurrentResVersionIdx < 0)
            {
                throw new IndexOutOfRangeException($"CurrentResVersionIdx ：{this.ResUpdateTarget.CurrentResVersionIdx} .");
            }

            var curVersionDesc = this.ResUpdateTarget.VersionDescs[this.ResUpdateTarget.CurrentResVersionIdx];
            return curVersionDesc;
        }

        public string GetCurrentLocalMd5()
        {
            var versionDesc = GetCurrentVersionDesc();

            return versionDesc.LocalMd5;
        }

        public string GetCurrentRemoteMd5()
        {
            var versionDesc = GetCurrentVersionDesc();

            return versionDesc.RemoteMd5;
        }

        public UpdateResourceType GetCurrentUpdateType()
        {
            var versionDesc = GetCurrentVersionDesc();

            return versionDesc.Type;
        }

        private static string GetResManifestName(string version, UpdateResourceType type = UpdateResourceType.TableData, bool remote = true)
        {
            if (type == UpdateResourceType.TableData)
            {
                return remote ? $"res_data.json{CommonConst.WellNumUtf8}{version}"
                    : "res_data.json";
            }
            else
            {
                return remote ? $"res_{Utility.GetPlatformName().ToLower()}.json{CommonConst.WellNumUtf8}{version}"
                    : $"res_{Utility.GetPlatformName().ToLower()}.json";
            }
        }

        private string GetCurrentRemoteVersionFileName()
        {
            var curVersionInfo = GetCurrentVersionDesc();

            return GetResManifestName(curVersionInfo.RemoteMd5 , curVersionInfo.Type);
        }

        public string GetCurrentLocalVersionFileName()
        {
            var curVersionInfo = GetCurrentVersionDesc();

            return GetResManifestName(curVersionInfo.LocalMd5, curVersionInfo.Type, false);
        }


        public bool IsNeedUpdate()
        {
            if (this.ResUpdateTarget.VersionDescs == null)
            {
                return false;
            }
            return this.ResUpdateTarget.VersionDescs.Length == 0;
        }

        public bool IsUpdateCompleted()
        {
            return this.ResUpdateTarget.CurrentResVersionIdx >= this.ResUpdateTarget.VersionDescs.Length;
        }

        public string GetCurrentLanguageName()
        {
            var language = PlayerPrefs.GetString(CommonConst.APP_LANGUAGE_KEY, "en");

            return language;
        }

        /// <summary>
        /// 获取设备存储信息
        /// </summary>
        public void FetchDeviceStorageInfo()
        {
            if (this.StorageInfoProvider != null)
            {
                this.DiskInfo.AvailableSpace = this.StorageInfoProvider.GetAvailableDiskSpace();
                this.DiskInfo.BusySpace = this.StorageInfoProvider.GetBusyDiskSpace();
                this.DiskInfo.TotalSpace = this.StorageInfoProvider.GetTotalDiskSpace();
                this.DiskInfo.time = DateTime.Now;
                this.DiskInfo.IsGetReady = true;
            }
        }

        /// <summary>
        /// 保存当前Revision信息
        /// </summary>
        public void SaveAppRevision()
        {
            if (!string.IsNullOrEmpty(this.ResUpdateTarget.TargetResVersionNum))
            {
                Version version = new Version(AppVersionManager.AppInfo.version);
                version.Patch = this.ResUpdateTarget.TargetResVersionNum;
                AppVersionManager.AppInfo.version = version.GetVersionString();
            }
            AppVersionManager.SaveCurrentAppInfo();
        }

        public void CleanExpireResFile()
        {
            for (int i = 0; i < this.ExpireResFileList.Count; i++)
            {
                string path = AssetsFileSystem.GetWritePath(this.ExpireResFileList[i]);
                if (File.Exists(path))
                {
                    File.Delete(path);
                    s_mLogger.Value.Info($"Delete Expired Res File:{path}");
                }
            }
        }

        public void LoadUpdateResMap(out UpdateResMap updateResMap)
        {
            string path = AssetsFileSystem.GetWritePath(AssetsFileSystem.UpdateResMap);
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                try
                {
                    updateResMap = UpdateResMapParser.Parse(content);
                }
                catch (Exception e)
                {
                    updateResMap = UpdateResMap.RegenUpdateResMap(BuiltInResManifest, LocalResManifest, false);
                    s_mLogger.Value.Info($"Load UpdateResMap throw exception:{e.Message}, stack:{e.StackTrace}");
                }
            }
            else
            {
                updateResMap = new UpdateResMap();
            }
        }

        public static string GetUpdateFileRelativeLocalPath(FileDesc desc)
        {
            var config = AppUpdaterConfigManager.AppUpdaterConfig;
            string localDataRoot = config.localDataRoot;
            string remoteRoot = config.remoteRoot;
            string unityResPrefix = "resource/";
            if (!string.IsNullOrWhiteSpace(remoteRoot))
                unityResPrefix = $"{remoteRoot}/{"resource/"}";
            string relativePath;
            string fileName = desc.GetLocalFileName();

            if (desc.RN.StartsWith(unityResPrefix))
            {
                relativePath = fileName;
            }
            else
            {
                if (AppUpdaterHints.Instance.LowerLuaName)
                {
                    if (!string.IsNullOrEmpty(localDataRoot))
                        relativePath = $"{localDataRoot}/{fileName.ToLower()}";
                    else
                        relativePath = fileName.ToLower();
                }
                else
                {
                    if (!string.IsNullOrEmpty(localDataRoot))
                        relativePath = $"{localDataRoot}/{fileName}";
                    else
                        relativePath = fileName;
                }
            }
            return relativePath;
        }

        public static string GetUpdateFileLocalPath(FileDesc desc)
        {
            string relativePath = GetUpdateFileRelativeLocalPath(desc);

            return AssetsFileSystem.GetWritePath(relativePath);
        }
    }

}
