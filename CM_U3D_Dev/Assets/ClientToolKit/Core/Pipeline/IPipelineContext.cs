namespace MTool.Core.Pipeline
{
    public interface IPipelineContext
    {
        string Error { get; }

        void AppendErrorLog(string message);
    }
}
