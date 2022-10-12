using System.Collections.Generic;
using MTool.AppUpdaterLib.Runtime.Download;

namespace MTool.AppUpdaterLib.Runtime.MTDownload
{
    public interface IMTRemoteFileDownloadService
    {
        void SetDownloadCallBack(MTFileDownloadCallback callback);

        void SetStartCallback(FileStartDownload callback);

        void StartDownload(List<FileDesc> fileDescs);

        void ResetService();

        bool IsDownloadCompleted();
    }
}