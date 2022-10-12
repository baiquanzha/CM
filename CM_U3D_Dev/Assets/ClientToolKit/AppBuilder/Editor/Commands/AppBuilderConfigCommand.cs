using MTool.AppBuilder.Editor.Builds;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Commands
{
    internal static class AppBuilderConfigCommand
    {
        [MenuItem("MTool/AppBuilder/Make Default AppBuilder Config")]
        static void CreateDefaultAppBuilderConfig()
        {
            string targetFolder = AppBuildConfig.GetConfigAbsoluteFolderPath();

            if (!Directory.Exists(targetFolder))
            {
                Directory.CreateDirectory(targetFolder);

                AssetDatabase.Refresh();
            }

            string configPath = AppBuildConfig.GetConfigAbsolutePath();
            if (File.Exists(configPath))
            {
                Debug.LogWarning("The appBuilder config that you want to make is exist in current project!");
                return;
            }

            configPath = AppBuildConfig.GetConfigRelativeToProjectPath();
            var config = ScriptableObject.CreateInstance<AppBuildConfig>();
            AssetDatabase.CreateAsset(config,configPath);

            Debug.Log("Write default appBuilder default config completd!");

            AssetDatabase.Refresh();
        }

    }

}
