using System.Text;
using System.IO;
using MTool.AppUpdaterLib.Runtime.ResManifestParser;

namespace MTool.AppUpdaterLib.Runtime.States.Concretes
{
    internal class AppUpdateCompletedState : BaseAppUpdaterFunctionalState
    {
        public enum InnerState
        {
            Idle,

            StartLoadResManifest,

            LoadingResManifest,

            SaveRevision,

            UpdateCompleted,

            ResUpdateFailure,
        }

        private InnerState mCurState = InnerState.Idle;

        public override void Enter(AppUpdaterFsmOwner entity, params object[] args)
        {
            base.Enter(entity, args);

            this.mCurState = InnerState.StartLoadResManifest;
        }

        public override void Execute(AppUpdaterFsmOwner entity)
        {
            base.Execute(entity);
            switch (mCurState)
            {
                case InnerState.StartLoadResManifest:
                    this.LoadResManifest();
                    break;
                case InnerState.SaveRevision:
                    this.SaveRevision();
                    break;
                case InnerState.UpdateCompleted:
                    this.OnAppUpdateCompleted();
                    break;
                case InnerState.ResUpdateFailure:
                    this.OnResUpdateFailure();
                    break;
            }
        }

        private void LoadResManifest()
        {
            this.mCurState = InnerState.LoadingResManifest;
            string localManifestPath = AssetsFileSystem.GetWritePath(AssetsFileSystem.UnityResManifestName);
            if (File.Exists(localManifestPath))
            {
                var localContent = File.ReadAllText(localManifestPath);
                Context.LocalResManifest = VersionManifestParser.Parse(localContent);
            }
            string builtInManifestPath = AssetsFileSystem.GetStreamingAssetsPath(AssetsFileSystem.UnityResManifestName, null, false);
            this.Target.Request.Load(builtInManifestPath, OnLoadBuiltInResManifest);
        }

        private void OnLoadBuiltInResManifest(byte[] data)
        {
            if (data == null && data.Length == 0)
            {
                Context.ErrorType = AppUpdaterErrorType.LoadBuiltinResManifestFailure;
                this.mCurState = InnerState.ResUpdateFailure;
            }
            else
            {
                var content = new System.Text.UTF8Encoding(false, true).GetString(data);
                Context.BuiltInResManifest = VersionManifestParser.Parse(content);
                this.mCurState = InnerState.SaveRevision;
            }
        }

        private void SaveRevision()
        {
            UpdateResMap.RegenUpdateResMap(Context.BuiltInResManifest, Context.LocalResManifest);

            Context.CleanExpireResFile();

            Context.SaveAppRevision();

            Context.ProgressData.Progress = 1;

            mCurState = InnerState.UpdateCompleted;
        }

        private void OnAppUpdateCompleted()
        {
            this.Target.ChangeState<AppUpdateFinalState>();

            if (Context.ErrorType == AppUpdaterErrorType.None)
            {
                Context.AppendInfo("Resource update completed!");
            }
            else
            {
                Context.AppendInfo("Resource update failure!");
            }
        }

        private void OnResUpdateFailure()
        {
            this.Target.ChangeState<AppUpdateFailureState>();
        }

        public override void Exit(AppUpdaterFsmOwner entity)
        {
            base.Exit(entity);
#if UNITY_EDITOR
            this.mSb.Clear();
            this.mLogSb.Clear();
#endif
        }

        public override void Reset()
        {
            base.Reset();
            this.mCurState = InnerState.Idle;
        }

#if UNITY_EDITOR

        private StringBuilder mSb = new StringBuilder();
        private StringBuilder mLogSb = new StringBuilder();
        public override string ToString()
        {
            this.mSb.Length = 0;
            this.mSb.AppendLine("State : " + this.GetType().Name);
            this.mSb.AppendLine("Log : \n"+this.mLogSb.ToString());

            return this.mSb.ToString();
        }
#endif

    }
}
