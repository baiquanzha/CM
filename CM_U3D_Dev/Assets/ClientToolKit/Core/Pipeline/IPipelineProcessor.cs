namespace MTool.Core.Pipeline
{
    public interface IPipelineProcessor
    {
        IPipelineContext Context { get; }

        IPipelineProcessor Register(IFilter filter);

        bool TestAll(IPipelineInput input);

        ProcessResult Process(IPipelineInput input);
    }
}
