using System;
using System.IO;
using MTool.Core.Pipeline;
using System.Collections.Generic;
using MTool.AppUpdaterLib.Runtime;
using MTool.AppBuilder.Editor.Builds.Contexts;
using UnityEngine;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class FileVersionManifestGenerateAction : BaseBuildFilterAction
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

        #endregion

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.GenerateVersionManifestFile(filter,input);
            this.State = ActionState.Completed;
        }

        /// <summary>
        /// 过滤以以下字符串为后缀的文件
        /// </summary>
        private static string[] ignoreSuffixs = { ".meta", ".log" };

        private static string[] ignoreFileNames = { "desc.txt", "res_android.json", "res_ios.json" };

        private static string[] ignoreFolderNames = { "Conf"};


        private void GenerateVersionManifestFile(IFilter filter, IPipelineInput input)
        {
            var appBuildContext = AppBuildContext;
            string streamFolder = string.Concat(System.Environment.CurrentDirectory
                , Path.DirectorySeparatorChar
                , appBuildContext.StreamingAssetsFolder);

            streamFolder = EditorUtils.OptimazePath(streamFolder);
            DirectoryInfo dirInfo = new DirectoryInfo(streamFolder);

            FileInfo[] fileInfos = dirInfo.GetFiles("*.*", SearchOption.AllDirectories);
            List<FileDesc> fileDescList = new List<FileDesc>();
            foreach (var fileInfo in fileInfos)
            {
                if (IsFileBeIgnore(fileInfo, streamFolder))
                    continue;

                FileDesc fileDesc = new FileDesc();
                fileDesc.S = (int)fileInfo.Length;
                fileDesc.H = EditorUtils.GetMD5(fileInfo.FullName);

                string fileFullName = EditorUtils.OptimazePath(fileInfo.FullName);
                int idx = fileFullName.LastIndexOf(streamFolder, StringComparison.Ordinal);
                string name = fileFullName.Substring(idx + streamFolder.Length + 1);
                fileDesc.N = name;
                fileDescList.Add(fileDesc);
            }
            
            appBuildContext.VersionManifest.Datas = fileDescList;

            //string json = JsonUtility.ToJson(appBuildContext.VersinManifest,true);

            //File.WriteAllText(Application.dataPath+"/../test.json",json,new System.Text.UTF8Encoding(false,true));
            //Debug.Log("Write test.json successful!");
        }
        

        private bool IsFileBeIgnore(FileInfo fileInfo,string rootPath)
        {
            string fileFullName = EditorUtils.OptimazePath(fileInfo.FullName);
            foreach (var ignoreSuffix in ignoreSuffixs)
            {
                if (fileFullName.EndsWith(ignoreSuffix))
                {
                    return true;
                }
            }
            foreach (var ignoreFileName in ignoreFileNames)
            {
                if (String.Compare(fileInfo.Name, ignoreFileName, StringComparison.Ordinal) == 0)
                {
                    return true;
                }
            }
            foreach (var ignoreFolderName in ignoreFolderNames)//剔除表数据资源
            {
                string folderPath = Path.GetDirectoryName(fileInfo.FullName);
                folderPath = EditorUtils.OptimazePath(folderPath);
                folderPath = folderPath.Replace(rootPath + "/", "");
                if (folderPath.StartsWith(ignoreFolderName + "/"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
