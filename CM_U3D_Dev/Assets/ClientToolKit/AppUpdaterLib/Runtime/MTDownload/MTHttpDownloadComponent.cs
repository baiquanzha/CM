using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using MTool.Core.Utilities;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    // A multithreaded http download component on Single File
    public class MTHttpDownloadComponent : IMTHttpDownloadComponent
    {
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

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("MTHttpDownloadComponent"));

        public const int MAX_RETRY_COUNT = 3;
        private DownloadState mState = DownloadState.Idle;
        private string mUrl;
        private string mFilePath;
        private string mTemporyPath;
        private int mSize = 0;

        private readonly float mRetryInterval = 3;
        private DateTime mRetryStartDateTime;
        private int mRetryCount;
        private string mMd5;
        private Action<bool> mDownloadCompletedCallBack;

        private Thread mMainThread = null;
        private List<Thread> mSegmentThreads = new List<Thread>();
        private List<FileSegment> mSegments = new List<FileSegment>();
        private readonly Exception mLastError;

        public void Update()
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
                this.mRetryStartDateTime = DateTime.Now;
                this.mRetryCount++;
                this.mState = DownloadState.WaitToRetry;
                s_mLogger.Value?.Debug($"Download the file that url is \"{this.mUrl}\" after {this.mRetryInterval} seconds .");
            }
        }

        private void OnWaitToRetry()
        {
            var Interval = DateTime.Now - this.mRetryStartDateTime;
            if (this.mRetryCount > 0 && Interval.TotalSeconds > this.mRetryInterval)
            {
                s_mLogger.Value?.Debug($"Start retry download file , retry time : {this.mRetryCount} .");
                this.mRetryStartDateTime = DateTime.MinValue;
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
            DownloadInternal();
        }

        private void DownloadInternal()
        {
            s_mLogger.Value?.Debug($"Downloading {this.mUrl}");
            mMainThread = new Thread(new ThreadStart(OnDownloadThreadStarted));
            mMainThread.Start();
        }

        public bool IsCanWorking()
        {
            return this.mState == DownloadState.Idle;
        }

        public void Download(string url, string filePath, int size, string md5 = null, Action<bool> downloadCompleted = null)
        {
            if (!IsCanWorking())
                return;

            Clear();
            Collect(url, filePath, size, md5, downloadCompleted);
            StartDownload();
        }

        public void SetTemporaryPath(string fileName)
        {
            mTemporyPath = AssetsFileSystem.GetWritePath($"g_caches/{fileName}_tmp");
        }

        private void Collect(string url, string filePath, int size, string md5, Action<bool> downloadCompleted)
        {
            mUrl = url;
            mFilePath = filePath;
            mSize = size;
            var dirPath = Path.GetDirectoryName(this.mTemporyPath);
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath ?? throw new InvalidOperationException());

            mMd5 = md5;
            mDownloadCompletedCallBack = downloadCompleted;
        }

        private void Clear()
        {
            mRetryStartDateTime = DateTime.MinValue;
            mRetryCount = 0;
            mState = DownloadState.Idle;
            mFilePath = null;
            //mTemporyPath = null;
            mUrl = null;
            mMd5 = null;
            mDownloadCompletedCallBack = null;
        }

        public float GetProgress()
        {
            int count = mSegments.Count;
            if (count > 0)
            {
                float progress = 0;
                for (int i = 0; i < count; i++)
                {
                    progress += mSegments[i].Progress;
                }
                return progress / count;
            }

            return 0;
        }

        private void OnDownloadThreadStarted()
        {
            Stream inputStream = null;
            RemoteFileInfo remoteFileInfo = new RemoteFileInfo();
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.mUrl);
                request.Timeout = 30000;
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                remoteFileInfo.MimeType = response.ContentType;
                remoteFileInfo.LastModified = response.LastModified;
                remoteFileInfo.FileSize = response.ContentLength;
                remoteFileInfo.AcceptRanges = string.Compare(response.Headers["Accept-Ranges"], "bytes", StringComparison.OrdinalIgnoreCase) == 0;

                inputStream = response.GetResponseStream();
            }
            catch (Exception e)
            {
                // Download failed
                s_mLogger.Value.Warn($"Download failed. exception:{e.Message}");
                this.mState = DownloadState.DownloadAgain;
                return;
            }

            Segment[] segments = SegmentCalculator.GetSegments(MTFileDownloader.MaxThreadCount, mSize);
            lock (mSegmentThreads) mSegmentThreads.Clear();
            lock (mSegments) mSegments.Clear();
            for (int i = 0; i < segments.Length; i++)
            {
                FileSegment segment = new FileSegment(i, segments[i].StartPosition, segments[i].EndPosition);
                if (i == 0)
                    segment.InputStream = inputStream;
                mSegments.Add(segment);
            }
            
            using (var outputStream = new FileStream(this.mTemporyPath, FileMode.OpenOrCreate, FileAccess.Write))
            {
                foreach (var segment in mSegments)
                {
                    segment.OutputStream = outputStream;
                    StartDownloadSegement(segment);
                }

                do
                {
                    while (!AllSegmentThreadDone(1000)) ;
                }
                while (RestartFailedSegments());

                outputStream.Close();
                outputStream.Dispose();
            }

            foreach (var segment in mSegments)
            {
                if (segment.State == SegmentState.Error)
                {
                    // Download Failed
                    this.mState = DownloadState.DownloadAgain;
                    return;
                }
            }

            // Download End
            if (!string.IsNullOrEmpty(mMd5)) // Need check file md5
            {
                var tempFileMd5 = CryptoUtility.GetHash(this.mTemporyPath);

                if (string.Equals(this.mMd5, tempFileMd5, StringComparison.OrdinalIgnoreCase))
                {
                    s_mLogger.Value?.Debug($"Verify file that was downloaded successful! MD5 : {tempFileMd5}");
                    if (File.Exists(this.mFilePath))
                        File.Delete(this.mFilePath);

                    var dirName = Path.GetDirectoryName(this.mFilePath);
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);

                    File.Move(this.mTemporyPath, this.mFilePath);
                    s_mLogger.Value?.Info($"Move File From {this.mTemporyPath} to {this.mFilePath}");
                    this.mState = DownloadState.DownloadSuccess;
                    s_mLogger.Value?.Debug($"Save file to local. Path is \"{this.mFilePath}\" . TemporyPath:{this.mTemporyPath}");
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
                this.mState = DownloadState.DownloadSuccess;
                s_mLogger.Value?.Debug("Download file success.");
            }
        }

        private void StartDownloadSegement(FileSegment segment)
        {
            Thread segmentThread = new Thread(OnSegmentThreadStarted);
            segmentThread.Start(segment);

            lock (mSegmentThreads)
            {
                mSegmentThreads.Add(segmentThread);
            }
        }

        private bool RestartFailedSegments()
        {
            bool hasErrors = false;
            double delay = 0;
            foreach (var segment in mSegments)
            {
                if (segment.State == SegmentState.Error && segment.LastErrorTime != DateTime.MinValue && segment.CanRetry)
                {
                    hasErrors = true;
                    TimeSpan ts = DateTime.Now - segment.LastErrorTime;

                    if (ts.TotalSeconds >= FileSegment.RetryDelay)
                    {
                        segment.RetriedCount++;
                        StartDownloadSegement(segment);
                    }
                    else
                    {
                        delay = Math.Max(delay, FileSegment.RetryDelay * 1000 - ts.TotalMilliseconds);
                    }
                }
            }

            Thread.Sleep((int)delay);

            return hasErrors;
        }

        private bool AllSegmentThreadDone(int timeout)
        {
            bool allFinished = true;

            Thread[] workers;
            lock (mSegmentThreads)
            {
                workers = mSegmentThreads.ToArray();
            }

            foreach (Thread t in workers)
            {
                bool finished = t.Join(timeout);
                allFinished = allFinished & finished;

                if (finished)
                {
                    lock (mSegmentThreads)
                    {
                        mSegmentThreads.Remove(t);
                    }
                }
            }

            return allFinished;
        }

        private void OnSegmentThreadStarted(object threadParam)
        {
            FileSegment segment = (FileSegment)threadParam;
            segment.LastError = null;
            try
            {
                if (segment.EndPos > 0 && segment.StartPos >= segment.EndPos)
                {
                    segment.SetState(SegmentState.Finished);
                    return;
                }

                segment.SetState(SegmentState.Connecting);
                if (segment.InputStream == null)
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.mUrl);
                    request.Timeout = 30000;
                    if (segment.StartPos != 0)
                    {
                        if (segment.EndPos == 0)
                            request.AddRange((int)segment.StartPos);
                        else
                            request.AddRange((int)segment.StartPos, (int)segment.EndPos);
                    }

                    segment.InputStream = request.GetResponse().GetResponseStream();
                }

                int buffSize = 8192;
                byte[] buffer = new byte[buffSize];
                using (segment.InputStream)
                {
                    segment.SetState(SegmentState.Downloading);
                    long readSize = 0;
                    do
                    {
                        // reads the buffer from input stream
                        readSize = segment.InputStream.Read(buffer, 0, buffSize);

                        // check if the segment has reached the end
                        if (segment.EndPos > 0 && segment.StartPos + readSize > segment.EndPos)
                        {
                            // adjust the 'readSize' to write only necessary bytes
                            readSize = (segment.EndPos - segment.StartPos);
                            if (readSize <= 0)
                            {
                                segment.StartPos = segment.EndPos;
                                break;
                            }
                        }

                        // locks the stream to avoid that other threads changes
                        // the position of stream while this thread is writing into the stream
                        lock (segment.OutputStream)
                        {
                            segment.OutputStream.Position = segment.StartPos;
                            segment.OutputStream.Write(buffer, 0, (int)readSize);
                        }

                        // increse the start position of the segment and also calculates the rate
                        segment.IncreaseStartPos(readSize);

                        // check if the stream has reached its end
                        if (segment.EndPos > 0 && segment.StartPos >= segment.EndPos)
                        {
                            segment.StartPos = segment.EndPos;
                            break;
                        }
                    } while (readSize > 0);

                    segment.SetState(SegmentState.Finished);

                }
            }
            catch (Exception e)
            {
                // store the error information
                segment.SetState(SegmentState.Error);
                segment.LastError = e;
                s_mLogger.Value.Warn(e.ToString());
            }
            finally
            {
                segment.InputStream = null;
            }
        }
    }
}