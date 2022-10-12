using MTool.LoggerModule.Runtime;

namespace MTool.Core.Pipeline
{
    public abstract class BaseFilterAction : IPipelineFilterAction
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        private ILogger mLogger;
        protected virtual ILogger Logger
        {
            get
            {
                if (mLogger == null)
                {
                    mLogger = LoggerManager.GetLogger(this.GetType().Name);
                }
                return mLogger;
            }
        }

        public ActionState State { get; set; }

        public virtual IPipelineContext Context { set; get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public virtual void BeforeExcute(IFilter filter, IPipelineInput input)
        {
            Logger?.Debug($"Begin excute action :{this.GetType().Name} ");
        }

        public abstract bool Test(IFilter filter, IPipelineInput input);

        public abstract void Execute(IFilter filter, IPipelineInput input);

        public virtual void EndExcute(IFilter filter, IPipelineInput input)
        {
            Logger?.Debug($"End excute action :{this.GetType().Name}");
        }

        #endregion
    }
}
