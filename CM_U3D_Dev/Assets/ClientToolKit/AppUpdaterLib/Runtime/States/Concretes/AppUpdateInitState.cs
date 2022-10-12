using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.Core.FSM;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal sealed class AppUpdateInitState : BaseAppUpdaterFunctionalState
    {
        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);

            if (!AppUpdaterHints.Instance.ManualPerformAppUpdate)
            {
                this.PerformAppUpdate();
            }
        }

        public override bool OnMessage(AppUpdaterFsmOwner entity, in IRoutedEventArgs eventArgs)
        {
            var eventType = (AppUpdaterInnerEventType)eventArgs.EventType;

            switch (eventType)
            {
                case AppUpdaterInnerEventType.PerformAppUpdate:
                    this.PerformAppUpdate();
                    return true;
            }

            return base.OnMessage(entity, in eventArgs);
        }

        private void PerformAppUpdate()
        {
            this.Target.State = AppUpdaterFsmOwner.AppUpdaterState.Runing;
            var appUpdaterConfig = AppUpdaterConfigManager.AppUpdaterConfig;
            if (appUpdaterConfig.skipAppUpdater)
            {
                Context.AppendInfo("Skip app updater !");
                this.Target.ChangeState<AppUpdateFinalState>();
            }
            else
            {
                Context.AppendInfo("AppUpdater start working!");
                this.Target.ChangeState<AppUpdateGetLighthouseConfigState>();
            }
        }

    }
}
