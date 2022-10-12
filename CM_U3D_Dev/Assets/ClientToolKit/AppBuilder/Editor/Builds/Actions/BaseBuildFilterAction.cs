using MTool.AppBuilder.Editor.Builds.Contexts;
using MTool.Core.Pipeline;
using MTool.LoggerModule.Runtime;

namespace MTool.AppBuilder.Editor.Builds.Actions
{
    public class BaseBuildFilterAction : BaseFilterAction 
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

        public override IPipelineContext Context
        {
            get => base.Context;
            set
            {
                base.Context = value;
                this.mAppBuildContext = value as AppBuildContext;
            }
        }

        private AppBuildContext mAppBuildContext;
        public AppBuildContext AppBuildContext => mAppBuildContext;

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            return false;
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
        }

        #endregion
    }
}
