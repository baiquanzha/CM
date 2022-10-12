using System;
using MTool.AppUpdaterLib.Runtime.Configs;

namespace MTool.AppUpdaterLib.Runtime
{
    /// <summary>
    /// 更新模块发生错误时回调
    /// </summary>
    /// <param name="errorType"></param>
    /// <param name="desc"></param>
    public delegate void AppUpdaterErrorCallback(AppUpdaterErrorType errorType , string desc);

    /// <summary>
    /// “服务器维护中”的回调函数
    /// </summary>
    /// <param name="errorType"></param>
    /// <param name="desc"></param>
    public delegate void AppUpdaterServerMaintenanceCallback(LighthouseConfig.MaintenanceInfo maintenanceInfo);

    /// <summary>
    /// 强制更新App的回调函数
    /// </summary>
    public delegate void AppUpdaterForceUpdateCallback(LighthouseConfig.UpdateDataInfo info);

    /// <summary>
    /// 目标版本获取后的回调
    /// </summary>
    public delegate void AppUpdaterOnTargetVersionObtainCallback(string version);

    /// <summary>
    /// 热更新模块执行完成回调
    /// </summary>
    public delegate void AppUpdaterPerformCompletedCallback();

    /// <summary>
    /// 文件更新规则过滤器
    /// </summary>
    /// <param name="remoteName">文件所在的远程路径名（例如：resource/xxx.x）</param>
    /// <returns></returns>
    public delegate bool AppUpdaterFileUpdateRuleFilter(ref string remoteName);

    /// <summary>
    /// Obsolete delegate type
    /// </summary>
    public delegate void AppUpdaterStartDownloadCallback();

    /// <summary>
    /// 获取Lighthouse完整内容
    /// </summary>
    /// <param name="content"></param>
    public delegate void AppUpdaterGetLighthouseContentCallback(string content);
  
    /// <summary>
    /// 是否允许开始下载
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public delegate void AppUpdaterEnableDownloadJudge(ulong size, Action downloadAction, Action stopAction);

    /// <summary>
    /// 开始下载缺失资源文件回调
    /// </summary>
    /// <param name="size"></param>
    /// <param name="count"></param>
    public delegate void AppUpdaterStartDownloadMissingResCallback(ulong size, ulong count);

    /// <summary>
    /// 下载缺失资源文件完成回调
    /// </summary>
    public delegate void AppUpdaterDownloadMissingResCompleteCallback();
}
