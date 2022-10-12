namespace MTool.Core.Pipeline
{
    public enum ActionState : byte
    {
        /// <summary>
        /// Normal state
        /// </summary>
        Normal = 0,
        /// <summary>
        /// Error state
        /// </summary>
        Error = 1,
        /// <summary>
        /// The action is excute competed
        /// </summary>
        Completed = 2,
    }

    public interface IPipelineFilterAction
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        ActionState State { set;get; }

        IPipelineContext Context { set; get; }

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

        bool Test(IFilter filter, IPipelineInput input);

        void BeforeExcute(IFilter filter, IPipelineInput input);

        void Execute(IFilter filter , IPipelineInput input);

        void EndExcute(IFilter filter, IPipelineInput input);

        #endregion
    }
}
