namespace MTool.Core.Pipeline
{
    public enum FilterState : byte
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
        /// The filter is excute competed
        /// </summary>
        Completed = 2,
    }

    public interface IFilter
    {
        bool Enabled { get; }

        IPipelineProcessor Processor { set;get; }

        IFilter NextFilter { get; }

        FilterState State { get; }

        void OnPreProcess();

        bool Test(IPipelineInput input);

        void Execute(IPipelineInput input);

        void OnPostProcess();

        void Connect(IFilter nextFilter);
    }
}
