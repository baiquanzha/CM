using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.AppBuilder.Editor.Builds;
using MTool.Editor;
using MTool.Editor.UploadUtilitis.WinSCP;
using UnityEngine;

namespace MTool.AppBuilder.Editor.UploadUtilitis.AmazonS3
{
    internal static class AmazonS3UploadUtility
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        //public static string TARGET_UPLOAD_ENGINE_PATH =>
        //    EditorUtils.OptimazePath(Application.dataPath + AppBuildConfig.GetAppBuildConfigInst().AmazonS3UpLoadEngineRelativeToAssetsPath);

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
        public static void Start(string sourceDir)
        {
            //string commandLineArgs = sourceDir;


            //var pStartInfo = new ProcessStartInfo();
            //pStartInfo.FileName = TARGET_UPLOAD_ENGINE_PATH;

            //pStartInfo.UseShellExecute = false;

            //pStartInfo.RedirectStandardInput = false;
            //pStartInfo.RedirectStandardOutput = true;
            //pStartInfo.RedirectStandardError = true;
            //var workingDirectory = Path.GetDirectoryName(TARGET_UPLOAD_ENGINE_PATH);
            //workingDirectory = EditorUtils.OptimazePath(workingDirectory);
            //pStartInfo.WorkingDirectory = workingDirectory;
            //pStartInfo.CreateNoWindow = false;
            //pStartInfo.WindowStyle = ProcessWindowStyle.Normal;
            //pStartInfo.Arguments = commandLineArgs;

            //pStartInfo.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
            //pStartInfo.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;

            //var proces = Process.Start(pStartInfo);

            //string standardOutput = proces.StandardOutput.ReadToEnd();
            //if (!string.IsNullOrWhiteSpace(standardOutput))
            //    UnityEngine.Debug.Log(standardOutput);

            //string standardErroOutput = proces.StandardError.ReadToEnd();
            //if (!string.IsNullOrWhiteSpace(standardErroOutput))
            //    UnityEngine.Debug.LogError(standardErroOutput);

            //proces.WaitForExit();
            //proces.Close();
            //UnityEngine.Debug.Log("Upload to amazon s3 successful!");
        }


        #endregion

    }
}
