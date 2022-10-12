using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Net.Mime;
using System.Runtime.InteropServices;
using MTool.Core.Pipeline;
using log4net.Filter;
using UnityEditor;
using UnityEngine;
using IFilter = MTool.Core.Pipeline.IFilter;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResProcess
{
    public class GenerateLuaAssetBundleAction : BaseBuildFilterAction
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion


        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion


        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion


        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            var luaprojectDir = input.GetData<string>("LuaProject", AppBuildContext.GetLuaProjectPath());

            if (string.IsNullOrEmpty(luaprojectDir))
            {
                AppBuildContext.ErrorSb.AppendLine($"The LuaProject not set!");
                return false;
            }

            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.ExecuteInternal(filter,input);
            this.State = ActionState.Completed;
        }

        private void ExecuteInternal(IFilter filter, IPipelineInput input)
        {
            Logger.Info("Start copy lua files to assets folder.");
            var luaprojectDir = input.GetData<string>("LuaProject", AppBuildContext.GetLuaProjectPath());
            luaprojectDir = EditorUtils.OptimazePath(luaprojectDir);

            DirectoryInfo dirInfo = new DirectoryInfo(luaprojectDir);
            FileInfo[] fileInfos = dirInfo.GetFiles("*.lua", SearchOption.AllDirectories);

            string targetFolder = EditorUtils.OptimazePath($"{Application.dataPath}/lua");
            if (Directory.Exists(targetFolder))
            {
                Directory.Delete(targetFolder,true);
            }
            Directory.CreateDirectory(targetFolder);

            List<string> allLuaFiles = new List<string>();
            foreach (var fileInfo in fileInfos)
            {
                string sourcePath = EditorUtils.OptimazePath(fileInfo.FullName);
                string sourcePathWithoutExtension =
                    $"{Path.GetDirectoryName(sourcePath)}/{Path.GetFileNameWithoutExtension(sourcePath)}";
                sourcePathWithoutExtension = EditorUtils.OptimazePath(sourcePathWithoutExtension);
                string targePath = $"{targetFolder}{sourcePathWithoutExtension.Replace(luaprojectDir, "")}.bytes";

                string dirName = Path.GetDirectoryName(targePath);
                allLuaFiles.Add($"Assets/lua{sourcePathWithoutExtension.Replace(luaprojectDir, "")}.bytes");
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                File.Copy(sourcePath, targePath, true);
            }

            AssetDatabase.Refresh();
            Logger.Info("Copy lua files to assets folder successful!");


            Logger.Info("Start build lua assetbundle.");

            AssetBundleBuild[] builds = new AssetBundleBuild[1];
            builds[0] = new AssetBundleBuild();
            builds[0].assetBundleName = "lua";
            builds[0].assetNames = allLuaFiles.ToArray();

            var outputPath = EditorUtils.OptimazePath($"{Application.dataPath}/../luaassetbundle");
            if (Directory.Exists(outputPath))
            {
                Directory.Delete(outputPath, true);
            }

            Directory.CreateDirectory(outputPath);

            AssetBundleManifest m = BuildPipeline.BuildAssetBundles(outputPath, builds,
                BuildAssetBundleOptions.DeterministicAssetBundle |
                BuildAssetBundleOptions.UncompressedAssetBundle,
                EditorUserBuildSettings.activeBuildTarget);

            if (m != null)
            {
                input.SetData(EnvironmentVariables.LUA_AB_PATH_KEY,$"{outputPath}/lua");
                Logger.Info("Build lua assetbundle successful!");
            }
            else
            {
                Logger.Info("Build lua assetbundle failure!");
            }
            
        }
        #endregion

    }
}