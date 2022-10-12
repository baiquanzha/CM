using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Builds
{
    [Serializable]
    public class AppVersion
    {
        /// <summary>
        /// 主版本号(必须是数字)，具有如下特点：
        /// 1).游戏有重大功能性的更新或者认为强制定义该App的大版本号
        /// 2).游戏必须下载并安装新的安装包，API已无法兼容（产生了新的API或者API发生了变化，这里一般指C#代码或本地代码发生了重大变更导致
        /// 需要更新新的版本）
        /// </summary>
        public string Major = "0";

        /// <summary>
        /// 次版本号（必须是数字)，特点如下：
        /// 1).同上2，游戏必须下载并安装新的安装包
        /// </summary>
        public string Minor = "0";

        /// <summary>
        /// 修订号(必须是数字)
        /// 在主版本号和次版本号不变的情况下，API没有发生变化，但是修复了若干bug，此种情况下不需要更新安装包
        /// 对于使用Lua做开发语言的游戏来说，如果Native代码没有发生变化，理论上这个值会跟着升高
        /// </summary>
        public string Patch = "1";

        /// <summary>
        /// 版本号后缀
        /// </summary>
        public string VersionSuffix = "-alpha00";

        /// <summary>
        /// 版本编译信息
        /// </summary>
        public string BuildMetadata = "";


        public string GetBaseVersion()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }

    public enum VcsType
    {
        Git,
        Svn
    }

    [Serializable]
    public class RepositoryInfo
    {
        /// <summary>
        /// Version control systems type
        /// </summary>
        public VcsType vcsType = VcsType.Git;

        /// <summary>
        /// 表配置仓库的地址
        /// </summary>
        public string gameTableDataRepositoryRemotePath = "";

        /// <summary>
        /// 分支名或路径
        /// </summary>
        public string branchName = "master";

        /// <summary>
        /// 表配置本地仓库目录名
        /// </summary>
        public string gameTableDataRepositoryLocalDirName = "conf";
    }


    [System.Serializable]
    public class FilesUpLoadInfo
    {
        public enum PythonType
        {
            Python,
            Python3
        }

        /// <summary>
        /// 本地上传目录名
        /// </summary>
        public string NativeUpLoadDirFolderName = "UpLoadRes";

        /// <summary>
        /// 需要上传的文件后缀
        /// </summary>
        public string uploadFilesPattern = ".x,.zip,.lua";

        /// <summary>
        /// 远端根目录
        /// </summary>
        public string remoteDir = "samples";

        /// <summary>
        /// 配置仓库的yaml文件名(带后缀)
        /// </summary>
        public string protokitgoConfigName = "protokitgo.yaml";

        /// <summary>
        /// Python版本
        /// </summary>
        public PythonType pythonType = PythonType.Python;

        /// <summary>
        /// 是否上传到远端服务器
        /// </summary>
        public bool isUploadToRemote = true;
    }


    public class AppBuildConfig : ScriptableObject
    {

        public string buildCacheRelativePath = "/../AppBuildCache";

        public string LastBuildVersionName = "LastBuildVersion.json";
        public string LastBuildInfoName = "LastBuildInfo.json";

        public string BuildInfoFolder
        {
            get
            {
                var result = GetBuildCacheFolderPath() + "/BuildInfos";
                if (!Directory.Exists(result))
                    Directory.CreateDirectory(result);
                return result;
            }
        }

        [Header("当前工程资源根目录名(相对于Assets文件夹)")]
        public string ResourcesFolder = "ResourcesAB";

        [Header("AssetsBundle资源临时输出目录名")]
        public string AssetBundleExportFolderName = "AB";

        public string LastBuildVersionPath => BuildInfoFolder + @"/"+ LastBuildVersionName;
        public string LastBuildInfoPath => BuildInfoFolder + @"/" + LastBuildInfoName;

        public string AppBuildConfigFolder => Application.dataPath + $"{GetRelativeToAssetsPath()}/Editor/BuildConfigs";

        public AppVersion targetAppVersion;

        [Header("此路径需要包含“Assets/”前缀，举例：Assets/AssetGraphConfig/demo.asset")]
        public string targetAssetGraphConfigRelativeToProjectPath = "";

        public string TargetAssetGraphConfigAssetsPath => $"{targetAssetGraphConfigRelativeToProjectPath}";

        [Header("数据仓库信息")]
        public RepositoryInfo repositoryInfo;

        [Header("资源上传信息")]
        public FilesUpLoadInfo upLoadInfo;

        [Header("制作Patch版本时自增修订号")]
        public bool incrementRevisionNumberForPatchBuild = true;

        /// <summary>
        /// 数据表配置仓库父目录
        /// </summary>
        public string GameTableDataConfigParentPath
        {
            get
            {
                var dir = EditorUtils.OptimazePath($"{Application.dataPath}/../Library/GameTableDataLocalPath");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        /// <summary>
        /// 数据表配置仓库目录
        /// </summary>
        public string GameTableDataConfigPath
        {
            get { return $"{GameTableDataConfigParentPath}/{this.repositoryInfo.gameTableDataRepositoryLocalDirName}"; }
        }

        public static AppBuildConfig GetAppBuildConfigInst()
        {
            var config = AssetDatabase.LoadAssetAtPath<AppBuildConfig>(GetConfigRelativeToProjectPath());

            if (!config)
            {
                throw new FileNotFoundException($"The file that path is \"{GetConfigRelativeToProjectPath()}\" is not found!");
            }
            return config;
        }

        public string GetBuildCacheFolderPath()
        {
            string pathFolder = Application.dataPath + buildCacheRelativePath;
            pathFolder = EditorUtils.OptimazePath(pathFolder);

            if(!Directory.Exists(pathFolder))
                Directory.CreateDirectory(pathFolder);
            return pathFolder;
        }

        private static string GetRelativeToAssetsPath()
        {
            return "/GamePackageRes/AppBuilder";
        }

        private static string GetRootPath()
        {
            return EditorUtils.OptimazePath(Application.dataPath + GetRelativeToAssetsPath());
        }

        public static string GetConfigAbsoluteFolderPath()
        {
            return $"{GetRootPath()}/Editor";
        }

        public static string GetConfigAbsolutePath()
        {
            return $"{GetRootPath()}/Editor/AppBuilderConfig.asset";
        }

        public static string GetConfigRelativeToProjectPath()
        {
            return $"Assets{GetRelativeToAssetsPath()}/Editor/AppBuilderConfig.asset";
        }
    }
}
