using MTool.AppUpdaterLib.Runtime;
using MTool.Core.Functional;
using MTool.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTool.AppBuilder.Editor.Builds.BuildInfos
{
    [Serializable]
    public class BaseBuildUnityResManifestInfo
    {
        public string appVersion = string.Empty;

        public string unityResVersion = string.Empty;

        public VersionManifest data = new VersionManifest();

        public void Copy(VersionManifest other)
        {
            other.Datas.ForEach(x => { other.Datas.Add(x.Clone()); });
        }

        public void Union(VersionManifest other)
        {
            Dictionary<string, FileDesc> tempDic = new Dictionary<string, FileDesc>();
            data.Datas.ForEach(x=>tempDic[x.N] = x);

            other.Datas.ForEach(x =>
            {
                FileDesc targetFileDesc;
                if (tempDic.TryGetValue(x.N , out targetFileDesc))
                {
                    if (targetFileDesc != null && !string.Equals(targetFileDesc.H, x.H, StringComparison.Ordinal))
                    {
                        targetFileDesc.H = x.H;
                    }
                }
                else
                {
                    tempDic.Add(x.N,x);
                }
            });

            List<FileDesc> fileDescList = new List<FileDesc>();
            tempDic.ForeachCall(x => { fileDescList.Add(x.Value); });
            data.Datas = fileDescList;
        }
    }
}
