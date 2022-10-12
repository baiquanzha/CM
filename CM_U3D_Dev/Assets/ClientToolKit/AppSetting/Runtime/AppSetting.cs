using UnityEngine;

namespace MTool.AppSetting.Runtime
{

    public class AppSetting : ScriptableObject
    {
        private static AppSetting mInstance = null;
        public static AppSetting Instance
        {
            get
            {
                if (mInstance == null)
                    mInstance = Resources.Load<AppSetting>("AppSetting");
                return mInstance;
            }
        }
    }
}