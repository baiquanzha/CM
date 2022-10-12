using MTool.AppBuilder.Editor.Builds.Actions.ResProcess;
using MTool.Core.Pipeline;
using MTool.LoggerModule.Runtime;

namespace MTool.AppBuilder.Editor.Builds.Filters.Concrete
{
    public class ResourcesProcessFilter : QueueActionsPipelineFilter
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
        public ResourcesProcessFilter()
        {
        }

        public ResourcesProcessFilter(bool autoAddActions) : base(autoAddActions)
        {
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override void OnAutoAddActions()
        {
            //this.pSequence.Enqueue(new TestOtherFilesExportAction());
            //this.pSequence.Enqueue(new AssetBundleAction());
        }

        #endregion

    }
}
