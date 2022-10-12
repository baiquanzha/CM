using System;
using System.Security;

namespace MTool.AppUpdaterLib.Runtime.Manifests
{
    [Serializable]
    public class AppInfoManifest
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// 当前app的Version
        /// </summary>
        public string version = "";

        /// <summary>
        /// 表数据版本
        /// </summary>
        public string dataResVersion;

        /// <summary>
        /// unity资源版本
        /// </summary>
        public string unityDataResVersion;

        /// <summary>
        /// 目标平台
        /// </summary>
        public string TargetPlatform;

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        public AppInfoManifest()
        {
            
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        #endregion

    }
}
