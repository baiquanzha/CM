using System;
using System.IO;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Download;
using MTool.Core.Utilities;
using MTool.LoggerModule.Runtime;
using UnityEngine;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public class MTFileDownloader
    {
        public enum InnerState
        {
            Idle,
            StartDownloadFromCDN,
            DownloadingFromCDN,
            StartDownloadFromOSS,
            DownloadingFromOSS,
            DownloadFailure,
            DownloadSuccess,
        }

        public static int MaxThreadCount = 8;

        private MTFileDownloadCallback mCallBack;
        private InnerState mState = InnerState.Idle;
        private FileDesc mCurDownloadInfo;

        private IMTHttpDownloadComponent mDownloadCore;
        private string mLocalPath = "";
        private string mFileName = "";

        public bool IsSucceed { get; private set; }

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("MTFileDownloader"));

        private static AppUpdaterConfig mUpdaterConfig = null;

        public MTFileDownloader(IMTHttpDownloadComponent downloadCore)
        {
            this.mDownloadCore = downloadCore;
            LoadConfig();
        }

        public void SetDownloadCallback(MTFileDownloadCallback callback)
        {
            this.mCallBack = callback;
        }

        public bool IsWorking()
        {
            return this.mState != InnerState.Idle;
        }

        public void PreDownload(FileDesc fileDesc)
        {
            mFileName = fileDesc.GetRNUTF8();
            Collect(fileDesc);
            string filePath = AppUpdaterContext.GetUpdateFileLocalPath(fileDesc);
            mLocalPath = filePath;
            mDownloadCore.SetTemporaryPath(Path.GetFileNameWithoutExtension(filePath));
            if (File.Exists(filePath))
            {
                var localMd5 = CryptoUtility.GetHash(filePath);
                if (string.Equals(localMd5, this.mCurDownloadInfo.H))
                {
                    s_mLogger.Value?.Info($"The file that path is \"{filePath}\" is already downloaded.");
                    this.mState = InnerState.DownloadSuccess;
                    return;
                }
            }
            this.mState = InnerState.StartDownloadFromCDN;
        }

        public void Download(FileDesc fileDesc)
        {
            do
            {
                Update();
                if (IsDone())
                {
                    Update();
                    break;
                }
            } while (mState != InnerState.DownloadFailure && mState != InnerState.DownloadSuccess);
        }

        public float GetProgress()
        {
            return mDownloadCore.GetProgress();
        }

        private void Update()
        {
            mDownloadCore.Update();
            switch (this.mState)
            {
                case InnerState.StartDownloadFromCDN:
                    this.OnStartDownloadFromCDN();
                    break;
                case InnerState.StartDownloadFromOSS:
                    this.OnStartDownloadFromOSS();
                    break;
                case InnerState.DownloadFailure:
                    this.OnDownloadFailure();
                    break;
                case InnerState.DownloadSuccess:
                    this.OnDownloadSuccess();
                    break;
                default:
                    break;
            }
        }

        private void OnStartDownloadFromCDN()
        {
            this.mState = InnerState.DownloadingFromCDN;
            this.StartDownloadInternal(FileServerType.CDN);
        }

        private void OnStartDownloadFromOSS()
        {
            s_mLogger.Value?.Debug("Start download file form oss .");
            this.mState = InnerState.DownloadingFromOSS;
            this.StartDownloadInternal(FileServerType.OSS);
        }

        private void OnDownloadFailure()
        {
            IsSucceed = false;
            try
            {
                this.mCallBack?.Invoke(false, mCurDownloadInfo);
            }
            finally
            {
                this.Clear();
            }
        }

        private void OnDownloadSuccess()
        {
            IsSucceed = true;
            try
            {
                this.mCallBack?.Invoke(true, mCurDownloadInfo);
            }
            finally
            {
                this.Clear();
            }
        }

        private string GetRemoteResFileUrl(string fileName, FileServerType fileServerType = FileServerType.CDN)
        {
            string serverUrl = fileServerType == FileServerType.CDN ? AppUpdaterConfig.cdnUrl : AppUpdaterConfig.ossUrl;
            string url = $"{serverUrl}/{fileName}";
            return url;
        }

        private void StartDownloadInternal(FileServerType serverType)
        {
            var url = GetRemoteResFileUrl(this.mFileName, serverType);
            this.mDownloadCore.Download(url, mLocalPath, mCurDownloadInfo.S, mCurDownloadInfo.H, this.OnDownloadCompleted);
        }

        private void OnDownloadCompleted(bool result)
        {
            if (!result && this.mState == InnerState.DownloadingFromCDN)
                this.mState = InnerState.StartDownloadFromOSS;
            else
                this.mState = result ? InnerState.DownloadSuccess : InnerState.DownloadFailure;
        }

        private void Collect(FileDesc fileDesc)
        {
            this.mCurDownloadInfo = fileDesc;
        }

        private void Clear()
        {
            this.mCurDownloadInfo = null;
        }

        private bool IsDone()
        {
            return mState == InnerState.DownloadFailure || mState == InnerState.DownloadSuccess;
        }

        private static void LoadConfig()
        {
            if (mUpdaterConfig == null)
            {
                var appUpdaterConfigText = Resources.Load<TextAsset>("appupdater");
                mUpdaterConfig = JsonUtility.FromJson<AppUpdaterConfig>(appUpdaterConfigText.text);
            }
        }

        public static AppUpdaterConfig AppUpdaterConfig
        {
            get
            {
                LoadConfig();
                return mUpdaterConfig;
            }
        }
    }
}