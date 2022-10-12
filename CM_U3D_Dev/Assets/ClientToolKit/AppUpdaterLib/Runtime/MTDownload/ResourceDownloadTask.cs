using System;
using MTool.AppUpdaterLib.Runtime.Download;
using MTool.ThreadPool.Runtime;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public class ResourceDownloadTask : ThreadTask
    {
        private FileDesc mFile = null;
        private MTFileDownloader mDownloader = null;
        private MTFileDownloadCallback mDownloadCallback;
        private FileStartDownload mStartCallback;

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("ResourceDownloadTask"));

        public ResourceDownloadTask(FileDesc file)
        {
            mFile = file;
            mDownloader = new MTFileDownloader(new MTHttpDownloadComponent());
            mDownloader.PreDownload(mFile);
            OnFinishProcess += OnDwonloadFinished;
            OnStartProcess += OnDownloadStarted;
        }

        public void SetDownloadCallback(MTFileDownloadCallback callback)
        {
            mDownloadCallback = callback;
        }

        public void SetStartCallback(FileStartDownload callback)
        {
            mStartCallback = callback;
        }

        public override void Process()
        {
            mDownloader.Download(mFile);
        }

        private void OnDwonloadFinished(ThreadTask task)
        {
            OnFinishProcess -= OnDwonloadFinished;
            mDownloadCallback?.Invoke(mDownloader.IsSucceed, mFile);
        }

        private void OnDownloadStarted(ThreadTask task)
        {
            OnStartProcess -= OnDownloadStarted;
            mStartCallback?.Invoke(mFile);
        }
    }
}