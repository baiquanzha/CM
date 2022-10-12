using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using MTool.Core.Utilities;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.Download
{
    public class HttpDownloadComponent : MonoBehaviour
    {
        //--------------------------------------------------------------
        #region Inner Class & Enum ...
        //--------------------------------------------------------------

        public enum DownloadState
        {
            Idle,

            StartDownload,

            Downloading,
            
            DownloadAgain,

            WaitToRetry,

            DownloadSuccess,

            DownloadFailure,
        }

        #endregion

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("HttpDownloadComponent"));

        public const int MAX_RETRY_COUNT = 3;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public float Progress => this.mProgress;

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        private void Update()
        {
            this.UpdateState();
            this.UpdateDownload();
        }

        private void UpdateState()
        {
            switch (this.mState)
            {
                case DownloadState.StartDownload:
                    this.OnStartDownload();
                    break;
                case DownloadState.DownloadAgain:
                    this.OnDownloadAgain();
                    break;
                case DownloadState.WaitToRetry:
                    this.OnWaitToRetry();
                    break;
                case DownloadState.DownloadFailure:
                    this.OnDownloadFailure();
                    break;
                case DownloadState.DownloadSuccess:
                    this.OnDownloadSuccess();
                    break;
                default:
                    break;

            }
        }

        private void UpdateDownload()
        {
        }

        private void OnStartDownload()
        {
            this.StartDownload();
        }

        private void OnDownloadAgain()
        {
            if (this.mRetryCount >= MAX_RETRY_COUNT)
            {
                this.mState = DownloadState.DownloadFailure;
            }
            else
            {
                this.mRetryStartTime = Time.realtimeSinceStartup;
                this.mRetryCount++;
                this.mState = DownloadState.WaitToRetry;
                s_mLogger.Value?.Debug($"Download the file that url is \"{this.mUrl}\" after {this.mRetryInterval} seconds .");
            }
        }

        private void OnWaitToRetry()
        {
            if (this.mRetryCount > 0 && (Time.realtimeSinceStartup - this.mRetryStartTime) > this.mRetryInterval)
            {
                s_mLogger.Value?.Debug($"Start retry download file , retry time : {this.mRetryCount} .");
                this.mRetryStartTime = 0;
                this.StartDownload();
            }
        }

        private void OnDownloadFailure()
        {
            try
            {
                this.mDownloadCompletedCallBack?.Invoke(false);
            }
            finally
            {
                this.Clear();
            }
        }

        private void OnDownloadSuccess()
        {
            try
            {
                this.mDownloadCompletedCallBack?.Invoke(true);
            }
            finally
            {
                this.Clear();
            }
        }

        private void StartDownload()
        {
            this.mState = DownloadState.Downloading;
            this.StartCoroutine(DownloadInternal());
        }

        private IEnumerator DownloadInternal()
        {
            s_mLogger.Value?.Debug($"Downloading {this.mUrl}");
            var headRequest = UnityWebRequest.Head(this.mUrl);

            yield return headRequest.SendWebRequest();

            if (headRequest.isNetworkError || headRequest.isHttpError)
            {
                s_mLogger.Value?.Error($"Network error, error message : {headRequest.error} . ResponseCode : {headRequest.responseCode}");
                this.mState = DownloadState.DownloadAgain;
            }
            else
            {
                var totalLength = long.Parse(headRequest.GetResponseHeader("Content-Length"));
               
                using (var fs = new FileStream(this.mTemporyPath, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    var request = UnityWebRequest.Get(this.mUrl);
                    var fileLength = fs.Length;
                    if (fileLength > 0)
                    {
                        s_mLogger.Value?.Debug($"Resume an interrupted file download , original file path is \"{this.mFilePath}\".");
                        request.SetRequestHeader("Range", "bytes=" + fileLength + "-" + totalLength);
                        fs.Seek(fileLength, SeekOrigin.Begin);
                    }

                    if (fileLength < totalLength)
                    {   
                        request.SendWebRequest();

                        var index = 0;
                        while (true)
                        {
                            yield return null;
                            var buff = request.downloadHandler.data;
                            if (buff != null)
                            {
                                var length = buff.Length - index;
                                if (length > 0)
                                {
                                    fs.Write(buff, index, length);
                                    index += length;
                                    fileLength += length;
                                }
                                
                                if (fileLength == totalLength)
                                {
                                    this.mProgress = 1f;
                                }
                                else
                                {
                                    this.mProgress = fileLength / (float)totalLength;
                                }
                            }

                            bool networkError = request.isNetworkError || request.isHttpError;
                            if (networkError)
                            {
                                break;
                            }

                            if (request.isDone && (fileLength == totalLength))
                            {
                                break;
                            }
                        }
                    }

                    fs.Close();
                    fs.Dispose();

                    if (request.isNetworkError || request.isHttpError)
                    {
                        s_mLogger.Value?.Fatal($"Network error , error message : {request.error} . ResponseCode : {request.responseCode}");
                        this.mState = DownloadState.DownloadAgain;
                    }
                    else if(fileLength != totalLength)
                    {
                        s_mLogger.Value?.Fatal($"The size of file that downloaded is not equal to the target file ,  " +
                                               $"the downloaded file size is {fileLength}  , taget size is {totalLength} .");
                        File.Delete(this.mTemporyPath);
                        this.mState = DownloadState.DownloadAgain;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(this.mMd5))//Need check file md5
                        {
                            var tempFileMd5 = CryptoUtility.GetHash(this.mTemporyPath);
                            
                            if (string.Equals(this.mMd5,tempFileMd5,StringComparison.OrdinalIgnoreCase))
                            {
                                s_mLogger.Value?.Debug($"Verify file that was downloaded successful! MD5 : {tempFileMd5}");
                                if (File.Exists(this.mFilePath))
                                {
                                    File.Delete(this.mFilePath);
                                }
                                var dirName = Path.GetDirectoryName(this.mFilePath);
                                if (!Directory.Exists(dirName))
                                {
                                    Directory.CreateDirectory(dirName);
                                }
                                File.Move(this.mTemporyPath,this.mFilePath);
                                this.mProgress = 1f;
                                this.mState = DownloadState.DownloadSuccess;
                                s_mLogger.Value?.Debug($"Save file to local. Path is \"{this.mFilePath}\" .");
                            }
                            else
                            {
                                s_mLogger.Value?.Debug($"Target md5 : {this.mMd5} , temporary file md5 : {tempFileMd5} .");
                                File.Delete(this.mTemporyPath);
                                this.mState = DownloadState.DownloadAgain;
                                s_mLogger.Value?.Debug($"Download failure , delete it and retry download. Original file path is \"{this.mFilePath}\" .");
                            }
                        }
                        else
                        {
                            this.mProgress = 1f;
                            this.mState = DownloadState.DownloadSuccess;
                            s_mLogger.Value?.Debug("Download file success.");
                        }
                    }

                    request.Dispose();
                }

                headRequest.Dispose();
            }
        }


        public bool IsCanWorking()
        {
            return this.mState == DownloadState.Idle;
        }

        public void Download(string url , string filePath ,string md5 = null, Action<bool> downloadCompleted = null)
        {
            if (!IsCanWorking())
            {
                return;
            }

            this.Clear();

            this.Collect(url,filePath,md5, downloadCompleted);

            this.StartDownload();
        }

        private void Collect(string url, string filePath , string md5 , Action<bool> downloadCompleted)
        {
            this.mUrl = url;
            this.mFilePath = filePath;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            this.mTemporyPath = AssetsFileSystem.GetWritePath($"g_caches/{fileName}_tmp");
            var dirPath = Path.GetDirectoryName(this.mTemporyPath);
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath ?? throw new InvalidOperationException());
            }

            this.mMd5 = md5;
            this.mDownloadCompletedCallBack = downloadCompleted;
        }

        private DownloadState mState = DownloadState.Idle;
        private string mUrl;
        private string mFilePath;
        private string mTemporyPath;

        private float mRetryInterval = 3;
        private float mRetryStartTime = 0;
        private int mRetryCount;
        private string mMd5;
        private Action<bool> mDownloadCompletedCallBack;
        private float mProgress;
        private void Clear()
        {
            this.mProgress = 0;
            this.mRetryStartTime = 0;
            this.mRetryCount = 0;
            this.mState = DownloadState.Idle;
            this.mFilePath = null;
            this.mTemporyPath = null;
            this.mUrl = null;
            this.mMd5 = null;
            this.mDownloadCompletedCallBack = null;
        }
        #endregion

    }
}
