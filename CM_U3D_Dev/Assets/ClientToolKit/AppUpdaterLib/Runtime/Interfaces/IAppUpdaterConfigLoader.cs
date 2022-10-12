using MTool.AppUpdaterLib.Runtime.Configs;

namespace MTool.AppUpdaterLib.Runtime.Interfaces
{
    public interface IAppUpdaterConfigLoader
    {
        AppUpdaterConfig Load();
    }
}