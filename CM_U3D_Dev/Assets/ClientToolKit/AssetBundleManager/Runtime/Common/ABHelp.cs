using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using System;
using System.Net;
using MTool.AssetBundleManager.Runtime;
using MTool.AppUpdaterLib.Runtime;

/// <summary>
/// AB系统帮助类
/// </summary>
public partial class AbHelp
{
    public static MonoBehaviour CoroutineHandle = null;

    //    public static void GetFileSize()
    //    {
    //        Texture target = Selection.activeObject as Texture;
    //        var type = Types.GetType ("UnityEditor.TextureUtil", "UnityEditor.dll");
    //        MethodInfo methodInfo = type.GetMethod ("GetStorageMemorySize", BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
    // 
    //        Debug.Log("内存占用："+EditorUtility.FormatBytes(Profiler.GetRuntimeMemorySize(Selection.activeObject)));
    //        Debug.Log("硬盘占用："+EditorUtility.FormatBytes((int)methodInfo.Invoke(null,new object[]{target})));
    //    }

    /// <summary>
    /// 获取热更系统在服务器上的相对平台路径
    /// </summary>
    /// <returns></returns>
    public static string GetPlatformStr()
    {
#if UNITY_EDITOR
        switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.Android:
                {
                    return "/Android/";
                }
            case UnityEditor.BuildTarget.iOS:
                {
                    return "/IOS/";
                }
            default:
                {
                    return "/Standlone/";
                }
        }
#else
#if UNITY_ANDROID && !UNITY_EDITOR
                return "/Android/";
#elif UNITY_IPHONE && !UNITY_EDITOR
                return "/IOS/";
#else
                return "/Standlone/";
#endif
        
#endif
    }

    public static string GetPlatformNameNoSlash()
    {
#if UNITY_EDITOR
        switch (UnityEditor.EditorUserBuildSettings.activeBuildTarget)
        {
            case UnityEditor.BuildTarget.Android:
                {
                    return "Android";
                }
            case UnityEditor.BuildTarget.iOS:
                {
                    return "IOS";
                }
            default:
                {
                    return "Standlone";
                }
        }
#else
#if UNITY_ANDROID && !UNITY_EDITOR
                return "Android";
#elif UNITY_IPHONE && !UNITY_EDITOR
                return "IOS";
#else
                return "Standlone";
#endif
        
#endif
    }

    /// <summary>
    /// 获取服务器下载路径
    /// </summary>
    /// <param name="path"></param>
    /// <param name="useExt"></param>
    /// <returns></returns>
    //public static string GetUrl(string path)
    //{
    //    TmpSB.Clear();
    //    TmpSB.Append(FileSite);
    //    TmpSB.Append("/");
    //    TmpSB.Append(path);
    //    return TmpSB.ToString();
    //}
    private static bool firstGetWritePath = true;


    private static string mPersistentDataPath = string.Empty;
    public static string PersistentDataPath
    {
        get
        {
            if (string.IsNullOrEmpty(mPersistentDataPath))
            {
#if UNITY_EDITOR
                mPersistentDataPath = System.IO.Path.GetFullPath(Application.dataPath + "/../SimulatePersistentDataPath/");
                mPersistentDataPath = mPersistentDataPath.Replace(@"\", @"/");
#else
                mPersistentDataPath = Application.persistentDataPath;
#endif
            }
            return mPersistentDataPath;
        }
    }


    /// <summary>
    /// 获取游戏的可写目录
    /// </summary>
    /// <param name="path"></param>
    /// <param name="createFolder"></param>
    /// <param name="ext"></param>
    /// <returns></returns>
    public static string GetWritePath(string path, bool createFolder = false, string ext = null)
    {
        if (string.IsNullOrEmpty(path)) return null;
        if (firstGetWritePath)
        {
            firstGetWritePath = false;
            TmpSB.Length = 0;
            TmpSB.Append(PersistentDataPath);
            TmpSB.Append("/");
            TmpSB.Append(RootFolder);
            DirectoryInfo folder = new DirectoryInfo(TmpSB.ToString());
            if (!folder.Exists)
                folder.Create();
        }
        string result = null;
        if (createFolder)
        {
            TmpSB.Length = 0;
            TmpSB.Append(PersistentDataPath);
            TmpSB.Append("/");
            TmpSB.Append(RootFolder);
            string[] ps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (ps.Length > 1)
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    TmpSB.Append("/");
                    TmpSB.Append(ps[i]);
                    result = TmpSB.ToString();
                    if (i < ps.Length - 1)
                    {
                        DirectoryInfo folder = new DirectoryInfo(result);
                        if (!folder.Exists)
                            folder.Create();
                    }
                }
            }
            else
            {
                TmpSB.Append("/");
                TmpSB.Append(path);
            }
        }
        else
        {
            TmpSB.Length = 0;
            TmpSB.Append(PersistentDataPath);
            TmpSB.Append("/");
            TmpSB.Append(RootFolder);
            TmpSB.Append("/");
            TmpSB.Append(path);
        }
        if (ext != null)
            TmpSB.Append(ext);
        result = TmpSB.ToString();
        return result;
    }
    /*
  /// <summary>
  /// 获取游戏自带包文件
  /// </summary>
  /// <param name="path"></param>
  /// <param name="ext"></param>
  /// <param name="loadAB"></param>
  /// <returns></returns>
  public static string GetStreamingAssetsPath(string path, string ext = null, bool loadAB = true)
  {
      if (loadAB && configList.ContainsKey(path))
      {
          ABTableItemInfoClient item = configList[path];
          if (item.R)
              return GetWritePath(path, true, ext);
      }
      TmpSB.Length = 0;

#if UNITY_ANDROID && !UNITY_EDITOR
      if(loadAB)
      {
          TmpSB.Append(Application.dataPath);
          TmpSB.Append("!assets/");
      }
      else
      {
          TmpSB.Append(Application.streamingAssetsPath);
          TmpSB.Append("/");
      }
#elif UNITY_IPHONE && !UNITY_EDITOR
      if(loadAB)
      {
          TmpSB.Append(Application.dataPath);
          TmpSB.Append("/Raw/");
      }
      else
      {
          TmpSB.Append("file://");
          TmpSB.Append(Application.streamingAssetsPath);
          TmpSB.Append("/");
      }
#elif UNITY_STANDLONE_WIN || UNITY_EDITOR
      if (!loadAB)
      {
          TmpSB.Append("file://");
      }
      TmpSB.Append(Application.streamingAssetsPath);
      TmpSB.Append("/");
#endif
      TmpSB.Append(path);
      if (ext != null)
          TmpSB.Append(ext);
      return TmpSB.ToString();
  }


  private static readonly Dictionary<string, ABTableItemInfoClient> configList = new Dictionary<string, ABTableItemInfoClient>(StringComparer.OrdinalIgnoreCase);

  /// <summary>
  /// 使用内置版本文件清单
  /// </summary>
  /// <param name="config"></param>
  /// <param name="clientPath"></param>
  public static void ABConfigInfoClientCopyFrom(VersionManifest config, string clientPath)
  {
      configList.Clear();
      ABConfigInfoClient clientInfo = new ABConfigInfoClient();
      clientInfo.Ver = config.Ver;
      clientInfo.List = new ABTableItemInfoClient[config.Datas.Count];
      for (int i = 0; i < config.Datas.Count; i++)
      {
          ABTableItemInfoClient infoClient = new ABTableItemInfoClient();
          var info = config.Datas[i];
          infoClient.N = info.N;
          infoClient.H = info.H;
          infoClient.S = info.S;
          infoClient.R = false;//因为使用的是内置的清单，所以当前默认文件都需要读内置的文件
          clientInfo.List[i] = infoClient;
          configList[infoClient.N] = infoClient;
      }
      ConfigInfoClient = null;
      ConfigInfoClient = clientInfo;
      string json = JsonUtility.ToJson(ConfigInfoClient);
      File.WriteAllBytes(clientPath, System.Text.Encoding.UTF8.GetBytes(json));
  }

  /// <summary>
  /// 拷贝客户端本地的文件清单
  /// </summary>
  /// <param name="config"></param>
  public static void ABConfigInfoClientCopyFrom(ABConfigInfoClient config)
  {
      configList.Clear();
      for (int i = 0; i < config.List.Length; i++)
      {
          ABTableItemInfoClient info = config.List[i];
          configList[info.N] = info;
      }
  }

  */

    /// <summary>
    /// 清除游戏的可写目录
    /// </summary>
    public static void DeleteRootFolder()
    {
        string rootPath = AbHelp.GetWritePath(AbHelp.RootFolder);
        DirectoryInfo rootFolder = new DirectoryInfo(rootPath);
        if (rootFolder.Exists)
        {
            rootFolder.Delete(true);
        }
    }

    /*
    /// <summary>
    /// 判断一个文件是否需要热更新
    /// </summary>
    /// <param name="item"></param>
    /// <param name="add"></param>
    /// <returns></returns>
    public static FileDesc GetTask(FileDesc item, ref bool add)
    {
        add = false;
        FileDesc result = null;
        if (configList.ContainsKey(item.N))
        {
            result = configList[item.N];
            if (result.H.CompareTo(item.H) != 0)
            {
                add = true;
                result = item;
            }
        }
        else
        {
            add = true;
            result = item;
        }
        return result;
    }

    /// <summary>
    /// 把一个热更的文件信息写入本地
    /// </summary>
    /// <param name="task"></param>
    public static void UpdateTask(FileDesc task)
    {
        if (configList.ContainsKey(task.N))
        {
            ABTableItemInfoClient item = configList[task.N];
            item.H = task.H;
            item.S = task.S;
            item.R = true;
        }
        else
        {
            ABTableItemInfoClient item = new ABTableItemInfoClient();
            item.N = task.N;
            item.H = task.H;
            item.S = task.S;
            item.R = true;
            configList[task.N] = item;
        }
    }
    

    /// <summary>
    /// 保存本地热更的配置到手机
    /// </summary>
    /// <param name="saveVer"></param>
    public static void SaveConfigInfoClient(bool saveVer)
    {
        List<ABTableItemInfoClient> tmpList = new List<ABTableItemInfoClient>();
        foreach (KeyValuePair<string, ABTableItemInfoClient> kv in configList)
        {
            tmpList.Add(kv.Value);
        }
        if (ConfigInfoClient != null)
        {
            if (saveVer)
            {
                ConfigInfoClient.Ver = RemoteConfigInfoVer.GetVersionString();
            }
            ConfigInfoClient.List = tmpList.ToArray();
            string json = JsonUtility.ToJson(ConfigInfoClient);
            File.WriteAllBytes(string.Concat(GetWritePath(AbConfigInfoClientName)), System.Text.Encoding.UTF8.GetBytes(json));
            ConfigInfoClient = null;
        }
        else
        {
            Debug.Log("Save ConfigInfoClient fail!");
        }
    }

    */

    //public static bool IsDebug = false;

    //public static bool IsUseAB = false;

    //public static bool IsDone = false;
    public static bool IsFirstRun = false;
    public static string AssetVer = string.Empty;
    //public static ABConfigInfoClient ConfigInfoClient = null;
    public static ResManifest AbConfig = null;
    public static StringBuilder TmpSB = new StringBuilder();
    public static UpdateResMap ResLoadMap = null;


#if UNITY_EDITOR
    internal static string ResourcesPath = "Assets/ResourcesAB";
#endif

    //public static readonly string HotfixInfoName = "Hotfix.x";
    //public static readonly string AbConfigName = "FileList.x";
    //public static readonly string AbConfigInfoName = "FileListInfo.x";
    //public static readonly string AbConfigStrName = "FileListView.x";
    public static readonly string AbConfigInfoClientName = "FileListClientInfo.x";
    //public static readonly string AbConfigInfoStrName = "FileListInfoView.x";
    public static readonly string RootFolder = "Assets";
    public static readonly string AbFileExt = ".x";
    //http://localhost:8080/download/
    public static readonly string DefaultFileSite = "http://10.1.65.158:8022/ftp/GameRes/Files/ABs";
    //public static readonly string DefaultUpdatePath = "http://10.1.65.158:8022/ftp/GameRes/Files/ABs";
    //public static string FileSite = "";//文件服务器远端目录
    //public static string UpdatePath = "";//app更新页
    //public static Version RemoteConfigInfoVer = null;
#if UNITY_EDITOR
    public static readonly string[] FileExt = new string[] { ".meta", ".cginc" };

    public static bool CheckFileExt(string ext)
    {
        bool result = false;
        for (int i = 0; i < FileExt.Length; i++)
        {
            if (FileExt[i].CompareTo(ext) == 0)
            {
                result = true;
                break;
            }
        }

        return result;
    }
    public static string GetStackTraceModelName()
    {
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        System.Diagnostics.StackFrame[] sfs = st.GetFrames();
        string _filterdName = "ResponseWrite,ResponseWriteError,";
        string _fullName = string.Empty, _methodName = string.Empty;
        for (int i = 1; i < sfs.Length; ++i)
        {
            if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset()) break;
            _methodName = sfs[i].GetMethod().Name;
            if (_filterdName.Contains(_methodName)) continue;
            _fullName = _methodName + "()->" + _fullName;
        }
        st = null;
        sfs = null;
        _filterdName = _methodName = null;
        return _fullName.TrimEnd('-', '>');
    }
#endif
}
