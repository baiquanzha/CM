using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.AppUpdaterLib.Runtime.Interfaces;
using MTool.AppUpdaterLib.Runtime.Configs.Loader;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.Managers
{
    public class AppUpdaterConfigManager
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// 默认Loader
        /// </summary>
        private static IAppUpdaterConfigLoader defaultLoader = new AppUpdaterConfigLoader();

        /// <summary>
        /// 自定义Loader
        /// </summary>
        private static IAppUpdaterConfigLoader customLoader;
        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private static AppUpdaterConfig mAppUpdaterConfig;

        public static AppUpdaterConfig AppUpdaterConfig
        {
            get
            {
                if (mAppUpdaterConfig == null)
                {
                    if (customLoader == null)
                    {
                        mAppUpdaterConfig = defaultLoader.Load();
                    }
                    else
                    {
                        mAppUpdaterConfig = customLoader.Load();
                    }
                }

                return mAppUpdaterConfig;
            }
        }
        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        /// <summary>
        /// 设置自定义Loader
        /// </summary>
        /// <param name="_customLoader"></param>
        public static void SetCustomLoader(IAppUpdaterConfigLoader _customLoader)
        {
            customLoader = _customLoader;
        }
        #endregion

    }
}
