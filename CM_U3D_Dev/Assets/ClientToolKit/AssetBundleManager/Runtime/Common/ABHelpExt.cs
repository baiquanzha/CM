using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using System;
using System.Net;

public partial class AbHelp
{
    /// <summary>
    /// 获取一个文件的内存大小
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static long GetObjectSize(object o)
    {
        long size = 0;
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            size = stream.Length;
        }
        return size;
    }

    //private static string logPath = null;
    
    /// <summary>
    /// AB系统启动时的日志写入
    /// </summary>
    /// <param name="log"></param>
    //public static void HotFixLog(string log)
    //{
    //    if (logPath == null)
    //    {
    //        logPath = GetWritePath("hotfix.log");
    //        if (File.Exists(logPath))
    //        {
    //            File.Delete(logPath);
    //        }
    //    }
    //    Debug.Log(log);
    //    File.AppendAllText(logPath, string.Concat(log, "\r\n"));
    //}


    private static string[] checkPath, checkPeople;
    
    /// <summary>
    /// AB系统的日志输出
    /// </summary>
    /// <param name="path"></param>
    /// <param name="isPath"></param>
    //public static void ABLog(string path)
    //{
    //    Debug.LogError(path);

        //if (isPath)
        //{
        //    if (checkPath == null)
        //    {
        //        string configPath = "WwiseTable/ResourcesPeople";
        //        if (ABMgr.ABExist(configPath))
        //        {
        //            TextAsset asset = ABMgr.Load<TextAsset>(configPath);
        //            string[] confStr = asset.text.Split(new char[] {'\n'}, StringSplitOptions.RemoveEmptyEntries);
        //            checkPath = new string[confStr.Length];
        //            checkPeople = new string[confStr.Length];
        //            for (int i = 0; i < confStr.Length; i++)
        //            {
        //                string[] tmp = confStr[i].Split(new char[] {':'});
        //                checkPath[i] = tmp[0];
        //                checkPeople[i] = tmp[1];
        //            }
        //        }
        //        else
        //        {
        //            checkPath = new string[0];
        //        }
        //    }

        //    if (checkPath.Length > 0)
        //    {
        //        string people = null;
        //        for (int i = 0; i < checkPath.Length; i++)
        //        {
        //            string tmp = checkPath[i];
        //            if (path.Length >= tmp.Length)
        //            {
        //                if (path.IndexOf(tmp, StringComparison.OrdinalIgnoreCase) > -1)
        //                {
        //                    people = checkPeople[i];
        //                    break;
        //                }
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }

        //        if (people == null)
        //        {
        //            Debug.LogError("缺资源:\"" + path + "\"");
        //        }
        //        else
        //        {
        //            Debug.LogError("缺资源:\"" + path + "\"，可以勾搭下\"" + people + "\" :-)");
        //        }
        //    }
        //    else
        //    {
        //        Debug.LogError("缺资源:\"" + path + "\"");
        //    }
        //}
        //else
        //{
        //    Debug.LogError(path);
        //}
    //}
    private static string[] SizeName = new string[] { "B", "KB", "MB", "GB", "TB" };
    private static string[] TriSizeName = new string[] { "", "K", "W"};
    
    /// <summary>
    /// 数字转换成容量
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetSizeStr(double size)
    {
        string result = null;
        int sizeIndex = 0;
        while(size > 1024)
        {
            size = size / 1024.0f;
            sizeIndex++;
        }
        result = string.Concat(size.ToString("f2"), SizeName[sizeIndex]);
        return result;
    }
    
    /// <summary>
    /// 数字转换成容量
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetTriStr(double size)
    {
        string result = null;
        int sizeIndex = 0;
        if(size > 10000)
        {
            size = size / 10000.0f;
            sizeIndex = 2;
        }
        else if (size > 1000)
        {
            size = size / 1000.0f;
            sizeIndex = 1;
        }
        result = string.Concat(size.ToString("f2"), TriSizeName[sizeIndex]);
        return result;
    }
    
#if UNITY_EDITOR
    /// <summary>
    /// 是否是默认场景
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static bool IsDefaultScene(string name)
    {
        for(int i = 0; i < defaultScenes.Length; i++)
        {
            if (string.Compare(name, defaultScenes[i], StringComparison.OrdinalIgnoreCase) == 0)
            {
                return true;
            }
        }
        return false;
    }
    
    private static readonly string[] defaultScenes = new string[] { "ab.unity", "Entry.unity"};
    
    /// <summary>
    /// 修改编辑器场景设置
    /// </summary>
    /// <param name="sourcePaths"></param>
    /// <param name="targetPaths"></param>
    /// <returns></returns>
    public static UnityEditor.EditorBuildSettingsScene[] GetEditorBuildSettingsScene(List<string> sourcePaths, List<string> targetPaths)
    {
        List<string> list = null;
        FileInfo lastScenesPathFile = new FileInfo(LastScenesPath);
        if (lastScenesPathFile.Exists)
        {
            list = JsonUtility.FromJson<LastScenes>(File.ReadAllText(LastScenesPath, Encoding.UTF8)).Scenes;
            lastScenesPathFile.Delete();
        }
        else
        {
            list = new List<string>();
        }
        
        UnityEditor.EditorBuildSettingsScene[] lastScenes = UnityEditor.EditorBuildSettings.scenes;
        for(int i = 0; i < lastScenes.Length; i++)
        {
            UnityEditor.EditorBuildSettingsScene item = lastScenes[i];
            if (list.Contains(item.path))
            {
                item.enabled = true;
            }
        }

        for(int i = 0; i < lastScenes.Length; i++)
        {
            UnityEditor.EditorBuildSettingsScene tmpScene = lastScenes[i];
            if (tmpScene.enabled)
            {
                int spIndex = tmpScene.path.LastIndexOf("/", StringComparison.Ordinal);
                string targetPath = tmpScene.path.Substring(spIndex + 1);
                if (!IsDefaultScene(targetPath))
                {
                    tmpScene.enabled = false;
                    if (sourcePaths != null)
                    {
                        sourcePaths.Add(tmpScene.path);
                    }

                    if (targetPaths != null)
                    {
                        targetPaths.Add(string.Concat(SceneFolder, "/", targetPath));
                    }
                }
            }
        }
        
        string json = JsonUtility.ToJson(new LastScenes(sourcePaths));
        if (json != null && json.Length > 0)
        {
            File.WriteAllText(AbHelp.LastScenesPath, json, Encoding.UTF8);
        }

        return lastScenes;
    }
    
    public static readonly string SceneFolder = "Assets/abScene";

    public static string LastScenesPath
    {
        get
        {
            return string.Concat(System.Environment.CurrentDirectory, Path.DirectorySeparatorChar, "LastScene.x");
        }
    }
    
    ///// <summary>
    ///// 恢复编辑器的场景设置
    ///// </summary>
    //public static void BackBuildSceneSetting()
    //{
    //    Debug.Log("ABMenu.BackBuildSceneSetting...");
    //    FileInfo lastScenesPathFile = new FileInfo(AbHelp.LastScenesPath);
    //    if (lastScenesPathFile.Exists)
    //    {
    //        Debug.Log("ABMenu.BackBuildSceneSetting OK.");
    //        List<string> list = JsonUtility.FromJson<LastScenes>(File.ReadAllText(AbHelp.LastScenesPath, Encoding.UTF8)).Scenes;
    //        UnityEditor.EditorBuildSettingsScene[] lastScenes = UnityEditor.EditorBuildSettings.scenes;
    //        for(int i = 0; i < lastScenes.Length; i++)
    //        {
    //            UnityEditor.EditorBuildSettingsScene tmpScene = lastScenes[i];
    //            if (list.Contains(tmpScene.path))
    //            {
    //                tmpScene.enabled = true;
    //            }
    //        }
    //        UnityEditor.EditorBuildSettings.scenes = lastScenes;
    //        lastScenesPathFile.Delete();
    //    }
    //    else
    //    {
    //        Debug.Log("ABMenu.BackBuildSceneSetting fail, no file:" + AbHelp.LastScenesPath);
    //    }
    //}
#endif
}

#if UNITY_EDITOR
[System.Serializable]
public sealed class LastScenes
{
    public LastScenes(List<string> scenes)
    {
        Scenes = scenes;
    }
    public List<string> Scenes;
}
#endif