using System;
using System.IO;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.AppUpdaterLib.Runtime.Manifests;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal sealed class AppUpdateGetLighthouseConfigState : BaseAppUpdaterFunctionalState
    {
        #region Enums

        public enum LogicState
        {
            Idle,

            /// <summary>
            /// 加载app的版本信息
            /// </summary>
            LoadAppVerisonInfo,

            /// <summary>
            /// 正在加载app版本信息
            /// </summary>
            LoadingAppVerisonInfo,

            /// <summary>
            ///     请求Lighthouse配置
            /// </summary>
            ReqLighthouseConfig,

            /// <summary>
            ///     请求Lighthouse配置中
            /// </summary>
            RequestingLighthouseConfig,

            /// <summary>
            ///     从Oss请求Lighthouse配置
            /// </summary>
            ReqLighthouseConfigFromOss,

            /// <summary>
            ///     从Oss请求Lighthouse配置中
            /// </summary>
            RequestingLighthouseConfigFromOss,

            /// <summary>
            /// 检查本地资源清单
            /// </summary>
            CheckLocalResManifest,

            CheckLocalResManifesting,

            /// <summary>
            ///     向HttpServer请求配置信息是否可靠
            /// </summary>
            ReqHttpServer,

            /// <summary>
            ///     HttpServer请求中
            /// </summary>
            RequestingHttpServer,

            /// <summary>
            ///     当前lighthouse配置较旧,下载HttpServer返回的lighthouse id对应的Lighthouse配置
            /// </summary>
            ReqLighthouseConfigAgain,

            /// <summary>
            ///     再次请求中
            /// </summary>
            RequestingLighthouseConfigAgain,

            /// <summary>
            ///     请求lighthouse配置失败
            /// </summary>
            ReqLighthouseConfigFailure,

            /// <summary>
            ///     获取lighthouse配置完成
            /// </summary>
            GetLighthouseCompleted
        }

        public enum CheckingResManifestType
        {
            UnKnow,

            UnityRes,

            Done,
        }



        #endregion

        //--------------------------------------------------------------

        #region Fields

        //--------------------------------------------------------------

        private LogicState mState = LogicState.Idle;

        private CheckingResManifestType mCheckingResManifestType = CheckingResManifestType.UnKnow;

        /// <summary>
        ///     当前lighthouse 配置
        /// </summary>
        private LighthouseConfig mCurrentLighthouseConfig;

        /// <summary>
        ///     当前下载的Lighthouse文件的来源
        /// </summary>
        private FileServerType mCurLighthouseFromTo = FileServerType.CDN;

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        public override void Reset()
        {
            this.mState = LogicState.Idle;
            this.mCheckingResManifestType = CheckingResManifestType.UnKnow;
            this.mCurrentLighthouseConfig = null;
            this.mCurLighthouseFromTo = FileServerType.CDN;
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------


        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            base.Enter(entity, args);

            if (Context.LighthouseConfigDownloader == null)
                Context.LighthouseConfigDownloader = new LighthouseConfigDownloader(Context, Target.Request);

            mState = LogicState.LoadAppVerisonInfo;
        }

        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);

            Context.LighthouseConfigDownloader.Update();
            Context.Requester.Update();

            switch (mState)
            {
                case LogicState.LoadAppVerisonInfo:
                    LoadAppVersion();
                    break;
                case LogicState.ReqLighthouseConfig:
                    StartReqLighthouseConfig();
                    break;
                case LogicState.ReqLighthouseConfigFromOss:
                    StartReqLighthouseConfigFromOss();
                    break;
                case LogicState.CheckLocalResManifest:
                    this.StartCheckUnityDataRes();
                    break;
                case LogicState.CheckLocalResManifesting:
                    this.StartCheckingResourceManifest();
                    break;
                case LogicState.ReqHttpServer:
                    RequestHttpServer();
                    break;
                case LogicState.ReqLighthouseConfigAgain:
                    StartReqLighthouseConfig(Context.GetVersionResponseInfo.lighthouseId);
                    break;
                case LogicState.GetLighthouseCompleted:
                    Target.ChangeState<AppVersionCheckState>();
                    break;
                case LogicState.ReqLighthouseConfigFailure:
                    OnFail();
                    break;
            }
        }


        private void OnFail()
        {
            Target.ChangeState<AppUpdateFailureState>();
        }


        #region Get lighthouse config

        private void StartReqLighthouseConfig(string lighthouseId = null)
        {
            if (string.IsNullOrEmpty(lighthouseId))
            {
                Logger.Info("Start request lighthouse config!");
                mState = LogicState.RequestingLighthouseConfig;
            }
            else
            {
                Logger.Warn($"Start request lighthouse config that id is {lighthouseId} !");
                mState = LogicState.RequestingLighthouseConfigAgain;
            }

            Context.LighthouseConfigDownloader.StartGetLighthouseConfigFromRemote(
                OnGetLighthouseConfigFromRemoteCallback
                , lighthouseId);
        }

        private void StartReqLighthouseConfigFromOss()
        {
            Logger.Info("Start request lighthouse config from oss.");
            mState = LogicState.RequestingLighthouseConfigFromOss;
            Context.LighthouseConfigDownloader.StartGetLighthouseConfigFromOss(OnGetLighthouseConfigFromRemoteCallback);
        }

        private bool IsServersConfigValid(LighthouseConfig config)
        {
            var servers = config.ServersData.Servers;
            bool valid = false;
            var curVersion = AppVersionManager.AppInfo.version;
            for (int i = 0; i < servers.Count; i++)
            {
                var serverData = servers[i];

                if (serverData.CanBeUseForVersion(curVersion))
                {
                    valid = true;
                    break;
                }
            }

            return valid;
        }

        private void OnGetLighthouseConfigFromRemoteCallback(bool success, string contents)
        {
            if (success)
            {
                mCurLighthouseFromTo = Context.LighthouseConfigDownloader.CurRequestFileServerType;

                switch (mState)
                {
                    case LogicState.RequestingLighthouseConfig:

                        try
                        {
                            Logger.Debug($"Lighthouse content : \r\n {contents}");
                            this.Target.OnGetLighthouseContentCallback(contents);
                            mCurrentLighthouseConfig = LighthouseConfig.ReadFromJson(contents);
                            if (!IsServersConfigValid(mCurrentLighthouseConfig))
                            {
                                AppVersionManager.MakeCurrentLighthouseConfig(this.mCurrentLighthouseConfig);
                                Context.ErrorType = AppUpdaterErrorType.LighthouseConfigServersIsUnReachable;
                                this.mState = LogicState.ReqLighthouseConfigFailure;
                                return;
                            }

                            mState = LogicState.CheckLocalResManifest;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(
                                $"Parse lighthouse config that from remote file server failure! StackTrace : {e.StackTrace}");
                            Logger.Debug("Type:" + mCurLighthouseFromTo);
                            if (mCurLighthouseFromTo != FileServerType.OSS)
                            {
                                mState = LogicState.ReqLighthouseConfigFromOss;
                            }
                            else
                            {
                                Context.ErrorType = AppUpdaterErrorType.ParseLighthouseConfigError;
                                mState = LogicState.ReqLighthouseConfigFailure;
                            }
                        }

                        break;
                    case LogicState.RequestingLighthouseConfigAgain:
                        this.Target.OnGetLighthouseContentCallback(contents);
                        var targetLighthouseConfig = LighthouseConfig.ReadFromJson(contents);
                        Logger.Info(
                            $"Get target config success , lighthouseId : {targetLighthouseConfig.MetaData.lighthouseId}");
                        if (targetLighthouseConfig.MetaData.lighthouseId == Context.GetVersionResponseInfo.lighthouseId)
                        {
                            AppVersionManager.MakeCurrentLighthouseConfig(targetLighthouseConfig);
                            mState = LogicState.GetLighthouseCompleted;
                        }
                        else
                        {
                            Context.ErrorType = AppUpdaterErrorType.DownloadLighthouseConfigInvalid;
                            Logger.Error($"Download lighthouse config is invalid ! remote id : " +
                                         $"{targetLighthouseConfig.MetaData.lighthouseId}  , " +
                                         $" target id : {Context.GetVersionResponseInfo.lighthouseId}");
                            mState = LogicState.ReqLighthouseConfigFailure;
                        }
                        break;
                    case LogicState.RequestingLighthouseConfigFromOss:
                        try
                        {
                            Logger.Debug($"Oss Lighthouse content : \r\n {contents}");
                            this.Target.OnGetLighthouseContentCallback(contents);
                            mCurrentLighthouseConfig = LighthouseConfig.ReadFromJson(contents);
                            if (!IsServersConfigValid(mCurrentLighthouseConfig))
                            {
                                AppVersionManager.MakeCurrentLighthouseConfig(this.mCurrentLighthouseConfig);
                                Context.ErrorType = AppUpdaterErrorType.LighthouseConfigServersIsUnReachable;
                                this.mState = LogicState.ReqLighthouseConfigFailure;
                                return;
                            }
                            mState = LogicState.CheckLocalResManifest;
                        }
                        catch (Exception e)
                        {
                            Logger.Error(
                                $"Parse lighthouse config that from oss failure! StackTrace : {e.StackTrace}");
                            Context.ErrorType = AppUpdaterErrorType.ParseLighthouseConfigError;
                            mState = LogicState.ReqLighthouseConfigFailure;
                        }
                        break;
                }
            }
            else //下载lighthouse失败
            {
                Context.ErrorType = AppUpdaterErrorType.DownloadLighthouseFailure;
                Logger.Error($"Download lighthouse config failure ! Current state : {mState}");
                mState = LogicState.ReqLighthouseConfigFailure;
            }
        }

        #endregion

        #region Get appinfo

        private void LoadAppVersion()
        {
            mState = LogicState.LoadingAppVerisonInfo;
            LoadLocalBuiltInAppInfo();
        }

        private void LoadLocalBuiltInAppInfo()
        {
            var innerAppInfoPath = Context.GetStreamingAssetsPath(AssetsFileSystem.AppInfoFileName);
            Logger.Debug($"Start load built appinfo file , path : {innerAppInfoPath}");
            Target.Request.Load(innerAppInfoPath, LocalBuiltInAppInfoCallback);
        }

        private void LocalBuiltInAppInfoCallback(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                Logger.Error($"Load built app info file failure , file name is \"{AssetsFileSystem.AppInfoFileName}\" !");
                Context.ErrorType = AppUpdaterErrorType.LoadBuiltinAppInfoFailure;
                this.mState = LogicState.ReqLighthouseConfigFailure;
                return;
            }

            AppInfoManifest builtinAppInfo;
            try
            {
                builtinAppInfo = AppVersionManager.ParseAppInfo(bytes);
            }
            catch (Exception ex)
            {
                Logger.Error($"Parse built app info failure , error stackTrace : {ex.StackTrace}");
                Context.ErrorType = AppUpdaterErrorType.ParseBuiltinAppInfoFailure;
                this.mState = LogicState.ReqLighthouseConfigFailure;
                return;
            }

            AppInfoManifest localAppInfo = null;
            try
            {
                localAppInfo = AppVersionManager.LoadLocalAppInfo();
            }
            catch (Exception ex)
            {
                Logger.Error($"Parse local app info failure , error stackTrace : {ex.StackTrace}");
                Context.ErrorType = AppUpdaterErrorType.ParseLocalAppInfoFailure;
                this.mState = LogicState.ReqLighthouseConfigFailure;
                return;
            }

            if (localAppInfo != null)
            {
                Logger.Debug($"Local veriosn : {localAppInfo.version} , builtin verison : {builtinAppInfo.version}");

                var builtinVerison = new Version(builtinAppInfo.version);
                var localVersion = new Version(localAppInfo.version);
                var result = builtinVerison.CompareTo(localVersion);

                if (result > Version.VersionCompareResult.Equal)
                {
                    this.ClearExternalStorage();
                    Logger.Info("Make built appinfo as current!");
                    AppVersionManager.MakeCurrentAppInfo(builtinAppInfo);
                }
                else if (result < Version.VersionCompareResult.LowerForPatch)
                {
                    Context.ErrorType = AppUpdaterErrorType.AppBuiltInVersionNumNotCompatibleToExternal;
                    this.mState = LogicState.ReqLighthouseConfigFailure;
                    return;
                }
                else
                {
                    Logger.Info("Make local appinfo as current!");
                    AppVersionManager.MakeCurrentAppInfo(localAppInfo);

                }
            }
            else
            {
                Logger.Info("Local appinfo file is not exist , make built appinfo file as current!");
                AppVersionManager.MakeCurrentAppInfo(builtinAppInfo);
            }
            this.mState = LogicState.ReqLighthouseConfig;
        }

        private void ClearExternalStorage()
        {
            try
            {
                bool hasRetainedDataFolderName = this.Target.RetainedDataFolderNameList.Count > 0;
                var pureRetainedDataFolderNameList = this.Target.RetainedDataFolderNameList;
                if (hasRetainedDataFolderName)
                {
                    for (var i = 0; i < pureRetainedDataFolderNameList.Count; i++)
                    {
                        var pureRetainedDataFolderName = pureRetainedDataFolderNameList[i];
                        if (pureRetainedDataFolderName.StartsWith("/"))
                            pureRetainedDataFolderName = pureRetainedDataFolderName.Substring(1);
                        if (pureRetainedDataFolderName.EndsWith("/"))
                            pureRetainedDataFolderName = pureRetainedDataFolderName.Substring(0, pureRetainedDataFolderName.Length - 1);
                        pureRetainedDataFolderNameList[i] = pureRetainedDataFolderName;
                    }
                }

                //Delete directories
                var lastVersionDirs = Directory.GetDirectories(AssetsFileSystem.RootFolder);
                foreach (var dir in lastVersionDirs)
                {
                    if (hasRetainedDataFolderName)
                    {
                        var dirName = dir.Replace(@"\", "/");
                        int idx = dirName.LastIndexOf("/", StringComparison.Ordinal);
                        if (idx != -1)
                        {
                            dirName = dirName.Substring(idx + 1);
                        }

                        bool isDirInRetainedList = false;
                        foreach (var pureRetainedDataFolderName in pureRetainedDataFolderNameList)
                        {
                            if (string.Equals(dirName, pureRetainedDataFolderName))
                            {
                                isDirInRetainedList = true;
                                break;
                            }
                        }

                        if (!isDirInRetainedList)
                        {
                            Logger.Debug($"Deleting dirrectory that name is \"{dirName}\".");
                            if (Directory.Exists(dir))
                            {
                                Directory.Delete(dir, true);
                            }
                        }
                    }
                    else
                    {
                        if (Directory.Exists(dir))
                        {
                            Directory.Delete(dir, true);
                        }
                    }

                }

                var lastVersionFiles = Directory.GetFiles(AssetsFileSystem.RootFolder, "*.*");

                foreach (var fileName in lastVersionFiles)
                {
                    Logger.Debug($"Deleting file that name is \"{fileName}\".");
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
                Logger.Info($"Clear external app folder that name is \"{AssetsFileSystem.RootFolder}\" completed!");
            }
            catch (Exception ex)
            {
                Logger.Fatal($"Error message : {ex.Message} \n StackTrace : {ex.StackTrace}");
                Context.ErrorType = AppUpdaterErrorType.DeleteExternalStorageFilesFailure;
                this.mState = LogicState.ReqLighthouseConfigFailure;
                return;
            }

        }

        #endregion

        #region Check local resource manifest
        private void StartCheckUnityDataRes()
        {
            this.mCheckingResManifestType = CheckingResManifestType.UnityRes;
            var localUnityDataResName = AssetsFileSystem.UnityResManifestName;
            if (!AppVersionManager.IsLocalResManifestExist(localUnityDataResName) && AppUpdaterHints.Instance.EnableUnityResUpdate)
            {
                Logger.Info($"The local manifest file that name is \"{localUnityDataResName}\" is not exist, Start load builtin !");
                Target.Request.Load(Context.GetStreamingAssetsPath(localUnityDataResName), OnLoadResManifestFileComplted);
            }
            else
            {
                Logger.Info($"The local manifest file that name is \"{localUnityDataResName}\" is exist!");
                this.mCheckingResManifestType = CheckingResManifestType.Done;
            }
        }

        private void OnLoadResManifestFileComplted(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                if (this.mCheckingResManifestType == CheckingResManifestType.UnityRes)
                {
                    throw new FileNotFoundException(AssetsFileSystem.UnityResManifestName);
                }

                throw new InvalidOperationException("Load resource manifest file error!");
            }
            else
            {
                if (this.mCheckingResManifestType == CheckingResManifestType.UnityRes)
                {
                    Logger.Info("Load builtin untiy resource manifest completd !");
                    this.mCheckingResManifestType = CheckingResManifestType.Done;
                    AppVersionManager.SaveToLocalDataResManifest(data, AssetsFileSystem.UnityResManifestName);
                }
                else
                {
                    throw new InvalidOperationException($"Checking manifest type : {this.mCheckingResManifestType.ToString()}");
                }
            }
        }

        private void StartCheckingResourceManifest()
        {
            if (this.mCheckingResManifestType == CheckingResManifestType.Done)
            {
                this.mState = LogicState.ReqHttpServer;
                Logger.Info("Checking resource manifest complted!");
            }
        }

        #endregion

        #region Request getVersion response

        private void RequestHttpServer()
        {
            mState = LogicState.RequestingHttpServer;

            var serverData = mCurrentLighthouseConfig.GetCurrentServerData();
            var appUpdaterConfig = AppUpdaterConfigManager.AppUpdaterConfig;
            Context.Requester.ReqGetVersion(serverData, AppVersionManager.AppInfo.version,
                mCurrentLighthouseConfig.MetaData.lighthouseId,
                appUpdaterConfig.channel,
                mCurLighthouseFromTo,
                info =>
                {
                    Logger.Debug($"info != null : {info != null} .");
                    if (info != null)
                        Logger.Debug($"string.IsNullOrEmpty(info.lighthouseId) : {string.IsNullOrEmpty(info.lighthouseId)}");
                    if (info != null && !string.IsNullOrEmpty(info.lighthouseId))
                    {
                        int subIndex = info.url.LastIndexOf("/lighthouse");
                        string url = info.url.Substring(0, subIndex);
                        AppVersionManager.MakeCurrentServerUrl(url);
                        Context.GetVersionResponseInfo = info;
                        if (info.forceUpdate || info.maintenance)
                        {
                            if (info.lighthouseId == mCurrentLighthouseConfig.MetaData.lighthouseId)
                            {
                                Logger.Info($"forceUpdate : {info.forceUpdate}  maintenance : {info.maintenance}");
                                Context.GetVersionResponseInfo = info;
                                AppVersionManager.MakeCurrentLighthouseConfig(mCurrentLighthouseConfig);
                                mState = LogicState.GetLighthouseCompleted;
                            }
                            else
                            {
                                Logger.Info("The server may be in maintence or you should to download latest app and install it ." +
                                            "The current lighthouseconfig is old , try to get latest!");
                                mState = LogicState.ReqLighthouseConfigAgain;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(info.lighthouseId))
                            {
                                Logger.Error("The lighthouse id that was return by server is null or empty!");
                                Context.ErrorType = AppUpdaterErrorType.RequestGetVersionFailure;
                                mState = LogicState.ReqLighthouseConfigFailure;
                                return;
                            }
                            if (info.lighthouseId == mCurrentLighthouseConfig.MetaData.lighthouseId)
                            {
                                Logger.Info("The current lighthouseconfig is latest !");
                                AppVersionManager.MakeCurrentLighthouseConfig(mCurrentLighthouseConfig);

                                mState = LogicState.GetLighthouseCompleted;
                            }
                            else
                            {
                                Logger.Info("The current lighthouseconfig is old , try to get a new one !");
                                mState = LogicState.ReqLighthouseConfigAgain;
                            }
                        }
                    }
                    else
                    {
                        if (serverData.MaintenanceInfo.IsOpen)
                        {
                            Logger.Info("The server is in maintence .");
                            this.Target.OnMaintenanceCallBack(serverData.MaintenanceInfo);
                            Context.AppendInfo("The server is in maintence .");
                        }
                        else
                        {
                            Context.ErrorType = AppUpdaterErrorType.RequestGetVersionFailure;
                            mState = LogicState.ReqLighthouseConfigFailure;
                        }
                    }
                });
        }

        #endregion

        #endregion
    }
}