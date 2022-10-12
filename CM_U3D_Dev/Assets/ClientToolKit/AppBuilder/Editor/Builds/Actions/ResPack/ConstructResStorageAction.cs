using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MTool.AppUpdaterLib.Runtime;
using MTool.AssetBundleManager.Runtime;
using MTool.Core.Pipeline;
using UnityEngine;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class ConstructResStorageAction : BaseBuildFilterAction
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
            
            return true;
        }


        public override void Execute(IFilter filter, IPipelineInput input)
        {
            Logger.Info($"Start construct the files that needed to upload !");

            this.ConstructUploadRes(filter,input);

            this.State = ActionState.Completed;
        }


        private string GetOutputAssetbundleFolder()
        {
            string result = string.Concat(System.Environment.CurrentDirectory,
                Path.DirectorySeparatorChar,
                AppBuildContext.Temporary,
                Path.DirectorySeparatorChar,
                AppBuildContext.AbExportFolder);
            return EditorUtils.OptimazePath(result);
        }

        private string GetAssetbundleManifestFilePath()
        {
            var abDir = GetOutputAssetbundleFolder();
            var manifestPath = string.Concat(abDir,
                Path.DirectorySeparatorChar,
                AppBuildContext.AbExportFolder);
            return EditorUtils.OptimazePath(manifestPath);
        }

        private void ConstructUploadRes(IFilter filter, IPipelineInput input)
        {
            var resStorage = CreateStorageDir();

            CopyAssetBundlesToUploadStorage(resStorage);
        }


        private string CreateStorageDir()
        {
            var resStorage = AppBuildContext.GetResStoragePath();
            if (Directory.Exists(resStorage))
            {
                Directory.Delete(resStorage, true);
            }

            AssetDatabase.Refresh();
            Directory.CreateDirectory(resStorage);
            return resStorage;
        }

        private void CopyAssetBundlesToUploadStorage(string resStorage)
        {
            var abDir = GetOutputAssetbundleFolder();
            var manifestPath = GetAssetbundleManifestFilePath();
            Logger.Info($"The assetbundle manifest path : {manifestPath}!");
            byte[] bytes = File.ReadAllBytes(manifestPath);
            var mainAb = AssetBundle.LoadFromMemory(bytes);
            var mainAbManifest = mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            mainAb.Unload(false);

            var abNames = mainAbManifest.GetAllAssetBundles();

            
            List<ABTableItem> list = new List<ABTableItem>();
            foreach (var abName in abNames)
            {
                var srcAbPath = $"{abDir}/{abName}";
                var desPath = $"{resStorage}/{abName}.x";
                var desDirPath = Path.GetDirectoryName(desPath);
                if (!Directory.Exists(desDirPath))
                {
                    Directory.CreateDirectory(desDirPath);
                }
                File.Copy(srcAbPath, desPath, true);
                ABTableItem item = new ABTableItem();
                item.Name = abName.Replace(AppBuildContext.ResExt, "");
                List<string> dependencies = new List<string>();
                foreach (var dependencie in mainAbManifest.GetAllDependencies(item.Name))
                {
                    dependencies.Add(dependencie.Replace(AppBuildContext.ResExt,""));
                }

                item.Dependencies = dependencies.ToArray();
                list.Add(item);
            }

            Logger.Info("Start craete target custom assetbundle manifest .");
            string targetPath = $"{resStorage}/{AssetsFileSystem.UnityABFileName}";
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            ResManifest resManifest = new ResManifest();
            resManifest.Tables = list.ToArray();
            var json = AppBuildContext.ToJson(resManifest);
            File.WriteAllText(targetPath, json,AppBuildContext.TextEncoding);
            Logger.Info("Create target custom manifest completed!");

            AssetDatabase.Refresh();
            Logger.Debug("Copy assetbundles completed!");
        }

        #endregion

    }
}