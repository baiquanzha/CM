using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MTool.AppUpdaterLib.Runtime
{
    public class AppUpdaterVersion
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// 当前程序集版本
        /// </summary>
        public const string AssemblyVersion = "0.0.1";

        /// <summary>
        /// 版本后缀信息
        /// </summary>
        public const string VersionSuffix = "PreAlpha";

        public static readonly string EngineVersion = "";

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        static AppUpdaterVersion()
        {
            EngineVersion = Application.unityVersion;
        }

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------


        public static string GetVersionInfo()
        {
            return $"{AssemblyVersion}-{VersionSuffix}-{EngineVersion}";
        }

        #endregion

    }
}
