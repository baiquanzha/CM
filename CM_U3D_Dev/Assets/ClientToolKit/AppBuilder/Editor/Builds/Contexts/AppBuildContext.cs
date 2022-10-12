using System;
using System.IO;
using System.Text;
using MTool.AppUpdaterLib.Runtime;
using MTool.AssetBundleManager.Runtime;
using UnityEngine;
using MTool.Core.Pipeline;
using MTool.AppBuilder.Editor.Builds.BuildInfos;
using MTool.AppUpdaterLib.Runtime.Manifests;
using UnityEditor;
using MTool.AppBuilder.Editor.Builds.Factories;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;
using Version = MTool.AppUpdaterLib.Runtime.Version;
#if DEBUG_FILE_CRYPTION
using File = MTool.Core.IO.File;
#else
using File = System.IO.File;
#endif

namespace MTool.AppBuilder.Editor.Builds.Contexts
{
    public class AppBuildContext : IPipelineContext
    {
        #region Enum & Inner Class

        public enum MakeVersionMode
        {
            UnKnow,

            MakeBaseVersion,//制作基础版本

            MakePatchVersion,//制作基础版本的修订版
        }

        #endregion

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        public MakeVersionMode makeVersionMode = MakeVersionMode.UnKnow;

        public readonly string AssetsTitle = "Assets:",
            ResExt = ".x",
            StreamingAssetsFolder = "Assets/StreamingAssets",
            DependenciesTitle = "Dependencies:",
            DependenciesTitleError = "Dependencies: []",
            ManifestFineTitle = "Name: ",
            PackagePath = "Package",
            PatternManifest = ".manifest",
            VersionFormat = "yyyyMMddHHmmss",
            LuaProjectFolderName = "LuaProject",
            Temporary = "Temporary",
           
            Rules = "rules",
            DataRes = "Conf",
            BuildReportFolderName = "BuildReporters",
            GenCodePattern = "/gen/",
            RemoteManifestFolderName = "version_list";


        public string AbExportFolder
        {
            get { return AppBuildConfig.GetAppBuildConfigInst().AssetBundleExportFolderName; }
        }

        public string ResourcesFolder
        {
            get { return AppBuildConfig.GetAppBuildConfigInst().ResourcesFolder; }
        }

        #endregion

        //--------------------------------------------------------------

        #region Properties & Events

        //--------------------------------------------------------------

        public StringBuilder ErrorSb { private set; get; } = new StringBuilder(10);

        /// <summary>
        /// 资源文件清单，主要用于运行时资源管理
        /// </summary>
        public ResManifest ResManifest { private set; get; } = new ResManifest();

        /// <summary>
        /// 版本文件清单，主要用于热更新操作
        /// </summary>
        public VersionManifest VersionManifest { private set; get; } = new VersionManifest();

        /// <summary>
        /// 由基础版本到目前的最新文件的清单，用以保存
        /// </summary>
        public VersionManifest FromBaseToNowVersionManifest { private set; get; } = new VersionManifest();

        /// <summary>
        /// 版本描述清单 
        /// </summary>
        public AppInfoManifest AppInfoManifest { private set; get; } = SimpleFactory.CreateAppInfoManifest();

        public string UploadFileFolder { set; get; }

        public string LastBuildAppInfoFilePath => AppBuildConfig.GetAppBuildConfigInst().LastBuildInfoPath;

        public string LastBuildUnityResManifestPath => AppBuildConfig.GetAppBuildConfigInst().LastBuildVersionPath;


        private Encoding mEncoding = new UTF8Encoding(false,true);
        public Encoding TextEncoding => mEncoding;

        public string UnityResVersion { set; get; }

        #endregion

        //--------------------------------------------------------------

        #region Creation & Cleanup

        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------

        #region Methods

        //--------------------------------------------------------------

        /// <summary>
        /// 获取package路径
        /// </summary>
        /// <returns></returns>
        public string GetPackageFolderPath()
        {
            string packagePath = string.Concat(System.Environment.CurrentDirectory
                , Path.DirectorySeparatorChar
                , this.PackagePath);
            packagePath = EditorUtils.OptimazePath(packagePath);
            return packagePath;
        }


        /// <summary>
        /// 获取内建的app信息(对应AppInfoManifest)
        /// </summary>
        /// <returns></returns>
        public string GetBuiltinAppInfoFilePath()
        {
            string path = string.Concat(this.GetAssetsOutputPath()
                , Path.DirectorySeparatorChar
                , AssetsFileSystem.AppInfoFileName);
            path = EditorUtils.OptimazePath(path);
            return path;
        }

        /// <summary>
        /// 获取AssetBundle依赖清单
        /// </summary>
        /// <returns></returns>
        public string GetAssetsBundleResManifestPath()
        {
            string path = string.Concat(this.GetAssetsOutputPath()
                , Path.DirectorySeparatorChar
                , AssetsFileSystem.UnityABFileName);
            path = EditorUtils.OptimazePath(path);
            return path;
        }

        #region Unity资源更新清单路径

        /// <summary>
        /// 获取本地StreamingAssets目录下的资源更新清单文件路径（不带MD5）（对应VersionManifest）
        /// </summary>
        /// <returns></returns>
        public string GetLocalUnityResUpdateManifestPath()
        {
            string path = string.Concat(this.GetAssetsOutputPath()
                , Path.DirectorySeparatorChar
                , AssetsFileSystem.UnityResManifestNamePattern);

            path = string.Format(path, AssetsFileSystem.GetPlatformStringForConfig());
            path = EditorUtils.OptimazePath(path);
            return path;
        }

        /// <summary>
        /// 获取将要上传的unity资源更新清单路径
        /// </summary>
        /// <param name="md5"></param>
        /// <param name="targetFolder">目标文件夹</param>
        /// <returns></returns>
        public string GetWillUploadedUnityResUpdateManifestPath(string md5,string targetFolder)
        {
            string path = string.Concat(targetFolder, Path.DirectorySeparatorChar
                , this.RemoteManifestFolderName
                , Path.DirectorySeparatorChar
                , AssetsFileSystem.RemoteUnityResManifestPattern);

            path = string.Format(path,AssetsFileSystem.GetPlatformStringForConfig(), md5);
            path = EditorUtils.OptimazePath(path);
            return path;
        }

        #endregion


        /// <summary>
        /// 获取本地Unity项目的表数据目标目录
        /// </summary>
        /// <returns></returns>
        public string GetConfResourceFodlerPath()
        {
            string path = string.Concat(Application.dataPath
                ,Path.DirectorySeparatorChar
                , this.ResourcesFolder
                , Path.DirectorySeparatorChar
                , this.DataRes);
            path = EditorUtils.OptimazePath(path);
            return path;
        }

        /// <summary>
        /// 获取本地表数据来源（表数据Git/SVN 本地仓库）的目录路径
        /// 此路径在Conf的父目录，与Conf目录平级的有一个res_data-{md5}.json的表数据版本的描述文件
        /// </summary>
        /// <returns></returns>
        public string GetConfLocalVerisonControlTargetFodlerPath()
        {
            string path = AppBuildConfig.GetAppBuildConfigInst().GameTableDataConfigPath;

            if (string.IsNullOrEmpty(path))
            {
                return string.Empty;
            }

            path = EditorUtils.OptimazePath(path);

            return path;
        }

        #region Local config load

        /// <summary>
        /// 获取当前项目的StreamingAssets文件夹下的文件Manifests,名为manifest,用作运行时AB资源管理
        /// </summary>
        /// <returns>文件Manifest</returns>
        public ResManifest LoadLocalUnityResManifestFile()
        {
            string resManifestPath = GetAssetsBundleResManifestPath();
            var contents = File.ReadAllText(resManifestPath, this.TextEncoding);
            ResManifest resManifest = JsonUtility.FromJson<ResManifest>(File.ReadAllText(resManifestPath,this.TextEncoding));
            return resManifest;
        }

        /// <summary>
        /// 加载本地StreamingAssets目录下的资源版本清单
        /// </summary>
        /// <returns></returns>
        public VersionManifest LoadLocalVersionManifestFile()
        {
            string versionManifestPath = GetLocalUnityResUpdateManifestPath();
            var contents = File.ReadAllText(versionManifestPath, this.TextEncoding);
            VersionManifest versionManifest = VersionManifestParser.Parse(contents);
            return versionManifest;
        }


        #endregion


        #region 本地缓存的配置，包括编译配置

        /// <summary>
        /// 获取上次编译的app信息
        /// </summary>
        /// <returns></returns>
        public string GetLastBuildInfoPath()
        {
            string path = this.LastBuildAppInfoFilePath;

            path = EditorUtils.OptimazePath(path);
            return path;
        }

        /// <summary>
        /// 获取上次编译的文件清单路径
        /// </summary>
        /// <returns></returns>
        public string GetLastUnityResManifestInfoPath()
        {
            string path = this.LastBuildUnityResManifestPath;

            path = EditorUtils.OptimazePath(path);
            return path;
        }


        public LastBuildInfo GetLastBuildInfo()
        {
            var path = GetLastBuildInfoPath();
            if (!File.Exists(path))
            {
                return null;
            }

            string jsonContents = File.ReadAllText(path,this.TextEncoding);
            var lastBuildInfo = JsonUtility.FromJson<LastBuildInfo>(jsonContents);

            return lastBuildInfo;
        }

        public void SaveLastBuildInfo(LastBuildInfo info)
        {
            var path = GetLastBuildInfoPath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string jsonContents = ToJson(info);
            File.WriteAllText(path,jsonContents,this.TextEncoding);
            Debug.Log($"Save last build info that path is \"{path}\" completed!");
        }

        #endregion


        public void AppendErrorLog(string message)
        {
            this.ErrorSb.AppendLine(message);
        }

        public string Error => this.ErrorSb.ToString();


        public string ToJson(object obj, bool prettyPrint = true)
        {
            return JsonUtility.ToJson(obj,prettyPrint);
        }

        //public VersionManifest JsonToVersionManifest(string contents)
        //{
        //    return VersionManifestParser.Parse(contents);
        //}

        public string VersionManifestToJson(VersionManifest manifest)
        {
            return VersionManifestParser.Serialize(manifest);
        }


        public string GetBuildReportsPath(string fileName)
        {
            string path = string.Concat(System.Environment.CurrentDirectory
                , Path.DirectorySeparatorChar
                , this.BuildReportFolderName
                , Path.DirectorySeparatorChar
                , fileName);
            path = EditorUtils.OptimazePath(path);

            var dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return path;
        }


        public string GetAssetsOutputPath()
        {
            string outputPath = null;

#if APPEND_PLATFORM_NAME
            outputPath = string.Concat(System.Environment.CurrentDirectory,
                    Path.DirectorySeparatorChar,
                    StreamingAssetsFolder,
                    Path.DirectorySeparatorChar,
                    Utility.GetPlatformName()
                );
#else
            outputPath = string.Concat(System.Environment.CurrentDirectory, 
                Path.DirectorySeparatorChar, 
                StreamingAssetsFolder);
#endif
            outputPath = EditorUtils.OptimazePath(outputPath);
            return outputPath;
        }


        public string GetResStoragePath()
        {
            string path = string.Concat(System.Environment.CurrentDirectory
                , Path.DirectorySeparatorChar
                , AppBuildConfig.GetAppBuildConfigInst().upLoadInfo.NativeUpLoadDirFolderName);
            path = EditorUtils.OptimazePath(path);

            return path;
        }

        public string GetLuaProjectPath()
        {
            //LuaProjectFolderName
            string path = $"{Application.dataPath}/../{this.LuaProjectFolderName}";
            path = EditorUtils.OptimazePath(path);
            return path;
        }


        public string GetPlatformStrForUpload()
        {
            var platformName = string.Empty;
#if UNITY_EDITOR && UNITY_ANDROID
            platformName = "android";
#elif UNITY_EDITOR && UNITY_IPHONE
            platformName = "ios";
#else
            throw new InvalidOperationException($"Unsupport build platform : {EditorUserBuildSettings.activeBuildTarget} .");
#endif
            return platformName;
        }


        /// <summary>
        /// 获取当前游戏版本号
        /// </summary>
        /// <returns></returns>
        public Version GetTargetAppVersion(bool getBaseVersion = false)
        {
            var appVersion = AppBuildConfig.GetAppBuildConfigInst().targetAppVersion;
            string versionStr = null;
            if (getBaseVersion)
            {
                versionStr = $"{appVersion.Major}.{appVersion.Minor}.0";
            }
            else
            {
                if (AppBuildConfig.GetAppBuildConfigInst().incrementRevisionNumberForPatchBuild)
                {
                    var lastVersionInfo = this.GetLastBuildInfo();
                    if (lastVersionInfo != null && lastVersionInfo.GetCurrentBuildInfo() != null)
                    {
                        var curBuildInfo = lastVersionInfo.GetCurrentBuildInfo();
                        var lastVersion = new Version(curBuildInfo.versionInfo.version);
                        lastVersion.IncrementOneForPatch();
                        return lastVersion;
                    }
                }
                else
                {
                    versionStr = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Patch}";
                }
            }
            Version result = new Version(versionStr);
            return result;
        }



        #endregion

    }
}
