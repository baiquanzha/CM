using System;
using System.IO;
using MTool.AppBuilder.Editor.Builds.BuildInfos;
using MTool.AppBuilder.Editor.Builds.Contexts;
using MTool.AppUpdaterLib.Runtime;
using MTool.AppUpdaterLib.Runtime.Configs;
using MTool.Core.Pipeline;
using UnityEditor;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class ExportBuildReportAction : BaseMakeVersionAction
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
            var buildReporterFolderPath = this.WriteBuildReporterFile(filter,input);

            this.CopyResManifessts(buildReporterFolderPath);

            this.State = ActionState.Completed;
        }

        private string WriteBuildReporterFile(IFilter filter, IPipelineInput input)
        {
            var reporter = new BuildReportInfo();

            var now = DateTime.Now;
            string timeStr = now.ToString("yyyy-MM-dd HH-mm-ss");
            reporter.meta.buildTime = timeStr;
            reporter.meta.unityVersion = Application.unityVersion;
            reporter.meta.machineName = System.Environment.MachineName;

            var activeBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            reporter.buildTarget = activeBuildTarget.ToString();

            var appBuildContext = AppBuildContext;
            reporter.unityResVerison = appBuildContext.AppInfoManifest.unityDataResVersion;
            reporter.dataResVersion = appBuildContext.AppInfoManifest.dataResVersion;
            var appUpdaterConfigText = Resources.Load<TextAsset>("appupdater");
            var appUpdaterConfig = JsonUtility.FromJson<AppUpdaterConfig>(appUpdaterConfigText.text);
            reporter.channel = appUpdaterConfig.channel;
            reporter.ossUrl = appUpdaterConfig.ossUrl;
            reporter.cdnUrl = appUpdaterConfig.cdnUrl;
            
            reporter.makeBaseVersion = input.GetData(EnvironmentVariables.MAKE_BASE_APP_VERSION_KEY, false);

            string json = appBuildContext.ToJson(reporter);

            string timeStr2 = now.ToString("yyyyMMddHHmmss");
            string targetPath = appBuildContext.GetBuildReportsPath($"{timeStr2}/Build.report");
            File.WriteAllText(targetPath,json, appBuildContext.TextEncoding);

            AssetDatabase.Refresh();
            Logger.Info("Write build report complted!");

            var result = Path.GetDirectoryName(targetPath);
            result = EditorUtils.OptimazePath(result);
            return result;
        }


        private void CopyResManifessts(string targetFolder)
        {
            //var sourceDataResPath = $"{AppBuildContext.GetAssetsOutputPath()}/res_data.json";
            //var desDataResListPath = $"{targetFolder}/res_data.json";

            //if (!File.Exists(sourceDataResPath))
            //{
            //    throw new FileNotFoundException($"The file path is \"{sourceDataResPath}\" .");
            //}

            //if (File.Exists(desDataResListPath))
            //{
            //    File.Delete(desDataResListPath);
            //}
            //File.Copy(sourceDataResPath, desDataResListPath);

            var platformName = string.Empty;
#if UNITY_EDITOR && UNITY_ANDROID
            platformName = "android";
#elif UNITY_EDITOR && UNITY_IPHONE
            platformName = "ios";
#else
            throw new InvalidOperationException($"Unsupport build platform : {EditorUserBuildSettings.activeBuildTarget} .");
#endif

            var sourceUnityaResPath = $"{AppBuildContext.GetAssetsOutputPath()}/res_{platformName}.json";
            var desUnityResListPath = $"{targetFolder}/res_{platformName}.json";

            if (!File.Exists(sourceUnityaResPath))
            {
                throw new FileNotFoundException($"The file path is \"{sourceUnityaResPath}\" .");
            }

            if (File.Exists(desUnityResListPath))
            {
                File.Delete(desUnityResListPath);
            }
            File.Copy(sourceUnityaResPath, desUnityResListPath);

            AssetDatabase.Refresh();

            Logger.Info("Copy resource manifest files complted!");
        }

        #endregion
    }
}
