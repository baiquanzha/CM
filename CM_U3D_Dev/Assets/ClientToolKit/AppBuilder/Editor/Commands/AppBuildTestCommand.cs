using UnityEditor;
using MTool.AppBuilder.Editor.Builds;
using MTool.AppBuilder.Editor.Builds.Actions.ResProcess;
using MTool.Core.Pipeline;
using MTool.AppBuilder.Editor.Builds.Filters.Concrete;
using MTool.AppBuilder.Editor.Builds.PipelineInputs;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Commands
{
    public static class AppBuildTestCommand
    {
        //[MenuItem("MTool/AppBuild/Commands/Test Build App")]
        static void TestBuild()
        {
            AppBuilderPipelineProcessor processor = new AppBuilderPipelineProcessor();

            ResourcesProcessFilter resourcesProcessFilter = new ResourcesProcessFilter(true);
            ResourcesPackageFilter resourcesPackageFilter = new ResourcesPackageFilter(true);

            processor.Register(resourcesProcessFilter);
            processor.Register(resourcesPackageFilter);

            AppBuildPipelineInput input = new AppBuildPipelineInput();
            var result = processor.Process(input);

            if (result.State == ProcessState.Error)
            {
                Debug.LogError($"Build app failure , error message : {result.Message}");
            }
            else
            {
                Debug.Log("Build app completed!");
            }
            
        }

        //[MenuItem("Commands/Test copy rules")]
        static void TestCopyRules()
        {
            AppBuilderPipelineProcessor processor = new AppBuilderPipelineProcessor();

            ResourcesProcessFilter resourcesProcessFilter = new ResourcesProcessFilter(false);
            //resourcesProcessFilter.Enqueue(new TestOtherFilesExportAction());
            //ResourcesPackageFilter resourcesPackageFilter = new ResourcesPackageFilter(true);

            processor.Register(resourcesProcessFilter);
            //processor.Register(resourcesPackageFilter);

            AppBuildPipelineInput input = new AppBuildPipelineInput();
            var result = processor.Process(input);

            if (result.State == ProcessState.Error)
            {
                Debug.LogError($"Build app failure , error message : {result.Message}");
            }
            else
            {
                Debug.Log("Build app completed!");
            }

        }

        //[MenuItem("test/copy standard assets")]
        static void CopyStandardAssetFolder()
        {
            string sourcePath = EditorUtils.OptimazePath(Application.dataPath+ "/Standard Assets");

            string targetPath = EditorUtils.OptimazePath(@"F:\MToolGitRepositories\Standard Assets");


            EditorUtils.CopyDirecotryToDestination(sourcePath,targetPath, x =>
            {
                if (x.EndsWith(".meta"))
                {
                    return true;
                }

                if (x.Contains(".get"))
                {
                    return true;
                }
                return false;
            });

            Debug.LogError("Copy standard asset completed!");
        }
    }
}
