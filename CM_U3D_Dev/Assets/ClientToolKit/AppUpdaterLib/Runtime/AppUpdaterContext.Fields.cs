using System;
using System.Collections.Generic;
using System.Text;
using MTool.AppUpdaterLib.Runtime.Helps;
using MTool.AppUpdaterLib.Runtime.Interfaces;
using MTool.AppUpdaterLib.Runtime.Protocols;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using MTool.AppUpdaterLib.Runtime.Utilities;

namespace MTool.AppUpdaterLib.Runtime
{
    public class AppUpdaterProgressData
    {
        #region 下载信息

        /// <summary>
        /// 当前热更新的资源类型
        /// </summary>
        public UpdateResourceType CurrentUpdateResourceType { set; get; }

        /// <summary>
        /// 当前下载进度
        /// </summary>
        public float Progress { set; get; }


        /// <summary>
        /// 总共需要下载的文件数目
        /// </summary>
        public ulong TotalDownloadFileCount { set; get; }

        /// <summary>
        /// 总的下载大小
        /// </summary>
        public ulong TotalDownloadSize { set; get; }

        /// <summary>
        /// 当前正在下载的文件的大小 
        /// </summary>
        public ulong CurrentDownloadingFileSize { set; get; }

        /// <summary>
        /// 当前已下载的文件数目
        /// </summary>
        public ulong CurrentDownloadFileCount { set; get; }
        /// <summary>
        /// 当前已下载的大小
        /// </summary>
        public ulong CurrentDownloadSize { set; get; }

        /// <summary>
        /// 下载总时长
        /// </summary>
        public float CurrentDownloadFileTotalTime { set; get; }

        /// <summary>
        /// 当前下载速度
        /// </summary>
        //public float LoadSpeed { set; get; }

        #endregion

        public void Clear()
        {
            this.CurrentUpdateResourceType = UpdateResourceType.UnKnow;
            this.Progress = 0;
            this.TotalDownloadSize = 0;
            this.CurrentDownloadingFileSize = 0;
            this.CurrentDownloadSize = 0;
            this.TotalDownloadFileCount = 0;
            this.CurrentDownloadFileCount = 0;
        }
    }

    internal class ResUpdateConfig
    {
        public ResSyncMode Mode = ResSyncMode.FULL;

        public AppUpdaterFileUpdateRuleFilter Filter;

        public AppUpdaterFileUpdateRuleFilter LocalModeFilter;
    }

    internal class VersionDesc
    {
        public UpdateResourceType Type = UpdateResourceType.UnKnow;

        public string RemoteMd5;

        public string LocalMd5;
    }


    internal class ResUpdateTarget
    {
        public static readonly VersionDesc[] DefaultVersionDescs = new VersionDesc[0];
        /// <summary>
        /// 需要更新的资源
        /// </summary>
        public VersionDesc[] VersionDescs = DefaultVersionDescs;

        public static readonly int DefaultResVerisonIdx = -1;
        /// <summary>
        /// 当前更新的资源版本相对于ResVersions的索引
        /// </summary>
        public int CurrentResVersionIdx = DefaultResVerisonIdx;

        /// <summary>
        /// 资源目标版本号，即目标Patch
        /// </summary>
        public string TargetResVersionNum = string.Empty;

        public void Clear()
        {
            this.VersionDescs = DefaultVersionDescs;
            this.CurrentResVersionIdx = DefaultResVerisonIdx;
        }

    }


    internal class DiskInfo
    {
        public bool IsGetReady = false;

        public int AvailableSpace;
        public int BusySpace;
        public int TotalSpace;

        public DateTime time;

        public void Clear()
        {
            this.IsGetReady = false;
            this.AvailableSpace = 0;
            this.BusySpace = 0;
            this.TotalSpace = 0;
            this.time = TimeUtility.EpochTime;            
        }

    }
    

    internal partial class AppUpdaterContext
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        /// <summary>
        /// 当前所处的状态
        /// </summary>
        public string StateName { set; get; }

        /// <summary>
        /// 进度数据
        /// </summary>
        public AppUpdaterProgressData ProgressData { private set; get; } = new AppUpdaterProgressData();

        /// <summary>
        /// 热更新错误类型
        /// </summary>
        public AppUpdaterErrorType ErrorType { set; get; } = AppUpdaterErrorType.None;

        /// <summary>
        /// 错误信息
        /// </summary>
        public StringBuilder InfoStringBuilder { set; get; } = new StringBuilder();

        private StringBuilder mTempSb = new StringBuilder();
        
        public bool IsFirstRun = true;

        /// <summary>
        /// GetVersion返回相关
        /// </summary>
        public GetVersionResponseInfo GetVersionResponseInfo { set; get; }

        public IAppUpdaterRequester Requester { set; get; }

        public LighthouseConfigDownloader LighthouseConfigDownloader = null;

        public IStorageInfoProvider StorageInfoProvider = null;

        public DiskInfo DiskInfo = new DiskInfo();

        public ResUpdateConfig ResUpdateConfig = new ResUpdateConfig();

        public ResUpdateTarget ResUpdateTarget = new ResUpdateTarget();

        public AppUpdateDownloadMode DownloadMode = AppUpdateDownloadMode.SingleThread;

        /// <summary>
        /// 待清除的老资源列表
        /// </summary>
        public List<string> ExpireResFileList = new List<string>();

        public VersionManifest BuiltInResManifest;

        public VersionManifest LocalResManifest;

        #endregion


        public void Clear()
        {
            this.StateName = string.Empty;
            this.ProgressData.Clear();
            
            this.ErrorType = AppUpdaterErrorType.None;
            this.mTempSb.Clear();
            this.IsFirstRun = false;
            
            this.GetVersionResponseInfo = null;
            this.ResUpdateTarget.Clear();
            this.LighthouseConfigDownloader?.Clear();
            this.DiskInfo.Clear();
            this.ExpireResFileList.Clear();
        }

#if DEBUG_APP_UPDATER

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"更新的资源类型 : {this.GetResType(this.ProgressData.CurrentUpdateResourceType)}");
            sb.AppendLine($"当前所属状态 : {this.StateName}");
            sb.AppendLine($"当前下载进度 : {(this.ProgressData.Progress * 100):f1}%");
            sb.AppendLine($"当前需要下载的文件数量 : {this.ProgressData.TotalDownloadFileCount}");
            sb.AppendLine($"当前需要下载的文件大小 : {this.ProgressData.TotalDownloadSize}");
            sb.AppendLine($"正在下载的文件大小 : {this.ProgressData.CurrentDownloadingFileSize}");
            sb.AppendLine($"当前已下载的文件数量 : {this.ProgressData.CurrentDownloadFileCount}");
            sb.AppendLine($"当前已下载的文件大小 : {this.ProgressData.CurrentDownloadSize}");
            sb.AppendLine($"当前下载的总时长 : {this.ProgressData.CurrentDownloadFileTotalTime}");

            if (this.DiskInfo.IsGetReady)
            {
                sb.AppendLine($"Last fetch disk info time : {this.DiskInfo.time:yyyy_MM_dd-HH_mm_ss.fff}");
                sb.AppendLine($"Disk totalSpace : {this.DiskInfo.TotalSpace:f1}");
                sb.AppendLine($"Disk availableSpace: {this.DiskInfo.AvailableSpace:f1}");
                sb.AppendLine($"Disk busySpace : {this.DiskInfo.BusySpace:f1}");
            }

            if (this.ErrorType != AppUpdaterErrorType.None)
            {
                sb.Append($"错误类型 ：{this.ErrorType} 描述 : {ErrorTypeHelper.GetErrorString(this.ErrorType)}");
            }

            return sb.ToString();
        }

        public string GetResType(UpdateResourceType type)
        {
            switch (type)
            {
                case UpdateResourceType.TableData:
                    return "表数据";
                case UpdateResourceType.NormalResource:
                    return "Unity资源";
                default:
                    return "未知";
            }
        }
#endif

    }
}
