using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.Core.FSM;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal sealed class AppUpdaterGlobalState : BaseAppUpdaterFunctionalState
    {

        public override bool OnMessage(AppUpdaterFsmOwner entity, in IRoutedEventArgs eventArgs)
        {
            var evtType = (AppUpdaterInnerEventType)eventArgs.EventType;
            Logger.Debug($"AppUpdater receive msg , type : {evtType.ToString()}");
            switch (evtType)
            {
                case AppUpdaterInnerEventType.OnCurrentResUpdateCompleted:
                    this.CheckResUpdateWasDone();
                    return true;
                case AppUpdaterInnerEventType.StartPerformResUpdateOperation:
                    this.CheckResUpdateWasDone();
                    return true;
                case AppUpdaterInnerEventType.StartPerformResPartialUpdateOperation:
                    if (AppUpdaterConfigManager.AppUpdaterConfig.skipAppUpdater)
                    {
                        Logger.Warn("You can't download partial data resources, because current runing mode will skip appupdate operation!");
                        if (AppUpdaterHints.Instance.EnableCheckMissingRes)
                            this.Target.ChangeState<AppCheckMissingResState>();
                        else
                            this.Target.ChangeState<AppUpdateCompletedState>();
                    }
                    else
                    {
                        this.Target.ChangeState<AppUpdatePartialDataDownloadState>();
                    }
                    return true;
            }
            return base.OnMessage(entity, in eventArgs);
        }


        private void CheckResUpdateWasDone()
        {
            Context.ResUpdateTarget.CurrentResVersionIdx++;
            if (Context.IsUpdateCompleted())// Resource update was Done!
            {
                if (AppUpdaterHints.Instance.EnableCheckMissingRes)
                    this.Target.ChangeState<AppCheckMissingResState>();
                else
                    this.Target.ChangeState<AppUpdateCompletedState>();
            }
            else
            {
                this.PerformNextResUpdateOperation();
            }
        }

        private void PerformNextResUpdateOperation()
        {
            if (Context.DownloadMode == AppUpdateDownloadMode.SingleThread)
                this.Target.ChangeState<AppUpdateDataResState>();
            else
                this.Target.ChangeState<AppUpdateMTDataResState>();
            IRoutedEventArgs arg = new RoutedEventArgs()
            {
                EventType = (int)AppUpdaterInnerEventType.PerformResUpdateOperation
            };
            this.Target.HandleMessage(in arg);
        }

    }
}
