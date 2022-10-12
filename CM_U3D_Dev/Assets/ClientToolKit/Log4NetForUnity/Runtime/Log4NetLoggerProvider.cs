using MTool.LoggerModule.Runtime;
using log4net;

namespace MTool.Log4NetForUnity.Runtime
{
    public sealed class Log4NetLoggerProvider : ILoggerProvider
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
            var logger = LogManager.GetLogger(name);

            return new Log4NetLoggerWrap(logger);
        }

        public void Shutdown()
        {
            LogManager.Shutdown();
        }

        public string Name => "Log4NetForUnity";
    }
}
