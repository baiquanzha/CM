using MTool.Core.Pipeline;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class LuaScriptsCopyAction : BaseBuildFilterAction
    {

        string GetLuaRootPath()
        {
            string luaRootPath = $"{Application.dataPath}/../LuaProject";
            luaRootPath = EditorUtils.OptimazePath(luaRootPath);
            return luaRootPath;
        }

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            var luaRootPath = GetLuaRootPath();

            if (Directory.Exists(luaRootPath))
            {
                return true;
            }

            var appBuildContext = AppBuildContext;
            appBuildContext.ErrorSb.AppendLine($"The luaRootPath that path is \"{luaRootPath}\" is not exist!");
            return false;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            string luaRootPath = GetLuaRootPath();
            Logger.Info($"Lua Root Path : {luaRootPath}");
            string copyPath = EditorUtils.OptimazePath(Application.streamingAssetsPath + "/lua"); //(Application.streamingAssetsPath + "/lua").Replace('/', '\\');
            ClearOldScripts(filter, input, copyPath);
            CopyScripts(filter, input, luaRootPath, copyPath);

            AssetDatabase.Refresh();
            this.State = ActionState.Completed;
        }

        private void ClearOldScripts(IFilter filter, IPipelineInput input, string scriptFolder)
        {
            if (Directory.Exists(scriptFolder))
            {
                Directory.Delete(scriptFolder, true);
            }

            AssetDatabase.Refresh();
        }

        private void CopyScripts(IFilter filter, IPipelineInput input, string srcPath, string dstPath)
        {
            try
            {
                Logger.Info($"copy lua script, src:{srcPath}, dst:{dstPath}");
                if (!Directory.Exists(dstPath))
                {
                    Directory.CreateDirectory(dstPath);
                }

                string[] files = Directory.GetFiles(srcPath, "*.lua");
                for (int i = 0; i < files.Length; i++)
                {
                    File.Copy(srcPath + "/" + Path.GetFileName(files[i]), dstPath + "/" + Path.GetFileName(files[i]));
                }

                string[] dirs = Directory.GetDirectories(srcPath);
                for (int i = 0; i < dirs.Length; i++)
                {
                    dirs[i] = EditorUtils.OptimazePath(dirs[i]);
                    int idx = dirs[i].LastIndexOf('/');
                    string subFolder = dirs[i].Substring(idx + 1);
                    CopyScripts(filter, input, srcPath + "/" + subFolder, dstPath + "/" + subFolder);
                }
            }
            catch (Exception e)
            {
                Logger.Warn($"copy lua files throw exception : {e.Message}");
            }
        }
    }
}