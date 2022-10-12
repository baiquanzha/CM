using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Channels;
using MTool.AppBuilder.Editor.Builds.Contexts;
using UnityEngine;
using MTool.Core.Pipeline;
using MTool.Editor.UploadUtilitis.WinSCP;
using MTool.AppBuilder.Editor.UploadUtilitis.AmazonS3;
using UnityEditor;
using Debug = UnityEngine.Debug;
using File = MTool.Core.IO.File;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class UploadFilesAction : BaseBuildFilterAction
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
            string pythonScripPath = $"{Application.dataPath}/../Tools/ProtokitUpload/ProtokitGoUploader.py";

            pythonScripPath = EditorUtils.OptimazePath(pythonScripPath);
            Logger.Info($"Lua projecet root path : {pythonScripPath} .");

            if (!File.Exists(pythonScripPath))
            {
                AppBuildContext.ErrorSb.AppendLine($"The target upload script that path is \"{pythonScripPath}\" is not exist!");
                return false;
            }
            
            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            var resStorage = AppBuildContext.GetResStoragePath();

            Logger.Info($"Start upload files , path is \" {resStorage}\" .");

            var result = this.UploadFiles(resStorage,input);
            if (result)
            {
                this.State = ActionState.Completed;
            }
            else
            {
                this.State = ActionState.Error;
            }
        }

        private bool UploadFiles(string sourceFolder, IPipelineInput input)
        {
            string pythonScripPath = $"{Application.dataPath}/../Tools/ProtokitUpload/ProtokitGoUploaderExt.py";
            
            pythonScripPath = EditorUtils.OptimazePath(pythonScripPath);
            Logger.Info($"Lua projecet root path : {pythonScripPath} .");

            var appBuildConfig = AppBuildConfig.GetAppBuildConfigInst();
            var configRepoPath = appBuildConfig.GameTableDataConfigPath;
            if (!Directory.Exists(configRepoPath))
            {
                throw new DirectoryNotFoundException(configRepoPath);
            }

            var protokitgoConfigName = appBuildConfig.upLoadInfo.protokitgoConfigName;

            var uploadFolder = sourceFolder;

            string platformName = string.Empty;
#if UNITY_EDITOR && UNITY_ANDROID
            platformName = "android";
#elif UNITY_EDITOR && UNITY_IPHONE
            platformName = "ios";
#else
            throw new InvalidOperationException($"Unsupport build platform : {EditorUserBuildSettings.activeBuildTarget} .");
#endif

            var uploadInfo = AppBuildConfig.GetAppBuildConfigInst().upLoadInfo;
            var uploadFilesPattern = uploadInfo.uploadFilesPattern;

            var makeBaseVersion = input.GetData(EnvironmentVariables.MAKE_BASE_APP_VERSION_KEY, false);
            var appVersion = AppBuildContext.GetTargetAppVersion(makeBaseVersion);
            
            var resVersion = appVersion.Patch;

            var noUpload = "false";
            if (uploadInfo.isUploadToRemote)
                noUpload = "false";
            else
                noUpload = "true";

            string remoteDir = uploadInfo.remoteDir; 
            if (string.IsNullOrEmpty(remoteDir) || string.IsNullOrWhiteSpace(remoteDir))
            {
                remoteDir = "**NOROOT**";
            }

            var vscTypeStr = appBuildConfig.repositoryInfo.vcsType == VcsType.Git ? "GIT" : "SVN";
            string commandLineArgs =
                $"{pythonScripPath} {vscTypeStr} {configRepoPath} {protokitgoConfigName} {platformName} {uploadFilesPattern} {uploadFolder} {remoteDir} {appVersion.Major}.{appVersion.Minor} {resVersion} {noUpload}";

            
            Debug.Log($"commandline args : {commandLineArgs}");

            var pStartInfo = new ProcessStartInfo();

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

            pStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            pStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;

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
                Logger.Debug("Upload files successful!");
            }
            proces.Close();

            AssetDatabase.Refresh();

            return exitCode == 0;
        }


        #endregion

    }
}
