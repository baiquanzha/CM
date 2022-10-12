using MTool.AppUpdaterLib.Runtime;
using MTool.Core.Pipeline;
using MTool.Core.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class ConstructStreamingAssetsAction : BaseBuildFilterAction
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


        private string GetUploadDir()
        {
            return AppBuildContext.GetResStoragePath();
        }

        public override bool Test(IFilter filter, IPipelineInput input)
        {


            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            CopyAssetsToStreamingAssetsPath();

            this.State = ActionState.Completed;
        }


        private void CopyAssetsToStreamingAssetsPath()
        {
            CopyGameResources();

            Logger.Info("Copy game resources to streaming assets path !");
        }


        private void CopyGameResources()
        {
            var streamingPath = AppBuildContext.GetAssetsOutputPath();
            if (Directory.Exists(streamingPath))
            {
                Directory.Delete(streamingPath, true);
            }
            AssetDatabase.Refresh();
            Directory.CreateDirectory(streamingPath);

            VersionManifest versionManifest = new VersionManifest();
            string[] splitStrs = { "UpLoadRes/" };
            string platformName = string.Empty;
#if UNITY_EDITOR && UNITY_ANDROID
            platformName = "android";
#elif UNITY_EDITOR && UNITY_IPHONE
            platformName = "ios";
#else
            throw new InvalidOperationException($"Unsupport build platform : {EditorUserBuildSettings.activeBuildTarget} .");
#endif

            var resStorage = AppBuildContext.GetResStoragePath();
            DirectoryInfo dirInfo = new DirectoryInfo(resStorage);
            FileInfo[] fileInfos = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
            Logger.Info("Start copy game resources to streaming assets path !");
            foreach (var fileInfo in fileInfos)
            {
                string sourcePath = EditorUtils.OptimazePath(fileInfo.FullName);
                if (sourcePath.Contains(AppBuildContext.GenCodePattern))
                {
                    continue;
                }

                if (sourcePath.Contains("protokitgo.yaml"))
                {
                    continue;
                }

                if (sourcePath.Contains("resource_versions.release"))
                {
                    continue;
                }

                string targePath = $"{streamingPath}{sourcePath.Replace(resStorage, "")}";

                string dirName = Path.GetDirectoryName(targePath);

                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                File.Copy(sourcePath, targePath, true);

                Debug.Log("-------------sourcePath = " + sourcePath);
                //res
                string resPath = sourcePath.Split(splitStrs, StringSplitOptions.None)[1];
                FileDesc fd = new FileDesc();
                fd.S = (int)fileInfo.Length;
                fd.H = CryptoUtility.GetMD5(fileInfo);
                fd.RN = $"resource/{resPath}#{fd.S}#{fd.H}";
                fd.N = resPath;
                versionManifest.Datas.Add(fd);
            }
            Logger.Info("Copy game resources  completed!");

            var json = Serialize(versionManifest);
            string resTargetPath = $"{streamingPath}/res_{platformName}.json";
            File.WriteAllText(resTargetPath, json, AppBuildContext.TextEncoding);
        }

        public string Serialize(VersionManifest manifest) {
            var doc = new JSONObject();

            for (int i = 0; i < manifest.Datas.Count; i++) {
                var desc = manifest.Datas[i];
                doc.AddField(desc.N, desc.RN);
            }

            return doc.ToString(true);
        }

        #endregion
    }
}