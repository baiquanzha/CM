using System.IO;
using MTool.AppBuilder.Runtime.Exceptions;
using MTool.Core.Pipeline;
using MTool.AppBuilder.Editor;
using Version = MTool.AppUpdaterLib.Runtime.Version;

namespace MTool.AppBuilder.Editor.Builds.Actions.AppPrepare
{
    public class MakeBaseVerionSetupAction : BaseBuildFilterAction
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


        private bool CheckAppVersionValid()
        {
            var appVersion = AppBuildConfig.GetAppBuildConfigInst().targetAppVersion;
            Logger.Info($"The version from build config is \"{appVersion.Major}.{appVersion.Minor}.{appVersion.Patch}\" .");
            
            var versionStr = $"{appVersion.Major}.{appVersion.Minor}.0";

            var targetVersion = new Version(versionStr);

            var lastVersionInfo = AppBuildContext.GetLastBuildInfo();
            if (lastVersionInfo != null && lastVersionInfo.GetCurrentBuildInfo() != null)
            {
                var buildInfo = lastVersionInfo.GetCurrentBuildInfo();
                var lastVersion = new Version(buildInfo.versionInfo.version);
                Logger.Info($"The last app version :  {lastVersion.GetVersionString()} .");

                var result = targetVersion.CompareTo(lastVersion);

                if (result < Version.VersionCompareResult.HigherForMinor)//次版本（Minor）本次必须一样或更高
                {
                    Logger.Error($"The target version that value is \"{appVersion.Major}.{appVersion.Minor}.0\" " +
                                 $"is lower or equal to last build ,last build verison is \"" +
                                 $"{lastVersion.GetVersionString()}\" .");
                    return false;
                }
            }
            else
            {
                Logger.Info($"The last build info is not exist .");
            }
            return true;
        }


        //private bool CheckGameConfigs()
        //{
        //    var configsPath = AppBuildConfig.GetAppBuildConfigInst().GameTableDataConfigPath;
        //    if (!Directory.Exists(configsPath))
        //    {
        //        Logger.Error($"The table config git repo that path is \"{configsPath}\" is not exist !" +
        //                     $" Pleause specify a valid path!");
        //        return false;
        //    }

        //    return true;
        //}

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            if (!CheckAppVersionValid())
            {
                var appVersion = AppBuildConfig.GetAppBuildConfigInst().targetAppVersion;
                var versionStr = $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Patch}";
                AppBuildContext.AppendErrorLog($"Invalid target version : {versionStr}.");
                return false;
            }

            //if (!CheckGameConfigs())
            //{
            //    return false;
            //}

            return true;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.Setup(filter,input);
            this.State = ActionState.Completed;
        }


        private void Setup(IFilter filter, IPipelineInput input)
        {
            input.SetData(EnvironmentVariables.MAKE_BASE_APP_VERSION_KEY, true);
        }

        #endregion

    }
}