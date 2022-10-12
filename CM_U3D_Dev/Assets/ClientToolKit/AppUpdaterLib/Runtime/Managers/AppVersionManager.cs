using System.IO;
using System;
using System.Text;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Manifests;
using UnityEngine;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime.Managers
{
    internal class AppVersionManager
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        private static ILogger s_mLogger = LoggerManager.GetLogger("AppVersionManager");

        private static Encoding m_Encoding = new UTF8Encoding(false,true);

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        /// <summary>
        /// App本地信息
        /// </summary>
        public static AppInfoManifest AppInfo { private set; get; }

        /// <summary>
        /// 当前获取的最新的Lighthouse配置
        /// </summary>
        public static LighthouseConfig LHConfig { private set; get; }

        /// <summary>
        /// 当前游戏服url（此url是由热更模块校验版本信息时确定的）
        /// </summary>
        public static string ServerUrl { private set; get; }

        /// <summary>
        /// 目标版本号
        /// </summary>
        public static AppInfoManifest TargetAppInfo { private set; get; }

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------


        public static void MakeCurrentAppInfo(AppInfoManifest appInfo)
        {
            AppInfo = appInfo;

            SaveCurrentAppInfo();
        }

        public static void SaveCurrentAppInfo()
        {
            string jsonContents = JsonUtility.ToJson(AppInfo,true);
            string externalAppInfoPath = AssetsFileSystem.GetWritePath(AssetsFileSystem.AppInfoFileName, true);

            s_mLogger.Debug($" SaveCurrentAppInfo [\"version\" : \"{AppInfo.version}\" , " +
                            $"\"dataResVersion\" : \"{AppInfo.dataResVersion} \" , \"unityDataResVersion\" : \"{AppInfo.unityDataResVersion}\"]");

            File.WriteAllText(externalAppInfoPath,jsonContents, m_Encoding);
        }

        public static AppInfoManifest LoadLocalAppInfo()
        {
            string externalAppInfoPath = AssetsFileSystem.GetWritePath(AssetsFileSystem.AppInfoFileName, true);

            if (File.Exists(externalAppInfoPath))
            {
                var contents = File.ReadAllText(externalAppInfoPath , m_Encoding);
                
                return ParseAppInfo(contents);
            }

            return null;
        }

        public static AppInfoManifest ParseAppInfo(string contents)
        {
            AppInfoManifest result = null;
            try
            {
                result = JsonUtility.FromJson<AppInfoManifest>(contents);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return result;
        }

        public static AppInfoManifest ParseAppInfo(byte[] bytes)
        {
            return ParseAppInfo(m_Encoding.GetString(bytes));
        }

        public static void MakeCurrentLighthouseConfig(LighthouseConfig config)
        {
            s_mLogger.Info("Set current lighthouse config!");
            LHConfig = config;
        }

        public static void MakeCurrentServerUrl(string url)
        {
            s_mLogger.Debug($"Make current server url : {url} .");
            ServerUrl = url;
        }

        public static void SetTargetVersion(AppInfoManifest targetAppInfo)
        {
            TargetAppInfo = targetAppInfo;
        }

        public static VersionManifest LoadLocalResManifest(string manifestName)
        {
            string path = AssetsFileSystem.GetWritePath(manifestName, true);

            if (File.Exists(path))
            {
                var contents = File.ReadAllText(path, m_Encoding);

                return ResManifestParser.VersionManifestParser.Parse(contents);
            }
            return null;
        }


        public static string LoadLocalResManifestContents(string manifestName)
        {
            string path = AssetsFileSystem.GetWritePath(manifestName, true);

            if (File.Exists(path))
            {
                return File.ReadAllText(path, m_Encoding);
            }
            return null;
        }


        public static bool IsLocalResManifestExist(string manifestName)
        {
            string path = AssetsFileSystem.GetWritePath(manifestName, true);
            return File.Exists(path);
        }


        public static void SaveToLocalDataResManifest(string json,string fileName)
        {
            s_mLogger.Info($"Save manifest file that name is {fileName} !");

            string externalAppInfoPath = AssetsFileSystem.GetWritePath(fileName, true);

            File.WriteAllText(externalAppInfoPath, json, m_Encoding);
        }

        public static void SaveToLocalDataResManifest(byte[] bytes, string fileName)
        {
            string externalAppInfoPath = AssetsFileSystem.GetWritePath(fileName, true);

            File.WriteAllBytes(externalAppInfoPath, bytes);
        }


        //public static bool LoadDeferredDownloadManifestToLocal()
        //{
        //    var path = AssetsFileSystem.GetWritePath(AssetsFileSystem.DeferredDownloadManifestName, true);
        //    if (File.Exists(path))
        //    {
        //        var contents = File.ReadAllText(path, m_Encoding);

        //    }
        //}

        public static void SaveDeferredDownloadManifestToLocal()
        {
            
        }


#if DEBUG_APP_UPDATER
        public static string GetAppVersionDesc()
        {
            if (AppInfo != null)
            {
                return $"App Version : \"{AppInfo.version}\" \r\n" +
                       $"Data Version : \"{AppInfo.dataResVersion}\" \r\n" +
                       $"Unity Data Version : \"{AppInfo.unityDataResVersion} \r\n" +
                       $"Platform : \"{AppInfo.TargetPlatform}\"";
            }
            return string.Empty;
        }
#endif

#endregion

    }
}
