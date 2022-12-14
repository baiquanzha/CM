using System;
using MTool.AppUpdaterLib.Runtime.Interfaces;
using MTool.LoggerModule.Runtime;
using System.IO;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Download;
using MTool.AppUpdaterLib.Runtime.MTDownload;
using MTool.AppUpdaterLib.Runtime.Manifests;
using MTool.ServiceLocation.Runtime;
using CommonServiceLocator;
using UnityEngine;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
using Object = UnityEngine.Object;

namespace MTool.AppUpdaterLib.Runtime.Managers
{
    public static class AppUpdaterManager
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static ILogger s_mLogger = LoggerManager.GetLogger("AppUpdaterManager");

        private static bool s_mInitialized = false;

        private static AppUpdaterService s_mService;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public static string ClientUniqueId { set; get; } = string.Empty;

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public static void Init()
        {
            if (!s_mInitialized)
            {
                CreateService();
                s_mInitialized = true;
            }
        }

        private static void CreateService()
        {
            var container = new ServiceContainer();
            ServiceLocator.SetLocatorProvider(() => container);
            var go = new GameObject("RemoteFileDownloadService");
            Object.DontDestroyOnLoad(go);
            var service = go.AddComponent<RemoteFileDownloadService>();
            container.Register<IRemoteFileDownloadService>(null, service);

            const string serviceName = "AppUpdater";
            var servicePfb = Resources.Load<GameObject>(serviceName);
            if (servicePfb == null)
            {
                throw new FileNotFoundException("AppUpdater");
            }
            var serviceGo = Object.Instantiate(servicePfb);
            serviceGo.name = serviceName;
            s_mService = serviceGo.GetComponent<AppUpdaterService>();
        }

        public static void AppUpdaterSetDownloadService<T>() where T : Component
        {
            var go = GameObject.Find("RemoteFileDownloadService");
            if (go)
            {
                var service = go.AddComponent<T>();
                ServiceContainer locator = (ServiceContainer)ServiceLocator.Current;
                locator.Unregister<IRemoteFileDownloadService>();
                locator.Register<IMTRemoteFileDownloadService>(null, service);
                s_mService.Context.DownloadMode = AppUpdateDownloadMode.MultiThread;
            }
        }

        /// <summary>
        /// ??????App???????????????????????????
        /// </summary>
        /// <param name="requester"></param>
        public static void AppUpdaterSetAppUpdaterRequester(IAppUpdaterRequester requester)
        {
            CheckIsInitialize("AppUpdaterSetAppUpdaterRequester");
            s_mService.SetAppUpdaterRequester(requester);
        }

        /// <summary>
        /// ??????????????????????????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetErrorCallback(AppUpdaterErrorCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetErrorCallback");
            s_mService.SetErrorCallback(callback);
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetServerMaintenanceCallback(AppUpdaterServerMaintenanceCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetServerMaintenanceCallback");
            s_mService.SetServerMaintenanceCallback(callback);
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetForceUpdateCallback(AppUpdaterForceUpdateCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetForceUpdateCallback");
            s_mService.SetForceUpdateCallback(callback);
        }

        /// <summary>
        /// ??????????????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetOnTargetVersionObtainCallback(AppUpdaterOnTargetVersionObtainCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetOnTargetVersionObtainCallback");
            s_mService.SetOnTargetVersionObtainCallback(callback);
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetPerformCompletedCallback(AppUpdaterPerformCompletedCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetPerformCompletedCallback");
            s_mService.SetPerformCompletedCallback(callback);
        }

        [Obsolete("This method is deprecated. AppUpdaterBindEnableDownloadJudger instead.", false)]
        public static void AppUpdaterSetStartDownloadCallback(AppUpdaterStartDownloadCallback callback)
        {
            
        }

        public static void AppUpdaterSetGetLighthouseContentCallback(AppUpdaterGetLighthouseContentCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetGetLighthouseContentCallback");
            s_mService.SetGetLighthouseContentCallback(callback);
        }

        /// <summary>
        /// ????????????????????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetStartDownloadMissingResCallback(AppUpdaterStartDownloadMissingResCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetStartDownloadMissingResCallback");
            s_mService.SetStartDownloadMissingResCallback(callback);
        }

        /// <summary>
        /// ????????????????????????????????????
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetDownloadMissingResCompleteCallback(AppUpdaterDownloadMissingResCompleteCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetDownloadMissingResCompleteCallback");
            s_mService.SetDownloadMissingResCompleteCallback(callback);
        }

        /// <summary>
        /// ?????????????????????????????????
        /// </summary>
        /// <param name="provider"></param>
        public static void AppUpdaterSetStorageInfoProvider(IStorageInfoProvider provider)
        {
            CheckIsInitialize("AppUpdaterSetStorageInfoProvider");
            s_mService.SetStorageInfoProvider(provider);
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <param name="filter">?????????????????????</param>
        public static void AppUpdaterBindFileUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            CheckIsInitialize("AppUpdaterBindFileUpdateRuleFilter");
            s_mService.BindFileUpdateRuleFilter(filter);
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        public static void AppUpdaterUnBindFileUpdateRuleFilter()
        {
            CheckIsInitialize("AppUpdaterUnBindFileUpdateRuleFilter");
            s_mService.UnBindFileUpdateRuleFilter();
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        /// <param name="judger"></param>
        public static void AppUpdaterBindEnableDownloadJudger(AppUpdaterEnableDownloadJudge judger)
        {
            CheckIsInitialize("AppUpdaterBindEnableDownloadJudger");
            s_mService.BindEnableDownloadJudger(judger);
        }

        /// <summary>
        /// ????????????????????????
        /// </summary>
        public static void AppUpdaterUnBindEnableDownloadJudger()
        {
            CheckIsInitialize("AppUpdaterUnBindEnableDownloadJudger");
            s_mService.UnBindEnableDownloadJudger();
        }

        public static void AppUpdaterBindLocalModeUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            CheckIsInitialize("AppUpdaterBindLocalModeUpdateRuleFilter");
            s_mService.BindLocalModeUpdateRuleFilter(filter);
        }

        public static void AppUpdaterUnBindLocalModeUpdateRuleFilter()
        {
            CheckIsInitialize("AppUpdaterUnBindLocalModeUpdateRuleFilter");
            s_mService.UnBindLocalModeUpdateRuleFilter();
        }

        /// <summary>
        /// ???????????????????????????
        /// </summary>
        /// <returns></returns>
        public static AppUpdaterProgressData AppUpdaterGetAppUpdaterProgressData()
        {
            CheckIsInitialize("AppUpdaterGetAppUpdaterProgressData");
            return s_mService.Context.ProgressData;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        public static void AppUpdaterStartUpdateAgain()
        {
            CheckIsInitialize("AppUpdaterStartUpdateAgain");
            s_mService.StartUpdateAgain();
        }

        /// <summary>
        /// ??????????????????????????????????????????????????????????????????
        /// ??????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????????app??????????????????????????????????????????????????????
        /// </summary>
        /// <param name="name">
        /// ???????????????????????????
        /// ?????????????????????????????????????????????????????????????????????"assets/effects",??????"assets"????????????????????????name???"effects"?????????????????????
        /// ?????????????????????????????????????????????????????????????????????effects???????????????
        /// </param>
        public static void AppUpdaterSetRetainedDataFolderName(string name)
        {
            CheckIsInitialize("AppUpdaterSetRetainedDataFolderName");
            s_mService.SetRetainedDataFolderName(name);
        }

        /// <summary>
        /// ?????????????????????
        /// </summary>
        public static void AppUpdaterStartDownloadPartialDataRes()
        {
            CheckIsInitialize("AppUpdaterStartDownloadPartialDataRes");
            s_mService.StartDownloadPartialDataRes();
        }

        /// <summary>
        /// ????????????
        /// </summary>
        public static void OpenAppStore()
        {
            var currentLhConfig = AppVersionManager.LHConfig;
            if (!string.IsNullOrEmpty(currentLhConfig.UpdateData.AppStoreUrl))
            {
                Application.OpenURL(currentLhConfig.UpdateData.AppStoreUrl);
            }
            else
            {
                if (!string.IsNullOrEmpty(currentLhConfig.UpdateData.PackageUrl))
                {
                    Application.OpenURL(currentLhConfig.UpdateData.PackageUrl);
                }
                else
                {
                    s_mLogger.Error($"Current lighthouse config is invalid! Lighthouse id : {currentLhConfig.MetaData.lighthouseId}");
                }
            }
        }

        /// <summary>
        /// ?????????????????????????????????
        /// </summary>
        /// <returns></returns>
        public static LighthouseConfig.Server AppUpdaterGetServerData()
        {
            CheckIsInitialize("AppUpdaterGetServerData");
            if(AppVersionManager.LHConfig == null)
                throw new InvalidOperationException("Get server config data failure !");
            return AppVersionManager.LHConfig.GetCurrentServerData();
        }

        /// <summary>
        /// ????????????App????????????
        /// </summary>
        /// <returns></returns>
        public static AppInfoManifest AppUpdaterGetAppInfoManifest()
        {
            CheckIsInitialize("AppUpdaterGetAppInfoManifest");
            return AppVersionManager.AppInfo;
        }


        /// <summary>
        /// ??????LighthouseConfig??????
        /// </summary>
        /// <returns></returns>
        public static LighthouseConfig AppUpdaterGetLHConfig()
        {
            CheckIsInitialize("AppUpdaterGetLHConfig");
            return AppVersionManager.LHConfig;
        }

        /// <summary>
        /// ???????????????
        /// </summary>
        /// <returns>????????????</returns>
        public static string AppUpdaterGetChannel()
        {
            CheckIsInitialize("AppUpdaterGetChannel");
            return AppUpdaterConfigManager.AppUpdaterConfig.channel;
        }

        /// <summary>
        /// ??????????????????
        /// </summary>
        /// <returns></returns>
        public static bool AppUpdaterIsSucceed()
        {
            if (s_mService == null)
                return false;
            return s_mService.IsSucceed();
        }

        /// <summary>
        /// ???????????????Url
        /// </summary>
        /// <returns></returns>
        public static string AppUpdaterGetServerUrl()
        {
            CheckIsInitialize("AppUpdaterGetServerUrl");
            return AppVersionManager.ServerUrl;
        }

        /// <summary>
        /// ???????????????????????????????????????????????????????????????
        /// </summary>
        /// <param name="hintName">????????????</param>
        /// <param name="hintVal">???</param>
        public static void AppUpdaterHint(AppUpdaterHintName hintName , int hitVal)
        {
            AppUpdaterHints.Instance.SetHintValue(hintName,hitVal);
        }

        public static void ManualStartAppUpdate()
        {
            CheckIsInitialize("ManualStartAppUpdate");
            s_mService.ManualStartAppUpdate();
        }

        private static void CheckIsInitialize(string methodName)
        {
            if (!s_mInitialized)
                throw new NullReferenceException($"Your want to use \"AppUpdaterManager\" that not initialized ! Call method :  \"{methodName}\" .");
        }


        public static void AppUpdaterDisposeAppUpdaterService()
        {
            if (s_mService != null)
            {
                Object.Destroy(s_mService.gameObject);
            }

            s_mInitialized = false;
        }

        #endregion

    }
}
