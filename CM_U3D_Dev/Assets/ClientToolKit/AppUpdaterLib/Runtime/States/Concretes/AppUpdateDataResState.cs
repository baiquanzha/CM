using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MTool.AppUpdaterLib.Runtime.Download;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using MTool.Core.FSM;
using MTool.Core.Functional;
using CommonServiceLocator;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal sealed class AppUpdateDataResState : BaseAppUpdaterFunctionalState
    {
        #region Inner class&enum

        public enum InnerState
        {
            Idle,

            StartRequestResManifest,

            RequestingManifest,

            StartLoadResManifest,

            LoadingResManifest,

            StartCalculateResDiff,

            CalculatingResDiff,
            
            DownloadAndApplyDiff,

            ResUpdateCompleted,

            ResUpdateFailed,
        }

        public enum FileDownLoadState
        {
            Idle,
            StartDownLoad,
            DownLoading,
            DownLoadCompleted,
            DownloadFail
        }

        #endregion

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private InnerState mCurState = InnerState.Idle;

        private FileServerType mCurFileServerType = FileServerType.CDN;

        private VersionManifest mCurRemoteManifest = null;

        private FileDownLoadState mCurFileDownLoadState = FileDownLoadState.Idle;

        private List<FileDesc> mCurDownloadTasks = new List<FileDesc>();
        private int mCurrentDownloadIndex = 0;

        private BaseResManifestParser mCurManifestParser = null;

        private VersionManifest mLocalManifest = null;

        private List<FileDesc> diff = null;

        private UpdateResMap updateResMap = null;
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

        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);
            switch (mCurState)
            {
                case InnerState.StartRequestResManifest:
                    this.RequestResManifest(FileServerType.CDN);
                    break;
                case InnerState.StartLoadResManifest:
                    this.LoadResManifest();
                    break;
                case InnerState.StartCalculateResDiff://开始计算差异
                    this.ProcessCalculateResDifference();
                    break;
                case InnerState.DownloadAndApplyDiff://开始应用差异
                    this.ProcessDownloadAndApplyDiff();
                    break;
                case InnerState.ResUpdateCompleted://更新完成
                    this.ProcessResUpdateCompleted();
                    break;
                case InnerState.ResUpdateFailed:
                    this.OnResUpdateFailure();
                    break;

            }
        }

        private void RequestResManifest(FileServerType fileServerType)
        {
            this.mCurFileServerType = fileServerType;
            string url = Context.GetCurrentVerisonFileUrl(this.mCurFileServerType);
            Logger.Info($"Start request remote resource manifest {url} .");

            Context.AppendInfo($"ResourceType : {Context.GetCurrentUpdateType()}  MD5 : C:{Context.GetCurrentLocalMd5()} --> R:{Context.GetCurrentRemoteMd5()}");
            this.mCurState = InnerState.RequestingManifest;
            this.Target.Request.Load(url, OnResponseResManifestCallback);
        }

        private void OnResponseResManifestCallback(byte[] netData)
        {
            if (netData == null || netData.Length == 0)
            {
                if (this.mCurFileServerType == FileServerType.CDN)
                {
                    this.RequestResManifest(FileServerType.OSS);
                }
                else
                {
                    Context.ErrorType = AppUpdaterErrorType.RequestResManifestFailure;
                    this.mCurState = InnerState.ResUpdateFailed;
                }
            }
            else
            {
                string resManifestContent = System.Text.Encoding.UTF8.GetString(netData);

                var version = Context.GetCurrentRemoteMd5();
                this.mCurManifestParser = this.Target.GetResManifestParserByType(Context.GetCurrentUpdateType());
                Context.ProgressData.CurrentUpdateResourceType = this.mCurManifestParser.GetUpdateResourceType();

                Logger.InfoFormat($"veriosn : {version} , parser : {this.mCurManifestParser.GetType().Name} .");

                try
                {
                    this.mCurRemoteManifest = this.mCurManifestParser.Parse(resManifestContent);
                }
                catch (Exception ex)
                {
                    Context.ErrorType = AppUpdaterErrorType.ParseRemoteResManifestFailure;

                    this.mCurState = InnerState.ResUpdateFailed;

                    Logger.Error($"Error stackTrace : {ex.StackTrace}");

                    return;
                }
                
                this.mCurState = InnerState.StartLoadResManifest;
            }
        }

        private void LoadResManifest()
        {
            this.mCurState = InnerState.LoadingResManifest;
            string localManifestPath = AssetsFileSystem.GetWritePath(AssetsFileSystem.UnityResManifestName);
            if (File.Exists(localManifestPath))
            {
                var localContent = File.ReadAllText(localManifestPath);
                Context.LocalResManifest = VersionManifestParser.Parse(localContent);
            }
            string builtInManifestPath = AssetsFileSystem.GetStreamingAssetsPath(AssetsFileSystem.UnityResManifestName, null, false);
            this.Target.Request.Load(builtInManifestPath, OnLoadBuiltInResManifest);
        }

        private void OnLoadBuiltInResManifest(byte[] data)
        {
            if (data == null && data.Length == 0)
            {
                Context.ErrorType = AppUpdaterErrorType.LoadBuiltinResManifestFailure;
                this.mCurState = InnerState.ResUpdateFailed;
            }
            else
            {
                var content = new System.Text.UTF8Encoding(false, true).GetString(data);
                Context.BuiltInResManifest = VersionManifestParser.Parse(content);
                this.mCurState = InnerState.StartCalculateResDiff;
            }
        }

        private void OnResUpdateFailure()
        {
            this.Target.ChangeState<AppUpdateFailureState>();
        }

        private void ProcessCalculateResDifference()
        {
            Logger.Debug($"Start calculate resource difference !");
            this.mCurState = InnerState.CalculatingResDiff;

            var localResManifestContents= AppVersionManager.LoadLocalResManifestContents(Context.GetCurrentLocalVersionFileName());

            if (string.IsNullOrEmpty(localResManifestContents))
            {
                throw new FileNotFoundException(Context.GetCurrentLocalVersionFileName());
            }

            try
            {
                this.mLocalManifest = this.mCurManifestParser.Parse(localResManifestContents);
            }
            catch (Exception ex)
            {
                Context.ErrorType = AppUpdaterErrorType.ParseLocalResManifestFailure;

                this.mCurState = InnerState.ResUpdateFailed;

                Logger.Error($"Error stackTrace : {ex.StackTrace}");

                return;
            }

            diff = null;
            var updateType = Context.GetCurrentUpdateType();
            if (updateType == UpdateResourceType.TableData)
            {
                Logger.Debug("Start sync table data!");
                diff = this.mLocalManifest.CalculateDifference(this.mCurRemoteManifest);
            }
            else
            {
                var syncMode = Context.ResUpdateConfig.Mode;
                var filter = Context.ResUpdateConfig.Filter;
                var localmodefilter = Context.ResUpdateConfig.LocalModeFilter;
                Logger.Debug($"Start sync unity resource , syncMode : {syncMode}  , filter null : {filter == null}, local mode filter null : {localmodefilter == null}");
                diff = this.mLocalManifest.CalculateDifference(this.mCurRemoteManifest, syncMode, filter, localmodefilter);
            }
            Context.LoadUpdateResMap(out updateResMap);

            if (diff != null && diff.Count > 0)
            {
                ulong totalDownloadSize = 0;
                diff.ForeachCall(x => totalDownloadSize += (ulong)x.S);
                Context.ProgressData.TotalDownloadSize += totalDownloadSize;
                Context.ProgressData.TotalDownloadFileCount += (ulong)diff.Count;

                Logger.Info($"Needed to update {diff.Count} files , total size is {totalDownloadSize}B .");

                Context.FetchDeviceStorageInfo();

                if (Context.DiskInfo.IsGetReady && Context.DiskInfo.BusySpace + CommonConst.MIN_DISK_AVAILABLE_SPACE > Context.DiskInfo.TotalSpace)
                {
                    Context.ErrorType = AppUpdaterErrorType.DiskIsNotEnoughToDownPatchFiles;
                    this.mCurState = InnerState.ResUpdateFailed;
                }
                else
                {
                    var downloadJudger = this.Target.EnableDownloadJudger;
                    if (downloadJudger != null)
                        downloadJudger.Invoke(totalDownloadSize, ConfirmDownload, GiveupDownload);
                    else
                        ConfirmDownload();
                }
            }
            else
            {
                Logger.Info($"The current remote manifest file that name is {Context.GetCurrentLocalVersionFileName()} is the same as local .");
                this.mCurState = InnerState.ResUpdateCompleted;
            }
        }

        private void ConfirmDownload()
        {
            this.mCurState = InnerState.DownloadAndApplyDiff;
            this.mCurDownloadTasks = diff;
            this.mCurrentDownloadIndex = 0;
            this.mCurFileDownLoadState = FileDownLoadState.StartDownLoad;

            var service = ServiceLocator.Current.GetInstance<IRemoteFileDownloadService>();
            service.SetDownloadCallBack(this.OnFileDownloaded);
        }

        private void GiveupDownload()
        {
            Context.ErrorType = AppUpdaterErrorType.UserGiveUpDownload;
            this.mCurState = InnerState.ResUpdateFailed;
        }

        private void ProcessDownloadAndApplyDiff()
        {
            Context.ProgressData.CurrentDownloadFileTotalTime += Time.unscaledDeltaTime;
            switch (this.mCurFileDownLoadState)
            {
                case FileDownLoadState.Idle:
                    this.ProcessFileIdle();
                    break;
                case FileDownLoadState.StartDownLoad:
                    this.ProcessFileStartDownLoad();
                    break;
                case FileDownLoadState.DownLoading:
                    this.ProcessDownLoading();
                    break;
                case FileDownLoadState.DownLoadCompleted:
                    this.ProcessDownLoadCompleted();
                    break;
                case FileDownLoadState.DownloadFail:
                    this.ProcessDownloadFail();
                    break;
                default: break;
            }

            this.UpdateDownloadInfos();
        }

        #region Download file from remote to local


        private void ProcessFileIdle()
        {
            
        }

        private void UpdateDownloadInfos()
        {
        }
        private void ProcessFileStartDownLoad()
        {
            var fileDesc = this.mCurDownloadTasks[this.mCurrentDownloadIndex];
            Context.ProgressData.CurrentDownloadingFileSize = (ulong)fileDesc.S;
           
            this.mCurFileDownLoadState = FileDownLoadState.DownLoading;

            var service = ServiceLocator.Current.GetInstance<IRemoteFileDownloadService>();
            service.StartDownload(fileDesc);
        }

        private void OnFileDownloaded(bool result)
        {
            if (!result)
            {
                Context.ErrorType = AppUpdaterErrorType.DownloadFileFailure;
                this.mCurState = InnerState.ResUpdateFailed;
                this.mCurFileDownLoadState = FileDownLoadState.DownloadFail;

                Logger.Error($"Download file that name is \"{this.mCurDownloadTasks[this.mCurrentDownloadIndex].N}\" failure! ");

                this.SaveCurrentConfig(false);
            }
            else
            {
                Context.ProgressData.CurrentDownloadSize += (ulong)this.mCurDownloadTasks[this.mCurrentDownloadIndex].S;
                Context.ProgressData.CurrentDownloadFileCount++;
                Context.ProgressData.Progress = Context.ProgressData.CurrentDownloadSize / ((float)Context.ProgressData.TotalDownloadSize);
                this.mLocalManifest.UpdateInnerFile(this.mCurDownloadTasks[this.mCurrentDownloadIndex]);

                //将之前下载的老资源添加到过期资源文件列表
                string localPathInMap = updateResMap.GetResLocalFileName(this.mCurDownloadTasks[this.mCurrentDownloadIndex].N);
                if (!string.IsNullOrEmpty(localPathInMap))
                {
                    string localPath = AppUpdaterContext.GetUpdateFileRelativeLocalPath(this.mCurDownloadTasks[this.mCurrentDownloadIndex]);
                    if (!string.Equals(localPathInMap, localPath))
                        Context.ExpireResFileList.Add(localPathInMap);
                }

                if (this.CheckDownloadOperationIsCompleted())
                {
                    Context.AppendInfo($"Current resource version update to \"{Context.GetCurrentRemoteMd5()}\".");
                    Logger.Info($"Update manifest that name is {Context.GetCurrentLocalVersionFileName()} is success!");
                    this.mCurFileDownLoadState = FileDownLoadState.Idle;
                    this.mCurState = InnerState.ResUpdateCompleted;
                }
                else
                {
                    Logger.Info($"Download file that name is {this.mCurDownloadTasks[this.mCurrentDownloadIndex].N} completed!");
                    this.mCurFileDownLoadState = FileDownLoadState.DownLoadCompleted;
                }
            }
        }

        private void ProcessDownLoading()
        {
            
        }


        private void ProcessDownLoadCompleted()
        {
            this.mCurrentDownloadIndex++;

            this.mCurFileDownLoadState = FileDownLoadState.StartDownLoad;
        }


        public bool CheckDownloadOperationIsCompleted()
        {
            return this.mCurrentDownloadIndex >= this.mCurDownloadTasks.Count - 1;
        }

        private void ProcessDownloadFail()
        {
            this.mCurFileDownLoadState = FileDownLoadState.Idle;
        }

        #endregion


        private void ProcessResUpdateCompleted()
        {
            this.SaveCurrentConfig();
            Logger.Info($"Update current resource completed , remote manifest name is \"{Context.GetCurrentLocalVersionFileName()}\" .");
            IRoutedEventArgs arg = new RoutedEventArgs()
            {
                EventType = (int)AppUpdaterInnerEventType.OnCurrentResUpdateCompleted
            };
            this.Target.HandleMessage(in arg);
        }

        public override bool OnMessage(AppUpdaterFsmOwner entity, in IRoutedEventArgs eventArgs)
        {
            var eventType = (AppUpdaterInnerEventType)eventArgs.EventType;

            switch (eventType)
            {
                case AppUpdaterInnerEventType.PerformResUpdateOperation:
                    this.Reset();
                    this.mCurState = InnerState.StartRequestResManifest;
                    return true;
                case AppUpdaterInnerEventType.OnApplicationFocus:
                    this.OnApplicationFocus(eventArgs);
                    return true;
                case AppUpdaterInnerEventType.OnApplicationQuit:
                    this.OnApplicationQuit();
                    return true;
                default:
                    break;
            }

            return base.OnMessage(entity, in eventArgs);
        }

        private void SaveCurrentConfig(bool updateVerisonNum = true)
        {
            string curManifestName = Context.GetCurrentLocalVersionFileName();
            var json = this.mCurManifestParser.Serialize(this.mLocalManifest);
            AppVersionManager.SaveToLocalDataResManifest(json, curManifestName);

            if (updateVerisonNum)
            {
                this.mCurManifestParser.WriteToAppInfo(Context.GetCurrentRemoteMd5(), Context.ResUpdateTarget.TargetResVersionNum);
            }
        }

        private void OnApplicationFocus(in IRoutedEventArgs eventArgs)
        {
            var hasFocus = ((RoutedEventArgs<bool>)eventArgs).arg;
            if (!hasFocus)
                SaveDownloadProgress();
        }

        private void OnApplicationQuit()
        {
            SaveDownloadProgress();
        }

        private void SaveDownloadProgress()
        {
            if (this.mLocalManifest != null)
                this.SaveCurrentConfig(false);
        }

        public override void Exit(AppUpdaterFsmOwner entity)
        {
            base.Exit(entity);
            Context.ProgressData.CurrentDownloadingFileSize = 0 ;
            this.Recycle();
        }

        private void Recycle()
        {
            List<FileDesc> recycleList = new List<FileDesc>();

            if (this.mCurRemoteManifest != null)
            {
                this.mCurRemoteManifest.Datas.ForEach(x => recycleList.Add(x));
                this.mCurRemoteManifest.Datas.Clear();
            }

            if (this.mLocalManifest != null)
            {
                this.mLocalManifest.Datas.ForEach(x =>
                {
                    if (!recycleList.Contains(x))
                        recycleList.Add(x);
                });
                this.mLocalManifest.Datas.Clear();
            }

            recycleList.ForEach(x=>VersionManifestParser.Pools.Recycle(x));
            this.mLocalManifest = null;
        }

        public override void Reset()
        {
            base.Reset();

            this.mCurState = InnerState.Idle;
            this.mCurFileServerType = FileServerType.CDN;
            this.mCurRemoteManifest = null;
            this.mCurFileDownLoadState = FileDownLoadState.Idle;
            this.mCurDownloadTasks.Clear();
            this.mCurrentDownloadIndex = 0;
            this.mCurManifestParser = null;

        }

        #endregion

    }
}
