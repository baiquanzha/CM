// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.Download
{
    public interface IRemoteFileDownloadService
    {
        void SetDownloadCallBack(FileDownloadCallBack callback);

        void StartDownload(FileDesc fileDesc);
    }
}
