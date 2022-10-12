using System;
using System.Text;
using System.Collections.Generic;
using MTool.Framework.Base;

namespace MTool.Framework.Network
{
    public enum ETracerLevel
    {
        DEBUG,
        TRACE,
        WARN,
        INFOR,
        ERROR,
        FATAL,
        MAX,
    }

    public delegate void TraceUpdateCallback(DateTime a_dataTime, ETracerLevel a_level, string a_context, string a_file, int a_line);

    public class Trace : Singleton<Trace>
    {
        class TracerItem
        {
            public DateTime m_dataTime;
            public ETracerLevel m_level;
            public string m_file;
            public int m_line;
            public string m_context;
        }

        private Queue<TracerItem> m_traceQueue = new Queue<TracerItem>();
        private const int m_indexFrame = 1;

        public Trace()
        {
        }

        public ETracerLevel Level { get; set; } = ETracerLevel.DEBUG;

        public void update(TraceUpdateCallback a_traceUpdateCb)
        {
            lock (m_traceQueue)
            {
                while (m_traceQueue.Count > 0)
                {
                    TracerItem item = m_traceQueue.Dequeue();
                    if (item == null)
                    {
                        break;
                    }
                    a_traceUpdateCb(item.m_dataTime, item.m_level, item.m_context, item.m_file, item.m_line);
                }
            }
        }

        public void debug(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.DEBUG)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.DEBUG, context, __FILE__, __LINE__);
        }

        public void debug(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.DEBUG)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.DEBUG, context, __FILE__, __LINE__);
        }

        public void debug(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.DEBUG)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.DEBUG, context, __FILE__, __LINE__);
        }

        public void debug(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.DEBUG)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.DEBUG, context, __FILE__, __LINE__);
        }

        public void trace(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.TRACE)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.TRACE, context, __FILE__, __LINE__);
        }

        public void trace(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.TRACE)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.TRACE, context, __FILE__, __LINE__);
        }

        public void trace(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.TRACE)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.TRACE, context, __FILE__, __LINE__);
        }

        public void trace(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.TRACE)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.TRACE, context, __FILE__, __LINE__);
        }


        public void warn(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.WARN)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.WARN, context, __FILE__, __LINE__);
        }

        public void warn(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.WARN)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.WARN, context, __FILE__, __LINE__);
        }

        public void warn(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.WARN)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.WARN, context, __FILE__, __LINE__);
        }

        public void warn(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.WARN)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.WARN, context, __FILE__, __LINE__);
        }


        public void infor(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.INFOR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.INFOR, context, __FILE__, __LINE__);
        }

        public void infor(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.INFOR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.INFOR, context, __FILE__, __LINE__);
        }

        public void infor(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.INFOR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.INFOR, context, __FILE__, __LINE__);
        }

        public void infor(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.INFOR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.INFOR, context, __FILE__, __LINE__);
        }

        public void error(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.ERROR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.ERROR, context, __FILE__, __LINE__);
        }

        public void error(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.ERROR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.ERROR, context, __FILE__, __LINE__);
        }

        public void error(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.ERROR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.ERROR, context, __FILE__, __LINE__);
        }

        public void error(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.ERROR)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.ERROR, context, __FILE__, __LINE__);
        }

        public void fatal(string a_format, object a_arg0)
        {
            if (Level > ETracerLevel.FATAL)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0);

            _trace(ETracerLevel.FATAL, context, __FILE__, __LINE__);
        }

        public void fatal(string a_format, object a_arg0, object a_arg1)
        {
            if (Level > ETracerLevel.FATAL)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1);

            _trace(ETracerLevel.FATAL, context, __FILE__, __LINE__);
        }

        public void fatal(string a_format, object a_arg0, object a_arg1, object a_arg2)
        {
            if (Level > ETracerLevel.FATAL)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_arg0, a_arg1, a_arg2);

            _trace(ETracerLevel.FATAL, context, __FILE__, __LINE__);
        }

        public void fatal(string a_format, params object[] a_args)
        {
            if (Level > ETracerLevel.FATAL)
            {
                return;
            }
            System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);

            string __FILE__ = "";
            int __LINE__ = 0;
            System.Diagnostics.StackFrame frame = stackTrace.GetFrame(m_indexFrame);
            if (null != frame)
            {
                __FILE__ = frame.GetFileName();
                __LINE__ = frame.GetFileLineNumber();
            }

            string context = string.Format(a_format, a_args);

            _trace(ETracerLevel.FATAL, context, __FILE__, __LINE__);
        }

        private void _trace(ETracerLevel a_level, string a_context, string a_file, int a_line)
        {
            lock (m_traceQueue)
            {
                TracerItem item = new TracerItem();
                if (item == null)
                {
                    return;
                }
                item.m_dataTime = DateTime.Now;
                item.m_level = a_level;
                item.m_file = a_file;
                item.m_line = a_line;
                item.m_context = a_context;

                m_traceQueue.Enqueue(item);
            }
        }
    }
}