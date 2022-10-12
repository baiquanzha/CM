using System;

namespace MTool.LoggerModule.Runtime
{
    public interface ILogger
    {
        #region Debug

        void Debug(object message);

        void DebugFormat(string format, params object[] args);

        #endregion


        #region Info

        void Info(object message);

        void InfoFormat(string format, params object[] args);

        #endregion

        #region Warn

        void Warn(object message);

        void WarnFormat(string format, params object[] args);

        #endregion


        #region Error

        void Error(object message);

        void Error(object message, Exception exception);

        void ErrorFormat(string format, params object[] args);

        #endregion


        #region Fatal

        void Fatal(object message);

        void Fatal(object message, Exception exception);

        void FatalFormat(string format, params object[] args);


        #endregion
    }
}
