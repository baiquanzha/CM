using UnityEngine;
using System;
using MTool.AppUpdaterLib.Runtime.Diagnostics;
using MTool.AppUpdaterLib.Runtime.Interfaces;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.Core.FSM;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime
{
    internal sealed class AppUpdaterService : MonoBehaviour
    {
        #region Fields

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("AppUpdaterService"));

        private AppUpdaterFsmOwner mOwner;

        private IStorageInfoProvider _mStorageInfoProvider = null;

        #endregion


        #region Properties And Events

        public AppUpdaterContext Context => mOwner?.Context;

        #endregion

        #region Init

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            this.InitializeComponent();
        }

        void Start()
        {
            if (!AppUpdaterHints.Instance.ManualPerformAppUpdate)
            {
                this.StartUpdate();
            }
        }

        public void InitializeComponent()
        {
            this.CreateFSMOwner();
            this.InitOthers();
        }

        private void CreateFSMOwner()
        {
            this.mOwner = new AppUpdaterFsmOwner();
            this.mOwner.Init();
        }

        private void InitOthers()
        {
#if DEBUG_APP_UPDATER
            this.gameObject.AddComponent<AppUpdaterDebugView>();
            Context.FetchDeviceStorageInfo();
#endif
        }

        #endregion

        #region Set Callbacks

        public void SetErrorCallback(AppUpdaterErrorCallback callback)
        {
            this.mOwner.SetErrorCallback(callback);
        }

        public void SetServerMaintenanceCallback(AppUpdaterServerMaintenanceCallback callback)
        {
            this.mOwner.SetServerMaintenanceCallback(callback);
        }

        public void SetForceUpdateCallback(AppUpdaterForceUpdateCallback callback)
        {
            this.mOwner.SetForceUpdateCallback(callback);
        }

        public void SetOnTargetVersionObtainCallback(AppUpdaterOnTargetVersionObtainCallback callback)
        {
            this.mOwner.SetOnTargetVersionObtainCallback(callback);
        }

        public void SetPerformCompletedCallback(AppUpdaterPerformCompletedCallback callback)
        {
            this.mOwner.SetPerformCompletedCallback(callback);
        }

        public void SetGetLighthouseContentCallback(AppUpdaterGetLighthouseContentCallback callback)
        {
            this.mOwner.SetGetLighthouseContentCallback(callback);
        }

        public void SetStartDownloadMissingResCallback(AppUpdaterStartDownloadMissingResCallback callback)
        {
            this.mOwner.SetStartDownloadMissingResCallback(callback);
        }

        public void SetDownloadMissingResCompleteCallback(AppUpdaterDownloadMissingResCompleteCallback callback)
        {
            this.mOwner.SetDownloadMissingResCompleteCallback(callback);
        }

        #endregion

        public void SetAppUpdaterRequester(IAppUpdaterRequester requester)
        {
            Context.Requester = requester;
        }

        public void SetStorageInfoProvider(IStorageInfoProvider provider)
        {
            this._mStorageInfoProvider = provider;
            if (this.Context != null)
            {
                this.Context.StorageInfoProvider = provider;
            }
        }

        public void ManualStartAppUpdate()
        {
            this.mOwner.ManualStartAppUpdate();
        }

        public void StartUpdate()
        {
            this.mOwner.StartupFsm();
        }

        public void StartUpdateAgain()
        {
            if (Context.IsFirstRun)
            {
                s_mLogger.Value?.Warn("Please call mathod that name is \"StartUpdate\" , because the appupdater is not running yet!");
                return;
            }

            this.mOwner.StartUpdateOperationAgain();
        }

        public bool IsSucceed()
        {
            return this.mOwner.State == AppUpdaterFsmOwner.AppUpdaterState.Done;
        }

        public void BindFileUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            this.mOwner.BindFileUpdateRuleFilter(filter);
        }

        public void UnBindFileUpdateRuleFilter()
        {
            this.mOwner.UnBindFileUpdateRuleFilter();
        }

        public void BindEnableDownloadJudger(AppUpdaterEnableDownloadJudge judger)
        {
            this.mOwner.BindEnableDownloadJudger(judger);
        }

        public void UnBindEnableDownloadJudger()
        {
            this.mOwner.UnBindEnableDownloadJudger();
        }

        public void BindLocalModeUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            this.mOwner.BindLocalModeUpdateRuleFilter(filter);
        }

        public void UnBindLocalModeUpdateRuleFilter()
        {
            this.mOwner.UnBindLocalModeUpdateRuleFilter();
        }

        public void SetRetainedDataFolderName(string name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));
            s_mLogger.Value?.Debug($"Set retained data folder name : \"{name}\" .");
            this.mOwner.SetRetainedDataFolderName(name);
        }

        public void StartDownloadPartialDataRes()
        {
            if (Context.IsFirstRun)
            {
                s_mLogger.Value?.Warn("Please call mathod that name is \"StartUpdate\" , because the appupdater is not running yet!");
                return;
            }
            this.mOwner.StartDownloadPartialDataRes();
        }

        #region Unity Callbacks

        public void Update()
        {
            this.mOwner?.Update();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            IRoutedEventArgs arg = new RoutedEventArgs<bool>
            {
                EventType = (int)AppUpdaterInnerEventType.OnApplicationFocus,
                arg = hasFocus
            };
            this.mOwner.HandleMessage(in arg);
        }

        private void OnApplicationQuit()
        {
            IRoutedEventArgs arg = new RoutedEventArgs
            {
                EventType = (int)AppUpdaterInnerEventType.OnApplicationQuit,
            };
            this.mOwner.HandleMessage(in arg);
        }

        #endregion
    }
}
