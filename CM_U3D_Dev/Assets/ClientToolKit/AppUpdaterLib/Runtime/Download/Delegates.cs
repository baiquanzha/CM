// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.Download
{
    public delegate void FileDownloadCallBack(bool result);
    public delegate void FileStartDownload(FileDesc handle);
    public delegate void MTFileDownloadCallback(bool result, FileDesc handle);
}
