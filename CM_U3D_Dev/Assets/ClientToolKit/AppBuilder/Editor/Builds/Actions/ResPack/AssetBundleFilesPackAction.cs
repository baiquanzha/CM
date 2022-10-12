using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Text;
using MTool.AppBuilder.Editor.Builds.Contexts;
using MTool.AppUpdaterLib.Runtime;
using MTool.AssetBundleManager.Runtime;
using UnityEditor;
using UnityEngine;
using MTool.Core.Pipeline;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class AssetBundleFilesPackAction : BaseBuildFilterAction
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
            this.createList(filter,input);

            this.State = ActionState.Completed;
        }

        private void createList(IFilter filter, IPipelineInput input)
        {
            var appBuildContext = AppBuildContext;
            //appBuildContext.currentVer = string.Concat(appBuildContext.VersionStar, DateTime.Now.ToString(appBuildContext.VersionFormat));
            DirectoryInfo disk = new DirectoryInfo(string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, appBuildContext.Temporary, Path.DirectorySeparatorChar, appBuildContext.AbExportFolder));
            FileInfo manifest = new FileInfo(string.Concat(disk.FullName, Path.DirectorySeparatorChar, appBuildContext.AbExportFolder, appBuildContext.PatternManifest));

            if (disk.Exists && manifest.Exists)
            {
                FileInfo manifestFile = new FileInfo(string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, appBuildContext.Temporary,
                    Path.DirectorySeparatorChar, appBuildContext.AbExportFolder, Path.DirectorySeparatorChar
                    , appBuildContext.AbExportFolder));
                Debug.Log($"manifest full path : {manifestFile.FullName}");
                AssetBundleManifest mainAbManifest = null;
                AssetBundle mainAb = null;
                if (manifest.Exists)
                {
                    byte[] bytes = File.ReadAllBytes(manifestFile.FullName);
                    mainAb = AssetBundle.LoadFromMemory(bytes);
                    mainAbManifest = mainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                    mainAb.Unload(false);
                }

                int pathLength = disk.FullName.Length;
                string targetPath = AppBuildContext.GetAssetsOutputPath();

                Logger.Info($"Output path : {targetPath} .");
                if (!Directory.Exists(targetPath))
                {
                    Directory.CreateDirectory(targetPath);
                }
                List<ABTableItem> list = new List<ABTableItem>();
                List<FileDesc> infoList = new List<FileDesc>();

                //收集AssetBundle里的文件信息
                Dictionary<string, bool> clearList = clearFilesGet(disk, manifest);
                List<FileInfo> files = getFiles(filter,input,manifest, clearList);
                clearFilesRemove(clearList, new[] { "", appBuildContext.PatternManifest });
                clearStreamingAssets(targetPath);
                for (int i = 0; i < files.Count; i++)
                {
                    FileInfo tmpFile = files[i];
                    readManifest(filter,input,tmpFile, list, infoList, pathLength, mainAbManifest);
                    copyFile(filter, input, tmpFile, disk.FullName, targetPath);
                }

                AssetDatabase.Refresh();

                try
                {
                    string unityResPath = appBuildContext.GetAssetsBundleResManifestPath();
                    ResManifest resManifest = new ResManifest();
                    resManifest.Tables = list.ToArray();
                    var json = appBuildContext.ToJson(resManifest);                    
                    File.WriteAllBytes(unityResPath, appBuildContext.TextEncoding.GetBytes(json));

                    AssetDatabase.Refresh();
                    Debug.Log("保存Unity资源清单成功！");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("保存设置失败，异常信息：" + ex.Message + "！异常堆栈： " + ex.StackTrace);
                }
            }
        }

        private Dictionary<string, bool> clearFilesGet(DirectoryInfo folder, FileInfo manifest)
        {
            FileInfo[] files = folder.GetFiles("*.*", SearchOption.AllDirectories);
            Dictionary<string, bool> clearList = new Dictionary<string, bool>();
            for (int i = 0; i < files.Length; i++)
            {
                FileInfo info = files[i];
                clearList[info.FullName] = true;
            }

            clearFilesCheck(clearList, manifest);
            return clearList;
        }

        private void clearFilesCheck(Dictionary<string, bool> clearList, FileInfo file)
        {
            if (clearList.ContainsKey(file.FullName))
            {
                clearList.Remove(file.FullName);
                string Ext = Path.GetExtension(file.FullName);
                string sourceFile = file.FullName.Substring(0, file.FullName.Length - Ext.Length);
                if (clearList.ContainsKey(sourceFile))
                {
                    clearList.Remove(sourceFile);
                }
            }
        }

        private List<FileInfo> getFiles(IFilter filter, IPipelineInput input , FileInfo manifest, Dictionary<string, bool> clearList)
        {
            var appBuildContext = AppBuildContext;
            List<FileInfo> files = new List<FileInfo>();
            string abPath = Path.GetDirectoryName(manifest.FullName);
            using (TextReader textReader = new StreamReader(manifest.OpenRead(), Encoding.UTF8))
            {
                while (true)
                {
                    string line = textReader.ReadLine();
                    if (line == null)
                        break;
                    int index = line.IndexOf(appBuildContext.ManifestFineTitle, StringComparison.Ordinal);
                    if (index > -1)
                    {
                        line = line.Substring(index + appBuildContext.ManifestFineTitle.Length);
                        FileInfo tmpFile = new FileInfo(string.Concat(abPath, Path.DirectorySeparatorChar, line, appBuildContext.PatternManifest));
                        if (tmpFile.Exists)
                        {
                            files.Add(tmpFile);
                            clearFilesCheck(clearList, tmpFile);
                        }
                    }
                }
            }
            return files;
        }
        private void clearFilesRemove(Dictionary<string, bool> clearList, string[] searchList)
        {
            foreach (var VARIABLE in clearList)
            {
                string flie = VARIABLE.Key;
                bool delete = false;
                for (int i = 0; i < searchList.Length; i++)
                {
                    if (Path.GetExtension(flie) == searchList[i])
                    {
                        delete = true;
                        break;
                    }
                }
                if (delete)
                {
                    File.Delete(flie);
                }
            }
        }

        private void clearStreamingAssets(string folder)
        {
            DirectoryInfo disk = new DirectoryInfo(folder);
            if (disk.Exists)
            {
                FileInfo[] files = disk.GetFiles("*.*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo info = files[i];
                    if (info.Extension == AbHelp.AbFileExt)
                    {
                        info.Delete();
                    }
                    else if (info.Extension == ".meta")
                    {
                        string sourceFile = info.FullName.Substring(0, info.FullName.Length - info.Extension.Length);
                        if (Path.GetExtension(sourceFile) == AbHelp.AbFileExt)
                        {
                            info.Delete();
                        }
                    }
                }
            }
        }

        private void readManifest(IFilter filter, IPipelineInput input , FileInfo tmpFile, List<ABTableItem> list, List<FileDesc> infoList, int pathLength, AssetBundleManifest mainAbManifest)
        {
            var appBuildContext = AppBuildContext;
            ABTableItem item = new ABTableItem();
            FileDesc itemInfo = new FileDesc();
            int index = tmpFile.FullName.LastIndexOf(".", StringComparison.Ordinal);
            FileInfo abFileInfo = new FileInfo(tmpFile.FullName.Substring(0, index));
            index = abFileInfo.Name.LastIndexOf(".", StringComparison.Ordinal);
            if (abFileInfo.Exists)
            {
                itemInfo.S = (int)abFileInfo.Length;
                itemInfo.H = EditorUtils.GetMD5(abFileInfo.FullName);
            }
            //string abName = abFileInfo.Name.Substring(0, index == -1 ? abFileInfo.Name.Length : index);
            abFileInfo = null;
            int tLen = tmpFile.FullName.Length - pathLength - appBuildContext.PatternManifest.Length;
            string rName = tmpFile.FullName.Substring(pathLength + 1, tLen);
            index = rName.LastIndexOf(".", StringComparison.Ordinal);
            if (index > -1)
            {
                rName = rName.Substring(0, index);
            }
            item.Name = rName.Replace(Path.DirectorySeparatorChar, '/');

            itemInfo.N = item.Name;

            int pathStar = item.Name.LastIndexOf("/", StringComparison.Ordinal);
            string miniName = pathStar == -1 ? item.Name : item.Name.Substring(pathStar);

            bool isHash = isHexadecimal(miniName) && miniName.Length == 32;

            //List<string> assets = new List<string>();
            //List<string> dependencies = new List<string>();
            using (TextReader textReader = new StreamReader(tmpFile.OpenRead(), Encoding.UTF8))
            {
                byte state = 0;
                while (true)
                {
                    string line = textReader.ReadLine();
                    if (line == null)
                        break;
                    else
                    {
                        if (string.CompareOrdinal(line, appBuildContext.AssetsTitle) == 0)
                        {
                            state = 1;
                            continue;
                        }
                        else if (string.CompareOrdinal(line, appBuildContext.DependenciesTitle) == 0)
                        {
                            state = 2;
                            continue;
                        }
                        else if (string.CompareOrdinal(line, appBuildContext.DependenciesTitleError) == 0)
                        {
                            state = 0;
                            break;
                        }
                        switch (state)
                        {
                            case 1:
                                {/*
                                    if (!isHash)
                                    {
                                        string str = line.Substring(2);

                                        //if (item.AbsPath == null)
                                        //{
                                        //    //string indexStr = string.Concat("/", abName);
                                        //    pathStar = str.IndexOf(item.Name, StringComparison.OrdinalIgnoreCase) + item.Name.Length;
                                        //    item.AbsPath = str.Substring(0, pathStar);
                                        //}

                                        if (pathStar < str.Length)
                                        {
                                            int extIndex = line.LastIndexOf(".", StringComparison.Ordinal);
                                            if (extIndex > -1)
                                            {
                                                string extFile = line.Substring(extIndex);
                                                //去掉.cginc文件
                                                if (!AbHelp.CheckFileExt(extFile))
                                                {
                                                    assets.Add(str.Substring(pathStar));
                                                }
                                            }
                                        }
                                        else
                                        {
                                            Debug.LogError("readManifest:" + tmpFile.FullName + "," + line);
                                        }
                                    }*/
                                    break;
                                }
                            case 2:
                                {
                                    //                                string dependenc = line.Substring(pathLength + 3);
                                    //                                index = dependenc.LastIndexOf(".", StringComparison.Ordinal);
                                    //                                if (index > -1)
                                    //                                    dependenc = dependenc.Substring(0, index);
                                    //                                dependencies.Add(dependenc);
                                    break;
                                }
                        }
                    }
                }
                textReader.Close();
            }

            //if (assets.Count > 0)
            //{
            //    item.Assets = assets.ToArray();
            //}
            //if (dependencies.Count > 0)
            //item.Dependencies = dependencies.ToArray();
            item.Dependencies = mainAbManifest.GetAllDependencies(item.Name);
            list.Add(item);
            infoList.Add(itemInfo);
        }

        private bool isHexadecimal(string str)
        {
            const string PATTERN = @"^[A-Fa-f0-9]+$";
            return System.Text.RegularExpressions.Regex.IsMatch(str, PATTERN);
        }

        private void copyFile(IFilter filter, IPipelineInput input ,FileInfo tmpFile, string sourcrPath, string targetPtah)
        {
            var appBuildContext = AppBuildContext;
            string extPath = tmpFile.FullName.Substring(sourcrPath.Length + 1);
            extPath = extPath.Substring(0, extPath.Length - appBuildContext.PatternManifest.Length);
            string[] extPaths = extPath.Split(new char[] { Path.DirectorySeparatorChar });

            int count = extPaths.Length - 1;
            if (count >= 0)
            {
                for (int i = 0; i < count; i++)
                {
                    targetPtah = string.Concat(targetPtah, Path.DirectorySeparatorChar, extPaths[i]);
                    if (!Directory.Exists(targetPtah))
                    {
                        Directory.CreateDirectory(targetPtah);
                    }
                }
                targetPtah = string.Concat(targetPtah, Path.DirectorySeparatorChar, extPaths[count]);
            }
            targetPtah = string.Concat(targetPtah, AbHelp.AbFileExt);
            string sourcePath = string.Concat(sourcrPath, Path.DirectorySeparatorChar, extPath);
            File.Copy(sourcePath, targetPtah, true);
        }

        #endregion

    }
}
