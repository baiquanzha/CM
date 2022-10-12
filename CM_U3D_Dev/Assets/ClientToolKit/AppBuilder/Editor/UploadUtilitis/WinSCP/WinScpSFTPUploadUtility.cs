using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ServerModel.Models;
using System.IO;
using MTool.AppBuilder.Editor;
using Debug = UnityEngine.Debug;

namespace MTool.Editor.UploadUtilitis.WinSCP
{
    public static class WinScpSFTPUploadUtility
    {
        public static string TARGET_UPLOAD_ENGINE_PATH => 
            Path.GetFullPath(Application.dataPath + WinSCPConfig.GetInstance().targetUploadEngineRelativePath);

        public static string TARGET_UPLOAD_ENGINE_WORKINGDIRECTORY_PATH =>
            Path.GetFullPath(Application.dataPath + WinSCPConfig.GetInstance().targetUploadEngineWorkDirectoryRelativePath);

        public static void Start(string sourceDir)
        {
            string serverInfoName = WinSCPConfig.GetInstance().serverInfoName.Trim();

            string commandLineArgs = string.Format("{0}={1} {2}={3} {4}={5}",
                EnvironmentVariables.SERVER_INFO_NAME_KEY, serverInfoName,
                EnvironmentVariables.SOURCE_DIR_KEY, sourceDir,
                EnvironmentVariables.PLATFORM_KEY, AbHelp.GetPlatformNameNoSlash());


            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = TARGET_UPLOAD_ENGINE_PATH;

            pStartInfo.UseShellExecute = false;

            pStartInfo.RedirectStandardInput = false;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;
            pStartInfo.WorkingDirectory = TARGET_UPLOAD_ENGINE_WORKINGDIRECTORY_PATH;

            pStartInfo.CreateNoWindow = false;
            pStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            pStartInfo.Arguments = commandLineArgs;

            pStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            pStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;

            var proces = Process.Start(pStartInfo);

            string standardOutput = proces.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(standardOutput))
                UnityEngine.Debug.Log(standardOutput);

            string standardErroOutput = proces.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(standardErroOutput))
                UnityEngine.Debug.LogError(standardErroOutput);

            proces.WaitForExit();
            proces.Close();
            Debug.Log("Upload successful!");

        }


        public static void StartUploadTask(string sourceDir, List<UploadTask> files, string version)
        {

            if (files == null || files.Count == 0)
            {
                Debug.LogWarning("Current not has the files needed to uploaded!");
                return;
            }
            //Test
            SFTPUpLoadFilesInfo info = new SFTPUpLoadFilesInfo();
            var sourceFiles = new List<string>();
            var targetFiles = new List<string>();
            foreach (var file in files)
            {
                //if (file.IsFolder)
                //    continue;
                sourceFiles.Add(file.Source);
                targetFiles.Add(file.Target);
            }
            info.SourceFiles = sourceFiles.ToArray();
            info.TargetFiles = targetFiles.ToArray();

            string yaml = YamlDotNet.YAMLSerializationHelper.Serialize(info);
            string needUploadFilesName = string.Format("{0}_needUploadFiles.txt", version);
            File.WriteAllText(sourceDir + "/" + needUploadFilesName, yaml, System.Text.Encoding.UTF8);

            string serverInfoName = WinSCPConfig.GetInstance().serverInfoName.Trim();

            string commandLineArgs = string.Format("{0}={1} {2}={3} {4}={5} {6}={7} {8}={9}",
                EnvironmentVariables.SERVER_INFO_NAME_KEY, serverInfoName,
                EnvironmentVariables.PLATFORM_KEY, AbHelp.GetPlatformNameNoSlash(),
                EnvironmentVariables.SOURCE_DIR_KEY, sourceDir,
                EnvironmentVariables.ASSET_VERSION_KEY, version,
                EnvironmentVariables.NEEDED_UPLOAD_LIST_NAME_KEY, needUploadFilesName
                );


            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = TARGET_UPLOAD_ENGINE_PATH;

            pStartInfo.UseShellExecute = false;

            pStartInfo.RedirectStandardInput = false;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;
            pStartInfo.WorkingDirectory = TARGET_UPLOAD_ENGINE_WORKINGDIRECTORY_PATH;

            pStartInfo.CreateNoWindow = false;
            pStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            pStartInfo.Arguments = commandLineArgs;

            pStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            pStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;

            var proces = Process.Start(pStartInfo);

            string standardOutput = proces.StandardOutput.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(standardOutput))
                UnityEngine.Debug.Log(standardOutput);

            string standardErroOutput = proces.StandardError.ReadToEnd();
            if (!string.IsNullOrWhiteSpace(standardErroOutput))
                UnityEngine.Debug.LogError(standardErroOutput);

            proces.WaitForExit();
            proces.Close();
            Console.WriteLine("Start process successful!");

        }
    }
}
