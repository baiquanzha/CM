using System;
using System.Collections.Generic;
using MTool.Core.Functional;
using MTool.LoggerModule.Runtime;

namespace MTool.AppUpdaterLib.Runtime
{
	public sealed partial class VersionManifest
    {

        private static readonly ILogger s_mLogger = LoggerManager.GetLogger("VersionManifest");

        private readonly Dictionary<string,FileDesc> mFileDscsDic = new Dictionary<string, FileDesc>();

        private bool mDicIsInitialized = false;

        public int Count => this.Datas.Count;

        public void InitDic()
        {
            if (this.mDicIsInitialized)
            {
                return;
            }
            this.Datas.ForCall((x, index) =>
            {
                var data = this.Datas[index];
                var key = data.N;
                mFileDscsDic[key] = data;
            });
            this.mDicIsInitialized = true;
        }

        public List<FileDesc> CalculateDifference(VersionManifest other,ResSyncMode mode = ResSyncMode.FULL , AppUpdaterFileUpdateRuleFilter filter = null, AppUpdaterFileUpdateRuleFilter localModeFilter = null)
        {
            if (mode == ResSyncMode.SUB_GROUP && filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            this.InitDic();
            List<FileDesc> diff = null;

            foreach (var fileDesc in other.Datas)
            {
                if (mode == ResSyncMode.FULL)
                {
                    if (this.mFileDscsDic.TryGetValue(fileDesc.N, out var desc))
                    {
                        if (String.Compare(desc.H, fileDesc.H, StringComparison.Ordinal) != 0)
                        {
                            if (diff == null)
                                diff = new List<FileDesc>();
                            diff.Add(fileDesc);
                        }
                    }
                    else
                    {
                        if (diff == null)
                            diff = new List<FileDesc>();
                        diff.Add(fileDesc);
                    }
                }
                else
                {
                    if (mode == ResSyncMode.LOCAL)
                    {
                        if (this.mFileDscsDic.TryGetValue(fileDesc.N, out var desc))
                        {
                            if (String.Compare(desc.H, fileDesc.H, StringComparison.Ordinal) != 0)
                            {
                                if (diff == null)
                                    diff = new List<FileDesc>();
                                diff.Add(fileDesc);
                            }
                        }
                        else
                        {
                            if (localModeFilter != null)
                            {
                                if (localModeFilter(ref fileDesc.RN))
                                {
                                    continue;
                                }
                                if (diff == null)
                                    diff = new List<FileDesc>();
                                diff.Add(fileDesc);
                            }
                        }
                    }
                    else if(mode == ResSyncMode.SUB_GROUP)//游戏中更新某个文件夹
                    {
                        if (filter(ref fileDesc.RN))
                        {
                            continue;
                        }

                        if (this.mFileDscsDic.TryGetValue(fileDesc.N, out var desc))
                        {
                            if (String.Compare(desc.H, fileDesc.H, StringComparison.Ordinal) != 0)
                            {
                                if (diff == null)
                                    diff = new List<FileDesc>();
                                diff.Add(fileDesc);
                            }
                        }
                        else
                        {
                            if (diff == null)
                                diff = new List<FileDesc>();
                            diff.Add(fileDesc);
                        }
                    }
                }
            }

            return diff;
        }

        public void UpdateInnerFile(FileDesc desc)
        {
            this.InitDic();
            FileDesc foundDesc = null;

            s_mLogger.Debug($"Update inner file that name is \"{desc.N}\" .");
            if (this.mFileDscsDic.TryGetValue(desc.N, out foundDesc))
            {
                desc.CopyTo(foundDesc);
            }
            else
            {
                this.mFileDscsDic.Add(desc.N,desc);
                this.Datas.Add(desc);
            }
        }

        public ulong GetTotalSize()
        {
            ulong totalSize = 0;
            this.Datas.ForCall((x, index) =>
            {
                totalSize += (ulong)x.S;
            });

            return totalSize;
        }

    }
}
