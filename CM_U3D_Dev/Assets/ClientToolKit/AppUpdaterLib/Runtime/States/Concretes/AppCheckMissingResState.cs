using System.Text;
using System.IO;
using System.Collections.Generic;
using CommonServiceLocator;
using MTool.Core.Utilities;
using MTool.AppUpdaterLib.Runtime.Download;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using MTool.AppUpdaterLib.Runtime.MTDownload;
using MTool.Core.Functional;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal sealed class AppCheckMissingResState : BaseAppUpdaterFunctionalState
    {
        private enum InnerState
        {
            Idle,
            LoadResManifest,
            LoadingResManifest,
            LoadingDataManifest,
            CalculateMissingFiles,
            DownloadMissingFiles,
            CheckComplete,
            CheckFailure,
        }

        private InnerState mState = InnerState.Idle;
        private VersionManifest builtInResManifest;
        private VersionManifest localResManifest;
        private VersionManifest builtInDataManifest;
        private VersionManifest localDataManifest;
        private List<FileDesc> missingFiles = new List<FileDesc>();
        private IMTRemoteFileDownloadService mMTDownloadService;
        private IRemoteFileDownloadService mDownloadService;

        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            base.Enter(entity, args);
            mState = InnerState.LoadResManifest;
        }

        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);
            switch (mState)
            {
                case InnerState.LoadResManifest:
                    LoadResManifest();
                    break;
                case InnerState.CalculateMissingFiles:
                    CalculateMissingFiles();
                    break;
                case InnerState.CheckComplete:
                    ProcessCheckComplete();
                    break;
                case InnerState.CheckFailure:
                    ProcessCheckFailure();
                    break;
            }
        }

        private void LoadResManifest()
        {
            string localPath = AssetsFileSystem.GetWritePath(AssetsFileSystem.UnityResManifestName);
            if (File.Exists(localPath))
            {
                mState = InnerState.LoadingResManifest;
                var content = File.ReadAllText(localPath);
                localResManifest = VersionManifestParser.Parse(content);
                string builtinPath = AssetsFileSystem.GetStreamingAssetsPath(AssetsFileSystem.UnityResManifestName, null, false);
                this.Target.Request.Load(builtinPath, LoadResManifestCallback);
            }
            else
                mState = InnerState.CalculateMissingFiles;
        }

        private void LoadResManifestCallback(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                mState = InnerState.CalculateMissingFiles;
            }
            else
            {
                var content = new UTF8Encoding(false, true).GetString(bytes);
                builtInResManifest = VersionManifestParser.Parse(content);
                mState = InnerState.CalculateMissingFiles;
            }
        }

        private bool CheckFileMissing(FileDesc desc)
        {
            string path = AppUpdaterContext.GetUpdateFileLocalPath(desc);
            if (File.Exists(path))
            {
                var localMd5 = CryptoUtility.GetHash(path);
                if (string.Equals(localMd5, desc.H))
                    return false;
                else
                    return true;
            }
            else
                return true;
        }

        private string GetResFilePath(FileDesc desc)
        {
            string relativePath;
            string unityResPrefix = AppUpdaterConfigManager.AppUpdaterConfig.remoteRoot + "/resource/";

            if (desc.RN.StartsWith(unityResPrefix))
            {
                relativePath = desc.N;
            }
            else
            {
                relativePath = $"lua/gen/{desc.N}";
            }

            return AssetsFileSystem.GetWritePath(relativePath);
        }

        private void CalculateMissingFiles()
        {
            missingFiles.Clear();
            if (builtInResManifest != null && localResManifest != null)
            {
                var resList = builtInResManifest.CalculateDifference(localResManifest);
                if (resList != null)
                {
                    foreach (FileDesc desc in resList)
                    {
                        if (CheckFileMissing(desc))
                            missingFiles.Add(desc);
                    }
                }
            }
            if (builtInDataManifest != null && localDataManifest != null)
            {
                var dataList = builtInDataManifest.CalculateDifference(localDataManifest);
                if (dataList != null)
                {
                    foreach (FileDesc desc in dataList)
                    {
                        if (CheckFileMissing(desc))
                            missingFiles.Add(desc);
                    }
                }
            }
            if (missingFiles.Count > 0)
            {
                Context.ProgressData.Clear();
                ulong totalSize = 0;
                missingFiles.ForeachCall(x => totalSize += (ulong)x.S);
                Context.ProgressData.TotalDownloadSize += totalSize;
                Context.ProgressData.TotalDownloadFileCount += (ulong)missingFiles.Count;
                Logger.Info($"Needed to download {missingFiles.Count} files , total size is {totalSize}B .");
                Context.FetchDeviceStorageInfo();
                if (Context.DiskInfo.IsGetReady && Context.DiskInfo.BusySpace + CommonConst.MIN_DISK_AVAILABLE_SPACE > Context.DiskInfo.TotalSpace)
                {
                    Context.ErrorType = AppUpdaterErrorType.DiskIsNotEnoughToDownPatchFiles;
                    mState = InnerState.CheckFailure;
                }
                else
                {
                    mState = InnerState.DownloadMissingFiles;
                    if (Context.DownloadMode == AppUpdateDownloadMode.MultiThread)
                    {
                        mMTDownloadService = ServiceLocator.Current.GetInstance<IMTRemoteFileDownloadService>();
                        mMTDownloadService.SetDownloadCallBack(OnFileDownloaded);
                        mMTDownloadService.StartDownload(missingFiles);
                    }
                    else
                    {
                        mDownloadService = ServiceLocator.Current.GetInstance<IRemoteFileDownloadService>();
                        mDownloadService.SetDownloadCallBack(OnFileDownloaded);
                        var fileDesc = missingFiles[0];
                        mDownloadService.StartDownload(fileDesc);
                    }
                    this.Target.OnStartDownloadMissingResCallback(Context.ProgressData.TotalDownloadSize, Context.ProgressData.TotalDownloadFileCount);
                }
            }
            else
            {
                Logger.Info($"Don't need download missing resource files.");
                mState = InnerState.CheckComplete;
            }
        }

        private void OnFileDownloaded(bool result, FileDesc fileDesc)
        {
            if (!result)
            {
                Context.ErrorType = AppUpdaterErrorType.DownloadFileFailure;
                mState = InnerState.CheckFailure;
                Logger.Error($"Download file that name is \"{fileDesc.N}\" failure! ");
            }
            else
            {
                Logger.Info($"Download file that name is {fileDesc.N} completed!");
                Context.ProgressData.CurrentDownloadSize += (ulong)fileDesc.S;
                Context.ProgressData.CurrentDownloadFileCount++;
                Context.ProgressData.Progress = Context.ProgressData.CurrentDownloadSize / ((float)Context.ProgressData.TotalDownloadSize);
                if (mMTDownloadService.IsDownloadCompleted())
                {
                    mState = InnerState.CheckComplete;
                    this.Target.OnDownloadMissingResCompleteCallback();
                }
            }
        }
        
        private void OnFileDownloaded(bool result)
        {
            int DownloadIndex = (int)Context.ProgressData.CurrentDownloadFileCount;
            var fileDesc = missingFiles[DownloadIndex];
            if (!result)
            {
                Context.ErrorType = AppUpdaterErrorType.DownloadFileFailure;
                mState = InnerState.CheckFailure;
                Logger.Error($"Download file that name is \"{fileDesc.N}\" failure! ");
            }
            else
            {
                Logger.Info($"Download file that name is {fileDesc.N} completed!");
                Context.ProgressData.CurrentDownloadSize += (ulong)fileDesc.S;
                Context.ProgressData.CurrentDownloadFileCount++;
                Context.ProgressData.Progress = Context.ProgressData.CurrentDownloadSize / ((float)Context.ProgressData.TotalDownloadSize);
                if (Context.ProgressData.CurrentDownloadFileCount >= Context.ProgressData.TotalDownloadFileCount)
                {
                    mState = InnerState.CheckComplete;
                    this.Target.OnDownloadMissingResCompleteCallback();
                }
                else
                {
                    int index = (int)Context.ProgressData.CurrentDownloadFileCount;
                    var downloadFile = missingFiles[index];
                    mDownloadService.StartDownload(downloadFile);
                }
            }
        }

        private void ProcessCheckComplete()
        {
            this.Target.ChangeState<AppUpdateCompletedState>();
        }

        private void ProcessCheckFailure()
        {
            this.Target.ChangeState<AppUpdateFailureState>();
        }

        public override void Reset()
        {
            base.Reset();
            mState = InnerState.Idle;
            missingFiles.Clear();
            builtInResManifest = null;
            localResManifest = null;
            builtInDataManifest = null;
            localDataManifest = null;
            mDownloadService = null;
            mMTDownloadService = null;
        }
    }
}