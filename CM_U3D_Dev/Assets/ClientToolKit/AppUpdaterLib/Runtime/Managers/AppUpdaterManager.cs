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
        /// 注入App更新依赖的请求对象
        /// </summary>
        /// <param name="requester"></param>
        public static void AppUpdaterSetAppUpdaterRequester(IAppUpdaterRequester requester)
        {
            CheckIsInitialize("AppUpdaterSetAppUpdaterRequester");
            s_mService.SetAppUpdaterRequester(requester);
        }

        /// <summary>
        /// 设置更新模块发生错误时的回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetErrorCallback(AppUpdaterErrorCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetErrorCallback");
            s_mService.SetErrorCallback(callback);
        }

        /// <summary>
        /// 注册维护中回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetServerMaintenanceCallback(AppUpdaterServerMaintenanceCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetServerMaintenanceCallback");
            s_mService.SetServerMaintenanceCallback(callback);
        }

        /// <summary>
        /// 注册强更回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetForceUpdateCallback(AppUpdaterForceUpdateCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetForceUpdateCallback");
            s_mService.SetForceUpdateCallback(callback);
        }

        /// <summary>
        /// 注册目标版本信息回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetOnTargetVersionObtainCallback(AppUpdaterOnTargetVersionObtainCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetOnTargetVersionObtainCallback");
            s_mService.SetOnTargetVersionObtainCallback(callback);
        }

        /// <summary>
        /// 注册更新完成时回调
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
        /// 注册开始下载缺失资源回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetStartDownloadMissingResCallback(AppUpdaterStartDownloadMissingResCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetStartDownloadMissingResCallback");
            s_mService.SetStartDownloadMissingResCallback(callback);
        }

        /// <summary>
        /// 注册下载缺失资源完成回调
        /// </summary>
        /// <param name="callback"></param>
        public static void AppUpdaterSetDownloadMissingResCompleteCallback(AppUpdaterDownloadMissingResCompleteCallback callback)
        {
            CheckIsInitialize("AppUpdaterSetDownloadMissingResCompleteCallback");
            s_mService.SetDownloadMissingResCompleteCallback(callback);
        }

        /// <summary>
        /// 设置磁盘信息获取提供者
        /// </summary>
        /// <param name="provider"></param>
        public static void AppUpdaterSetStorageInfoProvider(IStorageInfoProvider provider)
        {
            CheckIsInitialize("AppUpdaterSetStorageInfoProvider");
            s_mService.SetStorageInfoProvider(provider);
        }

        /// <summary>
        /// 绑定文件更新过滤器
        /// </summary>
        /// <param name="filter">文件更新过滤器</param>
        public static void AppUpdaterBindFileUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            CheckIsInitialize("AppUpdaterBindFileUpdateRuleFilter");
            s_mService.BindFileUpdateRuleFilter(filter);
        }

        /// <summary>
        /// 解绑文件更新过滤器
        /// </summary>
        public static void AppUpdaterUnBindFileUpdateRuleFilter()
        {
            CheckIsInitialize("AppUpdaterUnBindFileUpdateRuleFilter");
            s_mService.UnBindFileUpdateRuleFilter();
        }

        /// <summary>
        /// 绑定下载判断方法
        /// </summary>
        /// <param name="judger"></param>
        public static void AppUpdaterBindEnableDownloadJudger(AppUpdaterEnableDownloadJudge judger)
        {
            CheckIsInitialize("AppUpdaterBindEnableDownloadJudger");
            s_mService.BindEnableDownloadJudger(judger);
        }

        /// <summary>
        /// 解绑下载判断方法
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
        /// 获取热更新进度数据
        /// </summary>
        /// <returns></returns>
        public static AppUpdaterProgressData AppUpdaterGetAppUpdaterProgressData()
        {
            CheckIsInitialize("AppUpdaterGetAppUpdaterProgressData");
            return s_mService.Context.ProgressData;
        }

        /// <summary>
        /// 重试更新操作
        /// </summary>
        public static void AppUpdaterStartUpdateAgain()
        {
            CheckIsInitialize("AppUpdaterStartUpdateAgain");
            s_mService.StartUpdateAgain();
        }

        /// <summary>
        /// 设置需要保留的文件夹名（相对于外部资源目录）
        /// 用于在安装高版本后，设定不必删除的资源文件夹（一般这些目录的资源非常大，比如视频，在覆盖安装app后，不必将这些文件删除掉并重新下载）
        /// </summary>
        /// <param name="name">
        /// 需要保留的文件夹名
        /// 注意：目前只支持在资源根目录下，比如：此目录为"assets/effects",其中"assets"为资源根目录，则name为"effects"时，当内建版本
        /// 大于外部记录的版本号时，清理老文件的时候会保留effects目录的内容
        /// </param>
        public static void AppUpdaterSetRetainedDataFolderName(string name)
        {
            CheckIsInitialize("AppUpdaterSetRetainedDataFolderName");
            s_mService.SetRetainedDataFolderName(name);
        }

        /// <summary>
        /// 开始更新子资源
        /// </summary>
        public static void AppUpdaterStartDownloadPartialDataRes()
        {
            CheckIsInitialize("AppUpdaterStartDownloadPartialDataRes");
            s_mService.StartDownloadPartialDataRes();
        }

        /// <summary>
        /// 打开商店
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
        /// 获取当前服务器配置数据
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
        /// 获取当前App信息清单
        /// </summary>
        /// <returns></returns>
        public static AppInfoManifest AppUpdaterGetAppInfoManifest()
        {
            CheckIsInitialize("AppUpdaterGetAppInfoManifest");
            return AppVersionManager.AppInfo;
        }


        /// <summary>
        /// 获取LighthouseConfig清单
        /// </summary>
        /// <returns></returns>
        public static LighthouseConfig AppUpdaterGetLHConfig()
        {
            CheckIsInitialize("AppUpdaterGetLHConfig");
            return AppVersionManager.LHConfig;
        }

        /// <summary>
        /// 获取渠道名
        /// </summary>
        /// <returns>返回渠道</returns>
        public static string AppUpdaterGetChannel()
        {
            CheckIsInitialize("AppUpdaterGetChannel");
            return AppUpdaterConfigManager.AppUpdaterConfig.channel;
        }

        /// <summary>
        /// 是否更新成功
        /// </summary>
        /// <returns></returns>
        public static bool AppUpdaterIsSucceed()
        {
            if (s_mService == null)
                return false;
            return s_mService.IsSucceed();
        }

        /// <summary>
        /// 获取服务器Url
        /// </summary>
        /// <returns></returns>
        public static string AppUpdaterGetServerUrl()
        {
            CheckIsInitialize("AppUpdaterGetServerUrl");
            return AppVersionManager.ServerUrl;
        }

        /// <summary>
        /// 设置示意，每种类型的示意都有对应的数值范围
        /// </summary>
        /// <param name="hintName">示意类型</param>
        /// <param name="hintVal">值</param>
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
