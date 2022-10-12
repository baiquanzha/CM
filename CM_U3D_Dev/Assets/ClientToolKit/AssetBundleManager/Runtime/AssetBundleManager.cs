using System;
using MTool.LoggerModule.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

namespace MTool.AssetBundleManager.Runtime
{
    public class AssetBundleManager : MonoBehaviour
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private static readonly Lazy<LoggerModule.Runtime.ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("AssetBundleManager"));

        private static bool s_mInitialize = false;

        private static ABMgrHandle ABMgrHandle = null;

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        /// <summary>
        /// AssetBundle资源加载模块是否已初始化
        /// </summary>
        public static bool IsInitialize => s_mInitialize;

#if UNITY_EDITOR
        /// <summary>
        /// Flag to indicate if we want to simulate assetBundles in Editor without building them actually.
        /// </summary>
        public static bool SimulateAssetBundleInEditor
        {
            get
            {
                if (m_SimulateAssetBundleInEditor == -1)
                    m_SimulateAssetBundleInEditor = EditorPrefs.GetBool(kSimulateAssetBundles, true) ? 1 : 0;

                return m_SimulateAssetBundleInEditor != 0;
            }
            set
            {
                int newValue = value ? 1 : 0;
                if (newValue != m_SimulateAssetBundleInEditor)
                {
                    m_SimulateAssetBundleInEditor = newValue;
                    EditorPrefs.SetBool(kSimulateAssetBundles, value);
                }
            }
        }

        static int m_SimulateAssetBundleInEditor = -1;
        const string kSimulateAssetBundles = "SimulateAssetBundles";


        //public static bool LuaDevelopmentModeInEditor
        //{
        //    get
        //    {
        //        if (m_LuaDevelopmentMode == -1)
        //            m_LuaDevelopmentMode = EditorPrefs.GetBool(kLuaDevelopmentMode, false) ? 1 : 0;

        //        return m_LuaDevelopmentMode != 0;
        //    }
        //    set
        //    {
        //        int newValue = value ? 1 : 0;
        //        if (newValue != m_LuaDevelopmentMode)
        //        {
        //            m_LuaDevelopmentMode = newValue;
        //            EditorPrefs.SetBool(kLuaDevelopmentMode, value);
        //        }
        //    }
        //}
        //static int m_LuaDevelopmentMode = -1;
        //const string kLuaDevelopmentMode = "LuaDevelopmentMode";
#endif

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

#if UNITY_EDITOR

        /// <summary>
        /// 设置资源目录
        /// </summary>
        /// <param name="folderName"></param>
        public static void SetAssetsRootFolder(string folderName)
        {
            if (CheckResourceFolderName(folderName))
            {
                ABMgrHandle.ResourcesFolder = folderName;
            }
            else
            {
                s_mLogger.Value.Warn($"Set assets root folder that name is \'{folderName}\' failure!");
            }
        }

        private static bool CheckResourceFolderName(string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
                return false;
            return System.IO.Directory.Exists($"Assets/{folderName}");
        }
#endif

        /// <summary>
        /// 初始化AssetBundleManager
        /// </summary>
        /// <param name="onInitCompleted">初始化完成回调</param>
        /// <param name="onInitError">初始化异常回调</param>
        public static void Initialize(Action onInitCompleted = null, Action<ResourceLoadInitError> onInitError = null)
        {
            if (s_mInitialize)
            {
                s_mLogger.Value.Error($"AssetBundleManager is initialized!");
                return;
            }
            var go = new GameObject("AssetBundleManager", typeof(AssetBundleManager));
            DontDestroyOnLoad(go);
            ABMgrHandle = go.AddComponent<ABMgrHandle>();
            ABMgrHandle.Init(() =>
                {
                    s_mInitialize = true;
                    s_mLogger.Value.Info($"AssetBundleManager initialize completed !");
                    onInitCompleted?.Invoke();
                },
                error =>
                {
                    s_mLogger.Value.Error($"AssetBundleManager initialize fail! Error : {error} .");
                    onInitError?.Invoke(error);
                });
        }

        /// <summary>
        /// 设置当前需要编译成AssetBundle的场景资源文件夹名
        /// </summary>
        /// <param name="sceneFolderName">资源文件夹下的场景文件夹名</param>
        public static void SetSceneFolderName(string sceneFolderName = "")
        {
            if (!string.IsNullOrEmpty(sceneFolderName))
            {
                ABMgrHandle.sceneFolderName = sceneFolderName;
            }
        }

        /// <summary>
        /// 获取配置加载路径
        /// </summary>
        /// <param name="tableName">配置表名称</param>
        /// <param name="lowerName">配置文件是否采用全小写</param>
        /// <param name="ext">配置表文件扩展名</param>
        /// <returns>配置加载路径,是否可通过File.Read读取</returns>
        public static string GetTableLoadPath(string tableName, out bool fileReadable, bool lowerName = false, string ext = ".bytes")
        {
            CheckIsInitialize("GetTableLoadPath");
            return ABMgrHandle.GetTableLoadPath(tableName, out fileReadable, lowerName, ext);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源相对路径。该路径相对于"Assets/ResourcesAB" ，例如：abscene/test，且不带后缀名</param>
        /// <returns></returns>
        [Obsolete("This method is deprecated.Use Load(string path, System.Type type) instead.",false)]
        public static Object Load(string path)
        {
            CheckIsInitialize("Load(string path)");
            return ABMgrHandle.Load(path);
        }

        [Obsolete("This method is deprecated.Use LoadAsync(string path, System.Type type, ObjectCallBack callBack) instead.", false)]
        public static void LoadAsync(string path, ObjectCallBack callback)
        {
            CheckIsInitialize("LoadAsync(string path, ObjectCallBack callback)");
            ABMgrHandle.LoadAsync(path, callback);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源相对路径.该路径相对于资源根目录 ，例如：abscene/test，且不带后缀名</param>
        /// <param name="type">需要加载的资源类型</param>
        /// <returns>资源对象</returns>
        public static Object Load(string path, System.Type type)
        {
            CheckIsInitialize("Load(string path, System.Type type)");
            return ABMgrHandle.Load(path, type);
        }

        public static void LoadAsync(string path, System.Type type, ObjectCallBack callBack)
        {
            CheckIsInitialize("LoadAsync(string path, System.Type type, ObjectCallBack callBack)");
            ABMgrHandle.LoadAsync(path, type, callBack);
        }

        public static T Load<T>(string path) where T : Object
        {
            CheckIsInitialize("Load<T>");
            return ABMgrHandle.Load<T>(path);
        }

        public static void LoadAsync<T>(string path, ObjectCallBack callBack) where T : Object
        {
            CheckIsInitialize("LoadAsync<T>(string path, ObjectCallBack callBack)");
            ABMgrHandle.LoadAsync<T>(path, callBack);
        }

        /// <summary>
        /// 只加载资源的AssetBundle
        /// </summary>
        /// <param name="path">资源路径</param>
        public static void LoadBundleOnly(string path)
        {
            CheckIsInitialize("LoadBundleOnly");
            ABMgrHandle.LoadBundleOnly(path);
        }

        /// <summary>
        /// 只加载资源的AssetBundle，异步模式（Simulation Mode下由于实现机制，暂时未实现异步Load）
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        public static void LoadBundleOnlyAsync(string path, BundleLoadCompletedCallBack callback)
        {
            CheckIsInitialize("LoadBundleOnlyAsync(string path, BundleLoadCompletedCallBack callback)");
            ABMgrHandle.LoadBundleOnlyAsync(path, callback);
        }

        public static void ReleaseBundle(string path)
        {
            CheckIsInitialize("ReleaseBundle(string path)");
            ABMgrHandle.UnLoadBundle(path);
        }


        public static T LoadFromResourceFolder<T>(string path)
            where T : Object
        {
            CheckIsInitialize("LoadFromResourceFolder<T>(string path)");
            return ABMgrHandle.LoadFromResource<T>(path);
        }

        public static void LoadAndInit(string path, ABObjectCallBack callback, Vector3 pos, Vector3 rot,
            bool useDefaultPos = false)
        {
            CheckIsInitialize("LoadAndInit1");
            ABMgrHandle.LoadAndInit(path, callback, pos, rot, useDefaultPos);
        }

        public static void LoadAndInitAsync(string path, ABObjectCallBack callback, Vector3 pos, Vector3 rot, bool useDefaultPos = false)
        {
            CheckIsInitialize("LoadAndInitAsync1");
            ABMgrHandle.LoadAndInitAsync(path, callback, pos, rot, useDefaultPos);
        }

        public static void LoadAndInit(string path, ABObjectCallBack callback, bool useDefaultPos = false)
        {
            CheckIsInitialize("LoadAndInit2");
            ABMgrHandle.LoadAndInit(path, callback, useDefaultPos);
        }

        public static void LoadAndInitAsync(string path, ABObjectCallBack callback, bool useDefaultPos = false)
        {
            CheckIsInitialize("LoadAndInitAsync2");
            ABMgrHandle.LoadAndInitAsync(path, callback, useDefaultPos);
        }

        public static GameObject LoadAndInit(string path, Vector3 pos, Vector3 rot, bool useDefaultPos = false)
        {
            CheckIsInitialize("LoadAndInit3");
            return ABMgrHandle.LoadAndInit(path, pos, rot, useDefaultPos);
        }


        public static void Release(string path)
        {
            CheckIsInitialize("Release path");
            ABMgrHandle.Delete(path);
        }

        public static void Release(Object obj)
        {
            CheckIsInitialize("Release obj");
            ABMgrHandle.Delete(obj);
        }

        public static void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            CheckIsInitialize("LoadScene");
            ABMgrHandle.LoadScene(sceneName, mode);
        }

        [Obsolete("Use AssetBundleManager.LoadSceneAsync .  This function is not safe . ")]
        public static void UnLoadScene(string sceneName)
        {
            CheckIsInitialize("UnLoadScene");
            ABMgrHandle.UnLoadScene(sceneName);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            CheckIsInitialize("LoadSceneAsync");
            return ABMgrHandle.LoadSceneAsync(sceneName, mode);
        }

        public static AsyncOperation UnLoadSceneAsync(string sceneName)
        {
            CheckIsInitialize("UnLoadSceneAsync");
            return ABMgrHandle.UnLoadSceneAsync(sceneName);
        }

        private static void CheckIsInitialize(string methodName)
        {
            if (!s_mInitialize)
                throw new NullReferenceException($"Your want to use \"AssetBuildManager\" that not initialized ! Call method :  \"{methodName}\" .");
        }
        #endregion

    }
}
