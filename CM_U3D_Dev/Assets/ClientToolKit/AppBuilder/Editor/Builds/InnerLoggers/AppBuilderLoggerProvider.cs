using MTool.LoggerModule.Runtime;

namespace MTool.AppBuilder.Editor.Builds.InnerLoggers
{
    public class AppBuilderLoggerProvider : ILoggerProvider
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

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

        #endregion

        public ILogger GetLogger(string name)
        {
            return new AppBuilderLogger();
        }

        public void Shutdown()
        {
            
        }

        public string Name { get; } = $"{nameof(AppBuilderLoggerProvider)}";
    }
}
