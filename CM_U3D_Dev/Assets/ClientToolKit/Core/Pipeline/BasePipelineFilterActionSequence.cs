using System;
using System.Collections.Generic;

namespace MTool.Core.Pipeline
{
    public class BasePipelineFilterActionSequence : BaseFilterAction , IPipelineFilterActionSequence
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Func<IPipelineContext> _mGetContextFunc = null;
        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public Queue<IPipelineFilterAction> Actions { private set; get; } = new Queue<IPipelineFilterAction>(5);

        public override IPipelineContext Context
        {
            get => base.Context ?? (base.Context = this._mGetContextFunc());
            set => base.Context = value;
        }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public BasePipelineFilterActionSequence(Func<IPipelineContext> func)
        {
            this._mGetContextFunc = func;
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public override bool Test(IFilter filter, IPipelineInput input)
        {
            bool result = true;
            foreach (var pipelineFilterAction in Actions)
            {
                try
                {
                    Logger?.Debug($"Begin test action : {pipelineFilterAction.GetType().Name} .");
                    var pipelineFilterActionTestResult = pipelineFilterAction.Test(filter,input);
                    Logger?.Debug($"End test action : {pipelineFilterAction.GetType().Name} .");
                    if (!pipelineFilterActionTestResult)
                    {
                        Logger?.Error($"The action that name is \"{pipelineFilterAction.GetType().Name}\" test failure! .");
                    }

                    result &= pipelineFilterActionTestResult;
                }
                catch (Exception ex)
                {
                    Logger?.Error($"Error msg : {ex.Message} , stackTrace : {ex.StackTrace} .");
                    var context = this.Context;
                    context.AppendErrorLog(
                            $"Exception name : {ex.GetType().Name} . The action that name is {pipelineFilterAction.GetType().Name} is execute failure! error msg : {ex.Message} , " +
                            $"error stackTrace : {ex.StackTrace}");

                    return false;
                }
            }

            return result;
        }

        public override void BeforeExcute(IFilter filter, IPipelineInput input)
        {
            Logger?.Debug($"Begin excute action queue !");
        }

        public override void Execute(IFilter filter, IPipelineInput input)
        {
            this.State = ActionState.Normal;

            bool success = true;
            foreach (var pipelineFilterAction in Actions)
            {
                try
                {
                    pipelineFilterAction.State = ActionState.Normal;

                    pipelineFilterAction.BeforeExcute(filter, input);
                    pipelineFilterAction.Execute(filter, input);
                    pipelineFilterAction.EndExcute(filter, input);

                    var actionState = pipelineFilterAction.State;

                    if (actionState != ActionState.Completed)
                    {
                        success = false;

                        this.State = ActionState.Error;
                        var context = this.Context;
                        context.AppendErrorLog(
                                $"The action that name is {pipelineFilterAction.GetType().Name} is execute failure!");

                        break;
                    }
                }
                catch (Exception ex)
                {
                    success = false;
                    var context = this.Context;
                    context.AppendErrorLog(
                            $"Exception name : {ex.GetType().Name} . The action that name is {pipelineFilterAction.GetType().Name} is execute failure! error msg : {ex.Message} , " +
                            $"error stackTrace : {ex.StackTrace}");
                    this.State = ActionState.Error;
                    throw ex;
                }
            }

            if (success)
                this.State = ActionState.Completed;
        }

        public override void EndExcute(IFilter filter, IPipelineInput input)
        {
            Logger?.Debug($"End excute action queue ! ");
        }

        public void Enqueue(IPipelineFilterAction action)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }

            if (!this.Actions.Contains(action))
            {
                this.Actions.Enqueue(action);

                action.Context = this.Context;

                return;
            }

            this.State = ActionState.Error;

            Logger?.Error($"The action that you want to enque is exist in current actoin sequence! " +
                          $"Type name : {action.GetType().Name} . ");
        }

        #endregion

    }
}
