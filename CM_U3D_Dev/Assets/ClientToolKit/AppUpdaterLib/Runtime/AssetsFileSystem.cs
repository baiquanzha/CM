using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MTool.AppUpdaterLib.Runtime
{
    public class AssetsFileSystem
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static StringBuilder TmpSB = new StringBuilder();

        private static readonly Dictionary<string, ABTableItemInfoClient> configList = new Dictionary<string, ABTableItemInfoClient>(StringComparer.OrdinalIgnoreCase);

        public const string AppInfoFileName = "app_info.x";

        public const string UnityABFileName = "file_list.x";
        public const string UpdateResMap = "update_res_map.x";
        public const string RemoteUnityResManifestPattern = "res_{0}-{1}.json";

        public const string UnityResManifestNamePattern = "res_{0}.json";
        public static string UnityResManifestName => string.Format(UnityResManifestNamePattern, GetPlatformStringForConfig());

        public static readonly string RootFolderName = "assets";
        private static string s_mRootFolder = string.Empty;
        public static string RootFolder
        {
            get
            {
                if(string.IsNullOrEmpty(s_mRootFolder))
                    s_mRootFolder = $"{PersistentDataPath}/{RootFolderName}";
                return s_mRootFolder;
            }
        }

        public static readonly string UserFolderName = "k_u";
        private static string s_mUserFolder = string.Empty;
        public static string UserFolder
        {
            get
            {
                if (string.IsNullOrEmpty(s_mUserFolder))
                    s_mUserFolder = $"{PersistentDataPath}/{UserFolderName}";
                return s_mUserFolder;
            }
        }

        public static ABConfigInfoClient ConfigInfoClient = null;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private static string mPersistentDataPath = string.Empty;
        public static string PersistentDataPath
        {
            get
            {
                if (string.IsNullOrEmpty(mPersistentDataPath))
                {
#if UNITY_EDITOR
                    mPersistentDataPath = System.IO.Path.GetFullPath(Application.dataPath + "/../sandbox");
                    mPersistentDataPath = mPersistentDataPath.Replace(@"\", @"/");
#else
                mPersistentDataPath = Application.persistentDataPath;
#endif
                }
                return mPersistentDataPath;
            }
        }

        private static string mStreamingAssetsUrl = string.Empty;
        public static string StreamingAssetsUrl
        {
            get
            {
                if (string.IsNullOrEmpty(mStreamingAssetsUrl))
                {
                    var mTempSb = new StringBuilder();

#if UNITY_ANDROID && !UNITY_EDITOR
                    mTempSb.Append(Application.streamingAssetsPath);
                    mTempSb.Append("/");
#elif UNITY_IPHONE && !UNITY_EDITOR
                    mTempSb.Append("file://");
                    mTempSb.Append(Application.streamingAssetsPath);
                    mTempSb.Append("/");
#elif UNITY_EDITOR
                    mTempSb.Append("file://");
                    mTempSb.Append(Application.streamingAssetsPath);
                    mTempSb.Append("/");
#endif

#if APPEND_PLATFORM_NAME
                    mTempSb.Append(Utility.GetPlatformName());
                    mTempSb.Append("/");
#endif
                    mStreamingAssetsUrl = mTempSb.ToString();
                }
                return mStreamingAssetsUrl;
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

        public static string GetStreamingAssetsPath(string path, string ext = null, bool loadAB = true)
        {
            return GetStreamingAssetsPathInternal(path,ext,loadAB);
        }

        private static string GetStreamingAssetsPathInternal(string path, string ext, bool loadAB)
        {
            //if (loadAB && configList.ContainsKey(path))
            //{
            //    ABTableItemInfoClient item = configList[path];
            //    if (item.R)
            //        return GetWritePath(path, true, ext);
            //}
            TmpSB.Length = 0;

#if UNITY_ANDROID && !UNITY_EDITOR
            if(loadAB)
            {
                TmpSB.Append(Application.dataPath);
                TmpSB.Append("!assets/");
            }
            else
            {
                TmpSB.Append(Application.streamingAssetsPath);
                TmpSB.Append("/");
            }
#elif UNITY_IPHONE && !UNITY_EDITOR
            if(loadAB)
            {
                TmpSB.Append(Application.dataPath);
                TmpSB.Append("/Raw/");
            }
            else
            {
                TmpSB.Append("file://");
                TmpSB.Append(Application.streamingAssetsPath);
                TmpSB.Append("/");
            }
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            if (!loadAB)
            {
                TmpSB.Append("file://");
            }
            TmpSB.Append(Application.streamingAssetsPath);
            TmpSB.Append("/");
#endif

#if APPEND_PLATFORM_NAME

            TmpSB.Append(Utility.GetPlatformName());
            TmpSB.Append("/");
#endif

            TmpSB.Append(path);
            if (ext != null)
                TmpSB.Append(ext);
            return TmpSB.ToString();
        }

        public static string GetWritePath(string path, bool createFolder = false, string ext = null)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (!Directory.Exists(RootFolder))
            {
                Directory.CreateDirectory(RootFolder);
            }
            string result = null;
            if (createFolder)
            {
                TmpSB.Length = 0;
                TmpSB.Append(PersistentDataPath);
                TmpSB.Append("/");
                TmpSB.Append(RootFolderName);
                string[] ps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (ps.Length > 1)
                {
                    for (int i = 0; i < ps.Length; i++)
                    {
                        TmpSB.Append("/");
                        TmpSB.Append(ps[i]);
                        result = TmpSB.ToString();
                        if (i < ps.Length - 1)
                        {
                            DirectoryInfo folder = new DirectoryInfo(result);
                            if (!folder.Exists)
                                folder.Create();
                        }
                    }
                }
                else
                {
                    TmpSB.Append("/");
                    TmpSB.Append(path);
                }
            }
            else
            {
                TmpSB.Length = 0;
                TmpSB.Append(PersistentDataPath);
                TmpSB.Append("/");
                TmpSB.Append(RootFolderName);
                TmpSB.Append("/");
                TmpSB.Append(path);
            }
            if (ext != null)
                TmpSB.Append(ext);
            result = TmpSB.ToString();
            return result;
        }

        public static string GetPlatformStr()
        {
#if UNITY_EDITOR
            switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
            {
                case UnityEditor.BuildTarget.Android:
                    {
                        return "/Android/";
                    }
                case UnityEditor.BuildTarget.iOS:
                    {
                        return "/IOS/";
                    }
                default:
                    {
                        return "/Standlone/";
                    }
            }
#else
#if UNITY_ANDROID && !UNITY_EDITOR
                return "/Android/";
#elif UNITY_IPHONE && !UNITY_EDITOR
                return "/IOS/";
#else
                return "/Standlone/";
#endif
        
#endif
        }

        
        public static string GetPlatformStringForConfig()
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android:
                    {
                        return "android";
                    }
                case BuildTarget.iOS:
                    {
                        return "ios";
                    }
                default:
                    {
                        return "android";
                    }
            }
#else
#if UNITY_ANDROID && !UNITY_EDITOR
                        return "android";
#elif UNITY_IPHONE && !UNITY_EDITOR
                        return "ios";
#else
                        return "android";
#endif

#endif
        }

        #endregion

    }
}
