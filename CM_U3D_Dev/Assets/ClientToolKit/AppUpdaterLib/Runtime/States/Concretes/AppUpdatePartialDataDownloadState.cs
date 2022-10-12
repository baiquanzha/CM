using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MTool.AppUpdaterLib.Runtime.Managers;
using MTool.Core.FSM;

// ReSharper disable once CheckNamespace
namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    sealed class AppUpdatePartialDataDownloadState : BaseAppUpdaterFunctionalState
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

        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            this.Target.State = AppUpdaterFsmOwner.AppUpdaterState.Runing;
            base.Enter(entity, args);
        }

        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);
            StartUpdateRes();
        }

        private void StartUpdateRes()
        {
            Logger.Info("Start to check update resource !");
            VersionDesc info = new VersionDesc
            {
                Type = UpdateResourceType.NormalResource,
                LocalMd5 = AppVersionManager.AppInfo.unityDataResVersion,
                RemoteMd5 = AppVersionManager.AppInfo.unityDataResVersion,
            };

            Context.ResUpdateTarget.VersionDescs = new VersionDesc[]
            {
                info
            };

            Context.ResUpdateConfig.Mode = ResSyncMode.SUB_GROUP;
            Context.ResUpdateConfig.Filter = this.Target.FileUpdateRuleFilter;
            Context.ResUpdateTarget.CurrentResVersionIdx++;

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

        #endregion

    }
}
