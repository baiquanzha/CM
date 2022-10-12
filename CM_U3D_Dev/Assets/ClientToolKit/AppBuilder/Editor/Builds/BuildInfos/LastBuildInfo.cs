using System;
using System.Collections.Generic;
using System.Linq;
using MTool.AppBuilder.Editor.Builds.Factories;
using MTool.AppUpdaterLib.Runtime.Manifests;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.BuildInfos
{
    [Serializable]
    public class BuildInfo
    {
        /// <summary>
        /// 目标平台
        /// </summary>
        public string TargetPlatform;

        public AppInfoManifest baseVersionInfo = SimpleFactory.CreateAppInfoManifest();

        public AppInfoManifest versionInfo = SimpleFactory.CreateAppInfoManifest();

        public BuildInfo(string targetPlatformStr)
        {
            this.TargetPlatform = targetPlatformStr;
        }
    }

    [Serializable]
    public class LastBuildInfo
    {
        public List<BuildInfo> buildInfos = new List<BuildInfo>();

        public BuildInfo GetBuildInfo(BuildTarget target)
        {
            return buildInfos.FirstOrDefault(x => { return x.TargetPlatform == target.ToString(); });
        }

        public BuildInfo GetCurrentBuildInfo()
        {
            return GetBuildInfo(EditorUserBuildSettings.activeBuildTarget);
        }

        public void AddAppInfo(AppInfoManifest baseVersionInfo, AppInfoManifest versionInfo)
        {
            var buildInfo = GetCurrentBuildInfo();
            if (buildInfo == null)
            {
                buildInfo = new BuildInfo(EditorUserBuildSettings.activeBuildTarget.ToString());
                buildInfos.Add(buildInfo);
            }

            if (baseVersionInfo != null)
            {
                buildInfo.baseVersionInfo = baseVersionInfo;
            }

            buildInfo.versionInfo = versionInfo;
        }
    }

}
