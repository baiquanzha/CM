using System.IO;
using UnityEngine;
using MTool.Core.Pipeline;
using MTool.AppUpdaterLib.Runtime;
using MTool.AppBuilder.Editor.Builds.Contexts;
#if DEBUG_FILE_CRYPTION
using File = MTool.Core.IO.File;
#else
using File = System.IO.File;
#endif

namespace MTool.AppBuilder.Editor.Builds.Actions.ResPack
{
    public class SaveBaseVersionUnityResInfoAction : BaseBuildFilterAction
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
            this.SaveBulidVersionInfo(filter, input);
            this.State = ActionState.Completed;
        }
        public void SaveBulidVersionInfo(IFilter filter, IPipelineInput input)
        {
            var context = AppBuildContext;
            LastBuildVersion lastVersion = new LastBuildVersion();
            lastVersion.Version = context.AppInfoManifest.unityDataResVersion;

            lastVersion.Info = context.VersionManifest;
            string targetFile = context.LastBuildUnityResManifestPath;
            var dat = context.ToJson(lastVersion);
            File.WriteAllBytes(targetFile, System.Text.Encoding.UTF8.GetBytes(dat));
            Logger.Info($"Save file \"{targetFile}\" completed!");
        }

        #endregion


    }
}
