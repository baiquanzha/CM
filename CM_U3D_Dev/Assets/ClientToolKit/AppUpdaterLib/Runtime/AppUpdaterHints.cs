using System;

namespace MTool.AppUpdaterLib.Runtime
{
    internal class AppUpdaterHints
    {
        private static AppUpdaterHints smInstance;
        public static AppUpdaterHints Instance
        {
            get
            {
                if (smInstance == null)
                {
                    smInstance = new AppUpdaterHints();
                }
                return smInstance;
            }
        }

        /// <summary>
        /// 保存lua到本地路径名小写（在可写空间后）
        /// </summary>
        public bool LowerLuaName = false;

        /// <summary>
        /// 手动执行app更新
        /// </summary>
        public bool ManualPerformAppUpdate = false;

        /// <summary>
        /// 允许本地资源号比远端更新
        /// </summary>
        public bool AllowResNumLocalNewerToRemote = true;

        /// <summary>
        /// 激活资源增量式更新
        /// </summary>
        public bool EnableResIncrementalUpdate = false;

        /// <summary>
        /// 激活Unity资源更新
        /// </summary>
        public bool EnableUnityResUpdate = true;

        /// <summary>
        /// 激活热更新资源完整检查
        /// </summary>
        public bool EnableCheckMissingRes = false;


        public void SetHintValue(AppUpdaterHintName hintName , int hintVal)
        {
            switch (hintName)
            {
                case AppUpdaterHintName.LOWER_LUA_NAME:
                    if (hintVal == (int)AppUpdaterBool.FALSE)
                    {
                        LowerLuaName = false;
                    }
                    else if (hintVal == (int)AppUpdaterBool.TRUE)
                    {
                        LowerLuaName = true;
                    }
                    else
                    {
                        throw new ArgumentException($"hintName : {hintName}  , hintVal : {hintVal} .");
                    }
                    break;
                case AppUpdaterHintName.MANUAL_PERFORM_APP_UPDATE:
                    if (hintVal == (int)AppUpdaterBool.FALSE)
                    {
                        ManualPerformAppUpdate = false;
                    }
                    else if (hintVal == (int)AppUpdaterBool.TRUE)
                    {
                        ManualPerformAppUpdate = true;
                    }
                    else
                    {
                        throw new ArgumentException($"hintName : {hintName}  , hintVal : {hintVal} .");
                    }
                    break;
                case AppUpdaterHintName.ENABLE_RES_INCREMENTAL_UPDATE:
                    if (hintVal == (int)AppUpdaterBool.FALSE)
                    {
                        EnableResIncrementalUpdate = false;
                    }
                    else if (hintVal == (int)AppUpdaterBool.TRUE)
                    {
                        EnableResIncrementalUpdate = true;
                    }
                    else
                    {
                        throw new ArgumentException($"hintName : {hintName}  , hintVal : {hintVal} .");
                    }
                    break;
                case AppUpdaterHintName.ENABLE_UNITY_RES_UPDATE:
                    if (hintVal == (int)AppUpdaterBool.FALSE)
                    {
                        EnableUnityResUpdate = false;
                    }
                    else if (hintVal == (int)AppUpdaterBool.TRUE)
                    {
                        EnableUnityResUpdate = true;
                    }
                    else
                    {
                        throw new ArgumentException($"hintName : {hintName}  , hintVal : {hintVal} .");
                    }
                    break;
                case AppUpdaterHintName.ENABLE_CHECK_MISSING_RES:
                    if (hintVal == (int)AppUpdaterBool.FALSE)
                    {
                        EnableCheckMissingRes = false;
                    }
                    else if (hintVal == (int)AppUpdaterBool.TRUE)
                    {
                        EnableCheckMissingRes = true;
                    }
                    else
                    {
                        throw new ArgumentException($"hintName : {hintName}  , hintVal : {hintVal} .");
                    }
                    break;
                default:
                    break;
            }

            
        }
    }
}