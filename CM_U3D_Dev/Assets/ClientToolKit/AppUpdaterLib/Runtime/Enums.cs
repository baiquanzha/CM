namespace MTool.AppUpdaterLib.Runtime
{

    public enum AppUpdaterInnerEventType
    {
        /// <summary>
        /// 执行App更新操作，默认自动进入app更新操作
        /// </summary>
        PerformAppUpdate,

        /// <summary>
        /// 开始执行资源更新操作
        /// </summary>
        StartPerformResUpdateOperation,

        /// <summary>
        /// 更新部分资源
        /// </summary>
        StartPerformResPartialUpdateOperation,

        OnCurrentResUpdateCompleted,

        PerformResUpdateOperation,

        OnApplicationFocus,

        OnApplicationQuit
    }


    public enum FileServerType
    {
        CDN,
        OSS,
    }


    public enum AppUpdaterErrorType
    {
        None,

        LoadBuiltinAppInfoFailure,//加载内建AppInfo配置失败

        ParseBuiltinAppInfoFailure,//解析StreamingAssets目录下的appInfo失败

        ParseLocalAppInfoFailure,//解析本地appinfo失败

        DownloadLighthouseFailure,//下载lighthouse配置失败

        ParseLighthouseConfigError,//解析lighthouse配置失败

        LighthouseConfigServersIsUnReachable,//无效的lighthouse配置,当前没有服务器可以连接

        DownloadLighthouseConfigInvalid,//下载lighthouse配置无效

        RequestGetVersionFailure,//EntryPointRequest请求返回失败

        RequestDataResVersionFailure,//返回的数据表资源版本为空

        RequestUnityResVersionFailure,//返回的Unity资源版本为空

        RequestAppRevisionNumIsSmallToLocal,//返回的修订号小于当前修订号

        RequestAppRevisionNumFailure,//返回的Unity资源修订号不合理

        RequestResManifestFailure,//请求清单文件失败

        ParseLocalResManifestFailure,//解析本地清单文件失败

        ParseRemoteResManifestFailure,//解析远端清单文件失败

        DownloadFileFailure,//文件下载失败

        DiskIsNotEnoughToDownPatchFiles,//磁盘空间不足

        AppBuiltInVersionNumNotCompatibleToExternal,//app内建版本号与外部不兼容

        DeleteExternalStorageFilesFailure,//删除App外部目录失败

        UserGiveUpDownload,//用户禁止下载

        LoadBuiltinResManifestFailure,//加载内建资源清单失败

        LoadBuiltinDataManifestFailure,//加载内建配表清单失败

        Unknow,
    }

    /// <summary>
    /// 资源更新类型
    /// </summary>
    public enum UpdateResourceType
    {
        UnKnow,

        TableData,

        NormalResource,
    }


    public enum AppUpdaterHintName
    {
        LOWER_LUA_NAME,//lua路径小写，解决针对ios大小写敏感相关问题
        MANUAL_PERFORM_APP_UPDATE,//手动进入app更新
        ENABLE_RES_INCREMENTAL_UPDATE,//激活资源增量式更新
        ENABLE_UNITY_RES_UPDATE,    //激活Unity资源更新 (默认打开)
        ENABLE_CHECK_MISSING_RES,   //激活热更新资源完整检查
    }

    public enum ResSyncMode
    {
        FULL,//同步远端所有的资源

        LOCAL,//只同步本地清单资源

        SUB_GROUP,//只同步子组的资源
    }

    public enum AppUpdaterBool
    {
        FALSE,
        TRUE
    }

    public enum AppUpdateDownloadMode
    {
        SingleThread,   //单线程下载
        MultiThread,    //多线程下载
    }
}