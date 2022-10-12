using System;
using MTool.AppUpdaterLib.Runtime.Helps;
using MTool.AppUpdaterLib.Runtime.Managers;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal class AppUpdateFailureState : BaseAppUpdaterFunctionalState
    {
        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            base.Enter(entity, args);

            var errorType = Context.ErrorType;

            if (errorType == AppUpdaterErrorType.None)
                throw new InvalidOperationException(errorType.ToString());
            if (errorType == AppUpdaterErrorType.LighthouseConfigServersIsUnReachable)
            {
                this.Target.OnForceUpdateCallBack(AppVersionManager.LHConfig.UpdateData);
                Context.AppendInfo("App Need update to latest , current has no server to reachable !");
            }
            else if (errorType == AppUpdaterErrorType.RequestGetVersionFailure)
            {
                if (AppVersionManager.LHConfig != null)
                {
                    var maintenance = AppVersionManager.LHConfig.GetMaintenanceInfo();
                    if (maintenance != null)
                    {
                        if (maintenance.IsOpen)
                        {
                            Logger.Info("The server is in maintence .");
                            this.Target.OnMaintenanceCallBack(maintenance);
                            Context.AppendInfo("The server is in maintence .");
                            return;
                        }
                        else if (maintenance.ForceUpdate)
                        {
                            Logger.Info(" The current app client is too old , call app update function!");
                            this.Target.OnForceUpdateCallBack(AppVersionManager.LHConfig.UpdateData);
                            Context.AppendInfo(" The current app client is too old , call app update function!");
                            return;
                        }
                    }
                }
                this.Target.OnErrorCallback(errorType, ErrorTypeHelper.GetErrorString(errorType));
                Context.AppendInfo("App updater failure");
            }
            else
            {
                this.Target.OnErrorCallback(errorType, ErrorTypeHelper.GetErrorString(errorType));
                Context.AppendInfo("App updater failure");
            }
        }
    }
}
