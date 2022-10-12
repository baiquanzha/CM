using System;
using System.Diagnostics;
using MTool.Core.Pipeline;
using UnityEngine;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

// ReSharper disable once CheckNamespace
namespace MTool.AppBuilder.Editor.Builds.Actions.ResProcess
{
    public class GameTableDataLocalRepositoryGenerateBuildAction : BaseBuildFilterAction
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
            var appBuildConfig = AppBuildConfig.GetAppBuildConfigInst();
            if (string.IsNullOrEmpty(appBuildConfig.repositoryInfo.gameTableDataRepositoryRemotePath))
            {
                AppBuildContext.ErrorSb.AppendLine($"The gameTableDataRepositoryRemotePath is not set!");
            }

            if (string.IsNullOrEmpty(appBuildConfig.repositoryInfo.gameTableDataRepositoryLocalDirName))
            {
                AppBuildContext.ErrorSb.AppendLine($"The gameTableDataRepositoryLocalDirName is not set!");
            }

            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.UpdateRepository(filter,input);
            this.State = ActionState.Completed;
        }


        private bool UpdateRepository(IFilter filter, IPipelineInput input)
        {
            string pythonScripPath = $"{Application.dataPath}/../Tools/GameTableDataRepository/DataRepositoryGenerator.py";

            pythonScripPath = EditorUtils.OptimazePath(pythonScripPath);
            Logger.Info($"Lua projecet root path : {pythonScripPath} .");

            var appBuildConfig = AppBuildConfig.GetAppBuildConfigInst();
            var configRepoPath = appBuildConfig.GameTableDataConfigPath;
            
            var repositoryInfo = AppBuildConfig.GetAppBuildConfigInst().repositoryInfo;
            var vscTypeStr = repositoryInfo.vcsType == VcsType.Git ? "GIT" : "SVN";

            var _localDirName = repositoryInfo.gameTableDataRepositoryLocalDirName.Trim();
            var localDirName = string.IsNullOrEmpty(_localDirName) ? "conf" : _localDirName;

            var branchName = input.GetData<string>(EnvironmentVariables.GAME_TABLE_DATA_REPOSITORY_BRANCH_NAME,"");
            if (string.IsNullOrEmpty(branchName))
            {
                branchName = repositoryInfo.branchName.Trim();
            }
            if (string.IsNullOrEmpty(branchName))
            {
                branchName = "None";
            }

            string commandLineArgs =
                $"{pythonScripPath} {vscTypeStr} {repositoryInfo.gameTableDataRepositoryRemotePath} {branchName} {appBuildConfig.GameTableDataConfigParentPath} {localDirName}";


            Debug.Log($"commandline args : {commandLineArgs}");

            var pStartInfo = new ProcessStartInfo();

            var uploadInfo = appBuildConfig.upLoadInfo;
#if UNITY_EDITOR_WIN
            if (uploadInfo.pythonType == FilesUpLoadInfo.PythonType.Python)
            {
                pStartInfo.FileName = @"python.exe";
            }
            else
            {
                pStartInfo.FileName = @"python3.exe";
            }
#elif UNITY_EDITOR_OSX
            if (uploadInfo.pythonType == FilesUpLoadInfo.PythonType.Python)
            {
                pStartInfo.FileName = @"python";
            }
            else
            {
                pStartInfo.FileName = @"python3";
            }
#else
        throw new InvalidOperationException($"Unsupport build platform : {EditorUserBuildSettings.activeBuildTarget} .");
#endif


            pStartInfo.UseShellExecute = false;

            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;
            var workDir = Path.GetDirectoryName(pythonScripPath);
            workDir = EditorUtils.OptimazePath(workDir);
            pStartInfo.WorkingDirectory = workDir;

            pStartInfo.CreateNoWindow = true;
            pStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            pStartInfo.Arguments = commandLineArgs;

            pStartInfo.StandardErrorEncoding = System.Text.Encoding.UTF8;
            pStartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;

            var proces = Process.Start(pStartInfo);
            proces.ErrorDataReceived += (s, e) =>
            {
                Logger.Info(e.Data);
            };
            proces.OutputDataReceived += (s, e) =>
            {
                Logger.Debug(e.Data);
            };
            proces.BeginOutputReadLine();
            proces.BeginErrorReadLine();
            proces.WaitForExit();
            var exitCode = proces.ExitCode;
            if (exitCode != 0)
            {
                AppBuildContext.AppendErrorLog($"Exit code : {proces.ExitCode}!");
                Logger.Error($"Exit code : {proces.ExitCode}!");
            }
            else
            {
                Logger.Debug("Update repository successful!");
            }
            proces.Close();

            AssetDatabase.Refresh();

            return exitCode == 0;
        }


        #endregion
    }
}
