using System.Collections.Generic;

namespace MTool.Core.Pipeline
{
    public interface IPipelineFilterActionSequence : IPipelineFilterAction
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        Queue<IPipelineFilterAction> Actions { get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        void Enqueue(IPipelineFilterAction action);


        #endregion
    }
}
