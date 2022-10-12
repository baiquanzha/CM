namespace MTool.AppUpdaterLib.Runtime.Interfaces
{
    public interface IStorageInfoProvider
    {
        int GetAvailableDiskSpace();

        int GetTotalDiskSpace();

        int GetBusyDiskSpace();
    }
}
