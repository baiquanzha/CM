using System;
using MTool.LoggerModule.Runtime;
using log4net;

namespace MTool.Log4NetForUnity.Runtime
{
    public sealed class Log4NetLoggerWrap : ILogger
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private ILog _mInnerLogger;

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

        public Log4NetLoggerWrap(ILog innerLogger)
        {
            _mInnerLogger = innerLogger;
        }

        #endregion

        public void Debug(object message)
        {
            _mInnerLogger.Debug(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            _mInnerLogger.DebugFormat(format, args);
        }

        public void Info(object message)
        {
            _mInnerLogger.Info(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            _mInnerLogger.InfoFormat(format, args);
        }

        public void Warn(object message)
        {
            _mInnerLogger.Warn(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            _mInnerLogger.WarnFormat(format, args);
        }

        public void Error(object message)
        {
            _mInnerLogger.Error(message);
        }

        public void Error(object message, Exception exception)
        {
            _mInnerLogger.Error(message,exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            _mInnerLogger.ErrorFormat(format, args);
        }

        public void Fatal(object message)
        {
            _mInnerLogger.Fatal(message);
        }

        public void Fatal(object message, Exception exception)
        {
            _mInnerLogger.Fatal(message,exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            _mInnerLogger.FatalFormat(format, args);
        }
    }
}
