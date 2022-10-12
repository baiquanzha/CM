using System;
using System.IO;
using System.Xml;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using UnityEngine;

namespace MTool.Log4NetForUnity.Runtime
{
    public class UnityDefaultLogAppender : AppenderSkeleton
    {

        private static readonly int ErrorLevel = Level.Error.Value;
        private static readonly int WarnLevel = Level.Warn.Value;

        protected override void Append(LoggingEvent loggingEvent)
        {
            var level = loggingEvent.Level;
            
            if(level == null) return;
            
            string message;

            try
            {
                message = RenderLoggingEvent(loggingEvent);
            }
            catch (Exception e)
            {
                Debug.LogException(e, null);
                return;
            }

            if (level.Value < WarnLevel)
            {
                Debug.Log(message);
            }
            else if (level.Value >= WarnLevel && level.Value < ErrorLevel)
            {
                Debug.LogWarning(message);
            }
            else if(level.Value >= ErrorLevel)
            {
                Debug.LogError(message);
            }
        }
    }
}