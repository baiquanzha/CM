using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.AppUpdaterLib.Runtime.Manifests;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.Factories
{
    public static class SimpleFactory
    {

        public static AppInfoManifest CreateAppInfoManifest()
        {
            var appInfoManifest = new AppInfoManifest
            {
                TargetPlatform = EditorUserBuildSettings.activeBuildTarget.ToString()
            };
            return appInfoManifest;
        }
    }
}
