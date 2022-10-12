using UnityEngine;
using MTool.AppUpdaterLib.Runtime.Interfaces;

namespace MTool.AppUpdaterLib.Runtime.Configs.Loader
{
    public class AppUpdaterConfigLoader : IAppUpdaterConfigLoader
    {
        public AppUpdaterConfig Load()
        {
            var appUpdaterConfigText = Resources.Load<TextAsset>("appupdater");
            AppUpdaterConfig config = JsonUtility.FromJson<AppUpdaterConfig>(appUpdaterConfigText.text);
            return config;
        }
    }
}