using MTool.Core.MessengerSystem;
using System.Threading;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal class AppUpdateFinalState : BaseAppUpdaterFunctionalState
    {
        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            base.Enter(entity, args);

            this.Target.OnCompletedCallback();
        }
    }
}
