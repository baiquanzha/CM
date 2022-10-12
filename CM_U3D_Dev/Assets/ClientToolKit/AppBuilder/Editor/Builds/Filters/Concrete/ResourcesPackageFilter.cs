using MTool.AppBuilder.Editor.Builds.Actions.ResPack;
using MTool.Core.Pipeline;
using MTool.LoggerModule.Runtime;

namespace MTool.AppBuilder.Editor.Builds.Filters.Concrete
{
    public class ResourcesPackageFilter : QueueActionsPipelineFilter
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        protected override ILogger Logger
        {
            get
            {
                if (s_mlogger == null)
                    s_mlogger = LoggerManager.GetLogger(this.GetType().Name);
                return s_mlogger;
            }
        }
        private static ILogger s_mlogger;

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public ResourcesPackageFilter()
        {
        }

        public ResourcesPackageFilter(bool autoAddActions) : base(autoAddActions)
        {
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override void OnAutoAddActions()
        {
            //this.pSequence.Enqueue(new AssetBundleFilesPackAction());
            //this.pSequence.Enqueue(new FileVersionManifestGenerateAction());
        }


        #endregion

    }
}
