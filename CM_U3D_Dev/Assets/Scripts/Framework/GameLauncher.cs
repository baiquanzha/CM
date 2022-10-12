using UnityEngine;
using MTool.Framework.Network;
using MTool.Framework.Http;
using MTool.AssetBundleManager.Runtime;
using MTool.LoggerModule.Runtime;
using MTool.Log4NetForUnity.Runtime;
using MTool.AppUpdaterLib.Runtime;
using MTool.AppUpdaterLib.Runtime.Managers;
using ProtokitHelper;

namespace MTool.Framework
{
    using AssetBundleManager = AssetBundleManager.Runtime.AssetBundleManager;
    using ILogger = LoggerModule.Runtime.ILogger;

    public class GameLauncher : SingletonObject<GameLauncher> {
        public ILogger logger;

        public static NetworkManager Network {
            get;
            private set;
        }

        public static LuaManager Lua {
            get;
            private set;
        }

        public static HttpManager Http {
            get;
            private set;
        }

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);

            //日志管理
            LoggerManager.SetCurrentLoggerProvider(new Log4NetLoggerProvider());
            logger = LoggerManager.GetLogger("Log4NetForUnityLogger");
            logger.Debug($"组件版本 : " + AppUpdaterVersion.GetVersionInfo());
            logger.Debug($"客户端唯一id : " + AppUpdaterManager.ClientUniqueId);
        }

        private void Start()
        {
            LoggerManager.SetCurrentLoggerProvider(new Log4NetLoggerProvider());

            Lua = FrameworkEntry.GetModule<LuaManager>();
            Network = FrameworkEntry.GetModule<NetworkManager>();
            Http = FrameworkEntry.GetModule<HttpManager>();

            Network.SetMsgProcesser(ProtokitMsgProcessor.Instance);
            ProtokitUtil.Instance.Init();

            AssetBundleManager.Initialize(OnABMgrInited, OnABMgrInitFailed);
        }

        private void OnABMgrInited() {
            LuaBehaviour lua = this.gameObject.AddComponent<LuaBehaviour>();
            lua.DoFile("Booter.lua");
        }

        private void OnABMgrInitFailed(ResourceLoadInitError errCode) {
            Debug.LogErrorFormat("AssetBundleManager初始化失败, error code = {0}", errCode);
        }

        private void Update()
        {
            FrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
            ProtokitHelper.ProtokitClient.Instance.Update();
            ProtokitHelper.ProtokitHttpClient.Instance.Update();
        }

        private void LateUpdate()
        {
            FrameworkEntry.LateUpdate();
        }

        private void FixedUpdate()
        {
        }

        private void OnApplicationFocus(bool focus)
        {
        }

        private void OnApplicationPause(bool pause)
        {
        }

        private void OnApplicationQuit()
        {
            FrameworkEntry.ShutDown();
            LoggerManager.Shutdown();
        }
    }
}