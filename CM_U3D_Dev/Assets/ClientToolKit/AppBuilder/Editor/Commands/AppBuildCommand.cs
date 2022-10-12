using System.IO;
using MTool.AppBuilder.Editor.Builds;
using MTool.AppBuilder.Editor.Builds.Actions.ResPack;
using MTool.Core.Pipeline;
using MTool.AppBuilder.Editor.Builds.PipelineInputs;
using UnityEditor;
using UnityEngine;
using MTool.AppBuilder.Editor.Builds.Filters.Concrete;
using MTool.AppBuilder.Editor.Builds.Actions.ResProcess;
using MTool.AppBuilder.Editor.Builds.InnerLoggers;
using MTool.AppUpdaterLib.Runtime;
using MTool.LoggerModule.Runtime;

namespace MTool.AppBuilder.Editor.Commands
{
    internal static class AppBuildCommand
    {
        [MenuItem("MTool/AppBuilder/Commands/制作并上传Patch")]
        static void MakePatchResouces()
        {
            string configPath = AppBuildConfig.GetAppBuildConfigInst().AppBuildConfigFolder + "/MakePatchVersion.yaml";
            var result = RunPipeline(configPath);

            if (result.State == ProcessState.Error)
            {
                Debug.LogError($"Build app failure , error message : {result.Message}");
            }
            else
            {
                Debug.Log("Build app completed!");
            }
        }

        [MenuItem("MTool/AppBuilder/Commands/制作一个基础版本")]
        static void MakeBaseVersionResources()
        {
            string configPath = AppBuildConfig.GetAppBuildConfigInst().AppBuildConfigFolder + "/MakeBaseVersion.yaml";

            var result = RunPipeline(configPath);

            if (result.State == ProcessState.Error)
            {
                Debug.LogError($"Build app failure , error message : {result.Message}");
            }
            else
            {
                Debug.Log("Build app completed!");
            }
        }


        static ProcessResult RunPipeline(string pipelineConfigPath)
        {
            if (string.IsNullOrEmpty(pipelineConfigPath))
            {
                return ProcessResult.Create(ProcessState.Error,$"The pipeline config that path is \"{pipelineConfigPath}\" is not found!");
            }

            LoggerManager.SetCurrentLoggerProvider(new AppBuilderLoggerProvider());

            var processor = AppBuilderPipelineProcessor.ReadFromBuildProcessConfig(pipelineConfigPath);

            AppBuildPipelineInput input = new AppBuildPipelineInput();

            return processor.Process(input);
        }

        [MenuItem("MTool/AppBuilder/Commands/清理编译缓存")]
        static void ClearBuildCacheFolder()
        {
            string dir = AppBuildConfig.GetAppBuildConfigInst().GetBuildCacheFolderPath();
            dir = EditorUtils.OptimazePath(dir);

            if (Directory.Exists(dir))
                Directory.Delete(dir,true);

            Debug.Log("Clear build cache success !");
        }

    }
}
