using System.IO;
using System.Text;
using MTool.AppBuilder.Editor.Builds;
using UnityEditor;
using UnityEngine;

namespace MTool.AppBuilder.Editor.Menus
{
    public static class MenuItems
    {


        [MenuItem("MTool/AppBuilder/信息提取/显示上一次构建信息")]
        static void ShowLastBuildInfo()
        {
            string path = AppBuildConfig.GetAppBuildConfigInst().LastBuildInfoPath;

            path = EditorUtils.OptimazePath(path);
            if (!File.Exists(path))
            {
                Debug.LogError("此前从未执行过构建或构建信息已被清除！");

                return;
            }

            string jsonContents = File.ReadAllText(path, new UTF8Encoding(false, true));
            
            Debug.Log($"The Last build info : \n{jsonContents}");
        }

    }
}