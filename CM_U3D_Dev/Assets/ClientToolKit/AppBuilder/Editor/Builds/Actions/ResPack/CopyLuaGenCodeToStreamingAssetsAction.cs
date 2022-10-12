using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.Core.Pipeline;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class CopyLuaGenCodeToStreamingAssetsAction : BaseBuildFilterAction
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
            //var streamingPath = AppBuildContext.GetAssetsOutputPath();
            //if (!Directory.Exists(streamingPath))
            //{
            //    AppBuildContext.AppendErrorLog($"The target streaming directory that path is \"{streamingPath}\" not exist.");
            //    return false;
            //}

            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            CopyLuaGenCode(filter,input);

            this.State = ActionState.Completed;
        }


        private void CopyLuaGenCode(IFilter filter, IPipelineInput input)
        {
            var luaprojectDir = input.GetData<string>("LuaProject", AppBuildContext.GetLuaProjectPath());
            luaprojectDir = EditorUtils.OptimazePath(luaprojectDir);

            var streamingPath = AppBuildContext.GetAssetsOutputPath();
            
            DirectoryInfo dirInfo = new DirectoryInfo(luaprojectDir);
            FileInfo[] fileInfos = dirInfo.GetFiles("*.lua", SearchOption.AllDirectories);

            foreach (var fileInfo in fileInfos)
            {
                string sourcePath = EditorUtils.OptimazePath(fileInfo.FullName);
                if (!sourcePath.Contains(AppBuildContext.GenCodePattern))
                {
                    continue;
                }

                string targePath = $"{streamingPath}/lua{sourcePath.Replace(luaprojectDir, "")}";

                string dirName = Path.GetDirectoryName(targePath);

                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                File.Copy(sourcePath, targePath, true);
            }

            AssetDatabase.Refresh();
            Logger.Info("Copy lua gen code to streaming assets path !");
        }
        #endregion

    }
}