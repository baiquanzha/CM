using MTool.Core.FSM;
using MTool.LoggerModule.Runtime;

namespace MTool.AppUpdaterLib.Runtime.States
{
    internal abstract class BaseAppUpdaterFunctionalState : State<AppUpdaterFsmOwner>
    {
#if DEBUG_APP_UPDATER
        private string mStateDesc = null;
        public virtual string StateDesc
        {
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            get { return mStateDesc ?? (mStateDesc = this.GetType().Name); }
        }
#endif
        public AppUpdaterContext Context => this.Target.Context;

        private ILogger mLogger = null;

        protected override ILogger Logger
        {
            // ReSharper disable once ConvertToNullCoalescingCompoundAssignment
            get { return mLogger ?? (mLogger = LoggerManager.GetLogger(this.GetType().Name)); }
        }

        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
#if DEBUG_APP_UPDATER
            Context.StateName = this.StateDesc;
#endif
            base.Enter(entity, args);
        }

    }
}
