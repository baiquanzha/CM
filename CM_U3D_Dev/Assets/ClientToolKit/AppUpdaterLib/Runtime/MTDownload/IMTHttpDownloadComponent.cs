using System;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public interface IMTHttpDownloadComponent
    {
        void Update();

        void Download(string url, string filePath, int size, string md5 = null, Action<bool> downloadCompleted = null);

        float GetProgress();

        void SetTemporaryPath(string fileName);
    }
}