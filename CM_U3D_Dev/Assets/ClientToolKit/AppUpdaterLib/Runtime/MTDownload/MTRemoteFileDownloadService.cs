using System;
using System.Collections.Generic;
using MTool.AppUpdaterLib.Runtime.Download;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public class MTRemoteFileDownloadService : MonoBehaviour, IMTRemoteFileDownloadService, IDisposable
    {
        private MTFileDownloadCallback mOnFiledDownloaded;
        private FileStartDownload mOnFiledStartDownload;
        private List<FileDesc> mFileList;
        private int mDownloaded = 0;

        void Awake()
        {

        }

        void Update()
        {
            ThreadPool.Runtime.ThreadPool.Instance.Update();
        }

        public void SetDownloadCallBack(MTFileDownloadCallback callback)
        {
            mOnFiledDownloaded = callback;
        }

        public void SetStartCallback(FileStartDownload callback)
        {
            mOnFiledStartDownload = callback;
        }

        public void StartDownload(List<FileDesc> fileDescs)
        {
            mDownloaded = 0;
            mFileList = fileDescs;
            foreach (var file in fileDescs)
            {
                var task = new ResourceDownloadTask(file);
                task.SetDownloadCallback(OnFileDownloaded);
                task.SetStartCallback(OnFileDownloadStarted);
                ThreadPool.Runtime.ThreadPool.Instance.AddTask(task);
            }
            if (ThreadPool.Runtime.ThreadPool.Instance.IsClose())
                ThreadPool.Runtime.ThreadPool.Instance.ReStart();
        }

        public void Dispose()
        {
            
        }

        public bool IsDownloadCompleted()
        {
            return mDownloaded >= mFileList.Count;
        }

        public void ResetService()
        {
            mDownloaded = 0;
            mFileList?.Clear();
        }

        private void OnFileDownloaded(bool result, FileDesc handle)
        {
            if (!result)
            {
                mOnFiledDownloaded?.Invoke(false, handle);
                ThreadPool.Runtime.ThreadPool.Instance.Close();
            }
            else
            {
                mDownloaded++;
                mOnFiledDownloaded?.Invoke(true, handle);
                if (IsDownloadCompleted())
                    ThreadPool.Runtime.ThreadPool.Instance.Close();
            }
        }

        private void OnFileDownloadStarted(FileDesc handle)
        {
            mOnFiledStartDownload?.Invoke(handle);
        }
    }
}