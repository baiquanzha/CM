using UnityEngine;
using MTool.Core.Pipeline;
using System.IO;

namespace MTool.AppBuilder.Editor.Builds.Actions.ResProcess
{
    public class AssetBundleAction : BaseBuildFilterAction
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
            string configPath = GetCurrentAssetGraphProjectConfigPath();

            Logger.Info($"AssetsGraph config path : \"{configPath}\" .");

            if (!File.Exists(configPath))
            {
                var appBuildContext = AppBuildContext;
                appBuildContext.ErrorSb.AppendLine($"The assetgraph config that path is \"{configPath}\" is not exist!");
                return false;
            }

            //var appSetting = Resources.Load<AppSetting>("AppSetting");
            //if (appSetting == null)
            //{
            //    var appBuildContext = AppBuildContext;
            //    appBuildContext.AppendErrorLog($"Pleause create a appSetting asset !");
            //    return false;
            //}
            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            string configPath = GetCurrentAssetGraphProjectConfigPath(true);
            Logger.Info($"AssetGraph configPath : \"{configPath}\" .");
            UnityEngine.AssetGraph.AssetGraphUtility.ExecuteGraph(configPath);
            this.State = ActionState.Completed;
        }


        private string GetCurrentAssetGraphProjectConfigPath(bool relative = false)
        {
            string targetConfigPath = AppBuildConfig.GetAppBuildConfigInst().TargetAssetGraphConfigAssetsPath;
            targetConfigPath = EditorUtils.OptimazePath(targetConfigPath, false);

            string filePath;
            if (relative)
            {
                filePath = targetConfigPath;
            }
            else
            {
                filePath = $"{Application.dataPath}/../{targetConfigPath}";
                filePath = EditorUtils.OptimazePath(filePath);
            }

            return filePath;
        }
    }
}
