using UnityEngine;
using System.IO;
using MTool.AppBuilder.Editor.Builds.Contexts;
using MTool.Core.Pipeline;
using UnityEditor;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResProcess
{
    public class ProcessDataResAction : BaseBuildFilterAction
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
            string confPath = GetDataConfPath(filter,input);

            var appBuildContext = AppBuildContext;

            if (string.IsNullOrEmpty(confPath))
            {
                appBuildContext.ErrorSb.AppendLine($"The data res config folder path that path is \"{confPath}\" is null!");
                return false;
            }

            if (!Directory.Exists(confPath))
            {
                appBuildContext.ErrorSb.AppendLine($"The data res config folder path that path is \"{confPath}\" is not exist!");
                return false;
            }

            string data_res_version_path = $"{confPath}/res_data.ver";

            if (!File.Exists(data_res_version_path))
            {
                appBuildContext.ErrorSb.AppendLine($"The res data config that path is \"{data_res_version_path}\" is not exist!");
                return false;
            }

            return true;
        }

        private string GetDataConfPath(IFilter filter, IPipelineInput input)
        {
            var context = AppBuildContext;
            string sourceConfParentPath = context.GetConfLocalVerisonControlTargetFodlerPath();

            return sourceConfParentPath;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.CopyToTemporaryFolder(filter,input);

            this.State = ActionState.Completed;
        }

        private void CopyToTemporaryFolder(IFilter filter, IPipelineInput input)
        {
            var context = AppBuildContext;

            //1. Clear target conf folder
            string targeConfFolder = context.GetConfResourceFodlerPath();
            Logger.Info($"Start clear folder that path is \"{targeConfFolder}\"!");
            EditorUtils.ClearAndCreateDirectory(targeConfFolder);
            AssetDatabase.Refresh();
            Logger.Info($"Clear folder that path is \"{targeConfFolder}\" completed!");

            //2.Copy git repository to streamAssets
            string sourceConfParentPath = context.GetConfLocalVerisonControlTargetFodlerPath();

            if (string.IsNullOrEmpty(sourceConfParentPath))
            {
                throw new DirectoryNotFoundException("Data_res");
            }

            string streamFolder = string.Concat(System.Environment.CurrentDirectory
                , Path.DirectorySeparatorChar
                , context.StreamingAssetsFolder);
            streamFolder = EditorUtils.OptimazePath(streamFolder);

            string desFolder = streamFolder + "/" + context.DataRes;
            desFolder = Path.GetFullPath(desFolder);
            desFolder = desFolder.Replace(@"\", @"/");

            string sourcePath = $"{sourceConfParentPath}/gen/csharp/Conf";

            //3.Write current data version
            string data_resVersion = File.ReadAllText($"{sourceConfParentPath}/res_data.ver",context.TextEncoding);
            context.AppInfoManifest.dataResVersion = data_resVersion;
            Logger.Info($"Current data_res version : {data_resVersion} .");

            string data_res_source_path = $"{sourceConfParentPath}/res_data.json";
            Logger.Info($"data_res_source_path : {data_res_source_path}");
            if (!File.Exists(data_res_source_path))
            {
                throw new FileNotFoundException(data_res_source_path);
            }
            string data_res_des_path = $"{streamFolder}/res_data.json";
            Logger.Info($"data_res_des_path : {data_res_des_path}");

            string data_res_des_foler_path = Path.GetDirectoryName(data_res_des_path);
            if (!Directory.Exists(data_res_des_foler_path))
                Directory.CreateDirectory(data_res_des_foler_path);
            if (File.Exists(data_res_des_path))
            {
                File.Delete(data_res_des_path);
            }
            File.Copy(data_res_source_path, data_res_des_path);
            Logger.Info($"Copy data_res version manifest file complete .");
            //4.Copy dataRes
            EditorUtils.CopyDirecotryToDestination(sourcePath, desFolder, x =>
            {
                if (x.EndsWith(".meta"))
                {
                    return true;
                }

                if (x.Contains("res_data.ver"))//过滤掉版本信息
                {
                    return true;
                }
                return false;
            });

            AssetDatabase.Refresh();
            Logger.Info($"Copy source conf folder data to the target path that value is \"{sourcePath}\"completed!");
        }

        #endregion
    }
}
