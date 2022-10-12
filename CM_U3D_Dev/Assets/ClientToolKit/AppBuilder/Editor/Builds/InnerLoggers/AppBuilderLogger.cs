using System;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
using UnityEngine;
using UnityDebug = UnityEngine.Debug;

namespace MTool.AppBuilder.Editor.Builds.InnerLoggers
{
    public sealed class AppBuilderLogger : ILogger
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

        public void Debug(object message)
        {
            UnityDebug.Log(message);
        }

        public void DebugFormat(string format, params object[] args)
        {
            UnityDebug.LogFormat(format,args);
        }

        public void Info(object message)
        {
            UnityDebug.Log(message);
        }

        public void InfoFormat(string format, params object[] args)
        {
            UnityDebug.LogFormat(format,args);
        }

        public void Warn(object message)
        {
            UnityDebug.LogWarning(message);
        }

        public void WarnFormat(string format, params object[] args)
        {
            UnityDebug.LogWarningFormat(format,args);
        }

        public void Error(object message)
        {
            UnityDebug.LogError(message);
        }

        public void Error(object message, Exception exception)
        {
            UnityDebug.LogError(message);
            UnityDebug.LogException(exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(format,args);
        }

        public void Fatal(object message)
        {
            UnityDebug.LogError(message);
        }

        public void Fatal(object message, Exception exception)
        {
            UnityDebug.LogError(message);
            UnityDebug.LogException(exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            UnityDebug.LogErrorFormat(format,args);
        }
    }
}
