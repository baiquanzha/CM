namespace MTool.Core.Pipeline
{
    public abstract class QueueActionsPipelineFilter : BasePipelineFilter, IFilter
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        protected bool autoAddActions = false;

        protected readonly BasePipelineFilterActionSequence pSequence = null;

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public QueueActionsPipelineFilter()
        {
            this.pSequence = new BasePipelineFilterActionSequence(() => this.Processor.Context);
            this.pAction = pSequence;

        }

        public QueueActionsPipelineFilter(bool autoAddActions) : this()
        {
            this.autoAddActions = autoAddActions;

            if (this.autoAddActions)
            {
                this.OnAutoAddActions();
            }
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public virtual void OnAutoAddActions()
        {

        }

        public void Enqueue(IPipelineFilterAction action)
        {
            action.Context = this.Processor.Context;
            this.pSequence.Enqueue(action);
        }

        #endregion

    }
}
