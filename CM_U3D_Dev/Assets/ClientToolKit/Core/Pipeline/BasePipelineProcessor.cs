using System.Collections.Generic;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.Core.Pipeline
{
    public class BasePipelineProcessor : IPipelineProcessor
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------
        protected virtual ILogger Logger { get; }
        protected readonly LinkedList<IFilter> _filters = new LinkedList<IFilter>();

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public virtual IPipelineContext Context { get; }

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public virtual IPipelineProcessor Register(IFilter filter)
        {
            filter.Processor = this;
            if (_filters.Count == 0)
            {
                this._filters.AddFirst(filter);
            }
            else
            {
                var lastFilter = this._filters.Last;
                lastFilter.Value.Connect(filter);
                this._filters.AddLast(filter);
            }

            return this;
        }

        public bool TestAll(IPipelineInput input)
        {
            var headFilterNode = this._filters.First;
            var curProcessNode = headFilterNode;

            if (curProcessNode == null)
            {
                Logger?.Warn("The current pipeline processor has no pipeline filters .");

                return true;
            }

            bool testResult = true;

            do
            {
                var curFilter = curProcessNode.Value;

                bool curTestResult = true;
                if (curFilter.Enabled)
                {
                    curTestResult = curFilter.Test(input);

                    if (curTestResult)
                    {
                        Logger?.Debug($"Test filter that name is \"{curFilter.GetType().Name}\" test success .");
                    }
                    else
                    {
                        Logger?.Error($"Test filter that name is \"{curFilter.GetType().Name}\" test failure .");
                    }
                }
                else
                {
                    Logger?.Warn($"Skip test filter : {curFilter.GetType().Name}");
                }

                testResult &= curTestResult;

                
                curProcessNode = curProcessNode.Next;
            } while (curProcessNode != null);

            return testResult;
        }

        public virtual ProcessResult Process(IPipelineInput input)
        {
            var headFilterNode = this._filters.First;
            var curProcessNode = headFilterNode;

            if (curProcessNode == null)
            {
                return ProcessResult.Create(ProcessState.Error, "The pipeline has not filter to process !");
            }

            ProcessResult result = ProcessResult.Create(ProcessState.Completed);

            do
            {
                var curFilter = curProcessNode.Value;

                if (curFilter.Enabled)
                {
                    curFilter.OnPreProcess();

                    curFilter.Execute(input);

                    if (curFilter.State == FilterState.Completed)
                    {
                        curFilter.OnPostProcess();
                    }
                    else
                    {
                        result = ProcessResult.Create(ProcessState.Error, $"Filter : {curFilter.GetType().Name} is work error ! Error message : {Context.Error}");

                        break;
                    }
                }
                else
                {
                    Logger?.Warn($"Skip execute filter : {curFilter.GetType().Name}");
                }

                curProcessNode = curProcessNode.Next;
            } while (curProcessNode != null);

            return result;
        }

        #endregion
    }
}
