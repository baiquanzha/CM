using System;
using System.Collections.Generic;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using MTool.AppUpdaterLib.Runtime.States.Concretes;
using MTool.Core.FSM;

namespace MTool.AppUpdaterLib.Runtime
{
    internal sealed class AppUpdaterFsmOwner : IFSMOwner
    {
        public enum AppUpdaterState
        {
            Idle,
            Runing,
            Maintenance,
            ForceUpdate,
            Error,
            Done,
        }

        internal class AppUpdaterCallBacks
        {
            public AppUpdaterErrorCallback ErrorCallback;
            public AppUpdaterServerMaintenanceCallback ServermaintenanceCallback;
            public AppUpdaterForceUpdateCallback ForceUpdateCallback;
            public AppUpdaterOnTargetVersionObtainCallback OnTargetVersionObtainCallback;
            public AppUpdaterPerformCompletedCallback PerformCompletedCallback;
            public AppUpdaterGetLighthouseContentCallback GetLighthouseContentCallback;
            public AppUpdaterStartDownloadMissingResCallback StartDownloadMissingResCallback;
            public AppUpdaterDownloadMissingResCompleteCallback DownloadMissingResCompleteCallback;
        }

        #region

        public AppUpdaterState State = AppUpdaterState.Idle;

        private AppUpdaterCallBacks mCallBacks = new AppUpdaterCallBacks();

        private AppUpdaterFileUpdateRuleFilter _fileUpdateRuleFilter = null;
        public AppUpdaterFileUpdateRuleFilter FileUpdateRuleFilter => _fileUpdateRuleFilter;

        private AppUpdaterEnableDownloadJudge _enableDownloadJudger = null;
        public AppUpdaterEnableDownloadJudge EnableDownloadJudger => _enableDownloadJudger;

        private AppUpdaterFileUpdateRuleFilter _localModeUpdateRuleFilter = null;
        public AppUpdaterFileUpdateRuleFilter LocalModeUpdateRuleFilter => _localModeUpdateRuleFilter;

        /// <summary>
        /// 资源的保留目录，此目录在app覆盖安装后不会删除，用于多文件环境下的游戏中的更新
        /// </summary>
        private List<string> _retainedDataFolderNameList = new List<string>();
        public List<string> RetainedDataFolderNameList => _retainedDataFolderNameList;

        private static readonly UnityResManifestParser UnityManifestParser = new UnityResManifestParser();

        private static readonly DataResManifestParser DataResManifestParser = new DataResManifestParser(); 

        #region Inner FSM

        private AppUpdaterContext _mContext = new AppUpdaterContext();
        public AppUpdaterContext Context => _mContext;

        private StateMachine<AppUpdaterFsmOwner> mFSM;
        public StateMachine<AppUpdaterFsmOwner> FSM => mFSM;

        #endregion

        public HttpRequest Request { set; get; } = new HttpRequest();

        #endregion

        public void Init()
        {
            this.CreateFsm();
        }

        private void CreateFsm()
        {
            this.mFSM = new StateMachine<AppUpdaterFsmOwner>(this);
        }

        public bool HandleMessage(in IRoutedEventArgs msg)
        {
            return this.mFSM.HandleMessage(in msg);
        }

        public void ChangeState<T>() where T : State<AppUpdaterFsmOwner>,new() 
        {
            this.mFSM.ChangeState<T>();
        }

        private void InitializeFsm()
        {
            this.mFSM.SetCurrentState<AppUpdateInitState>();
            this.mFSM.SetGlobalState<AppUpdaterGlobalState>();
        }

        public void Update()
        {
            this.Request.Update();
            mFSM?.Update();
        }

        public void StartupFsm()
        {
            if (_mContext.IsFirstRun)
            {
                this.InitializeFsm();
                _mContext.IsFirstRun = false;
            }
            else
            {
                this.mFSM.ChangeState<AppUpdateInitState>();
            }
        }

        public void StartUpdateOperationAgain() 
        {
            this.Clear();
            Context.AppendInfo("Start app update operation again !");
            this.StartupFsm();

            if (AppUpdaterHints.Instance.ManualPerformAppUpdate)
            {
                this.ManualStartAppUpdate();
            }
        }


        public void BindFileUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            this._fileUpdateRuleFilter = filter;
        }

        public void UnBindFileUpdateRuleFilter()
        {
            this._fileUpdateRuleFilter = null;
        }

        public void BindEnableDownloadJudger(AppUpdaterEnableDownloadJudge judger)
        {
            this._enableDownloadJudger = judger;
        }

        public void UnBindEnableDownloadJudger()
        {
            this._enableDownloadJudger = null;
        }

        public void BindLocalModeUpdateRuleFilter(AppUpdaterFileUpdateRuleFilter filter)
        {
            this._localModeUpdateRuleFilter = filter;
        }

        public void UnBindLocalModeUpdateRuleFilter()
        {
            this._localModeUpdateRuleFilter = null;
        }

        public void SetRetainedDataFolderName(string name)
        {
            this._retainedDataFolderNameList.Add(name);
        }

        public void StartDownloadPartialDataRes()
        {
            this.Clear();
            Context.AppendInfo("Start resource partial update operation again !");
            IRoutedEventArgs arg = new RoutedEventArgs()
            {
                EventType = (int)AppUpdaterInnerEventType.StartPerformResPartialUpdateOperation
            };
            this.HandleMessage(in arg);
        }

        public void ManualStartAppUpdate()
        {
            IRoutedEventArgs arg = new RoutedEventArgs()
            {
                EventType = (int)AppUpdaterInnerEventType.PerformAppUpdate
            };
            this.HandleMessage(in arg);
        }


        public BaseResManifestParser GetResManifestParserByType(UpdateResourceType type)
        {
            if (type == UpdateResourceType.TableData)
            {
                return DataResManifestParser;
            }
            else if (type == UpdateResourceType.NormalResource)
            {
                return UnityManifestParser;
            }

            throw new ArgumentException($"UpdateResourceType : {type}");
        }
            

        internal void Clear()
        {
            Context?.Clear();
        }


        #region 由外部发送的事件封装


        #endregion

        #region Set Callbacks

        public void SetErrorCallback(AppUpdaterErrorCallback callback)
        {
            mCallBacks.ErrorCallback = callback;
        }

        public void SetServerMaintenanceCallback(AppUpdaterServerMaintenanceCallback callback)
        {
            mCallBacks.ServermaintenanceCallback = callback;
        }

        public void SetForceUpdateCallback(AppUpdaterForceUpdateCallback callback)
        {
            mCallBacks.ForceUpdateCallback = callback;
        }

        public void SetOnTargetVersionObtainCallback(AppUpdaterOnTargetVersionObtainCallback callback)
        {
            mCallBacks.OnTargetVersionObtainCallback = callback;
        }

        public void SetPerformCompletedCallback(AppUpdaterPerformCompletedCallback callback)
        {
            mCallBacks.PerformCompletedCallback = callback;
        }

        public void SetGetLighthouseContentCallback(AppUpdaterGetLighthouseContentCallback callback)
        {
            mCallBacks.GetLighthouseContentCallback = callback;
        }

        public void SetStartDownloadMissingResCallback(AppUpdaterStartDownloadMissingResCallback callback)
        {
            mCallBacks.StartDownloadMissingResCallback = callback;
        }

        public void SetDownloadMissingResCompleteCallback(AppUpdaterDownloadMissingResCompleteCallback callback)
        {
            mCallBacks.DownloadMissingResCompleteCallback = callback;
        }

        #endregion

        #region Callbacks


        public void OnErrorCallback(AppUpdaterErrorType errorType, string desc)
        {
            this.State = AppUpdaterState.Error;
            this.mCallBacks.ErrorCallback?.Invoke(errorType, desc);
        }

        public void OnMaintenanceCallBack(LighthouseConfig.MaintenanceInfo maintenanceInfo)
        {
            this.State = AppUpdaterState.Maintenance;
            this.mCallBacks.ServermaintenanceCallback?.Invoke(maintenanceInfo);
        }

        public void OnForceUpdateCallBack(LighthouseConfig.UpdateDataInfo info)
        {
            this.State = AppUpdaterState.ForceUpdate;
            this.mCallBacks.ForceUpdateCallback?.Invoke(info);
        }


        public void OnnTargetVersionObtainCallback(string version)
        {
            this.mCallBacks.OnTargetVersionObtainCallback?.Invoke(version);
        }


        public void OnCompletedCallback()
        {
            this.State = AppUpdaterState.Done;
            this.mCallBacks.PerformCompletedCallback?.Invoke();
        }

        public void OnGetLighthouseContentCallback(string content)
        {
            this.mCallBacks.GetLighthouseContentCallback?.Invoke(content);
        }

        public void OnStartDownloadMissingResCallback(ulong size, ulong count)
        {
            this.mCallBacks.StartDownloadMissingResCallback?.Invoke(size, count);
        }

        public void OnDownloadMissingResCompleteCallback()
        {
            this.mCallBacks.DownloadMissingResCompleteCallback?.Invoke();
        }
        #endregion

    }
}
