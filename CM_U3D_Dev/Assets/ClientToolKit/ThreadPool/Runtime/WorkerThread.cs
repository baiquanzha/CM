using System;
using System.Collections.Generic;
using System.Threading;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

namespace MTool.ThreadPool.Runtime
{
    public class WorkerThread
    {
        private int mThreadId;
        private volatile bool mFlag;
        private Queue<ThreadTask> mTaskQueue;
        private ThreadTask mTask;
        private Thread mThread = null;
        private ThreadPool mThreadPool;

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
            LoggerManager.GetLogger("WorkerThread"));

        public WorkerThread(ThreadPool pool, ref Queue<ThreadTask> queue, int id)
        {
            mThreadPool = pool;
            mTaskQueue = queue;
            mThreadId = id;
            mFlag = true;
            mThread = new Thread(OnRun);
            mThread.Start();
        }

        private void OnRun()
        {
            while (mFlag)
            {
                lock (mTaskQueue)
                {
                    try
                    {
                        if (mTaskQueue.Count > 0)
                            mTask = mTaskQueue.Dequeue();
                        else
                            mTask = null;
                    }
                    catch (Exception e)
                    {
                        mTask = null;
                        s_mLogger.Value?.Warn($"exception:{e.Message}, stack:{e.StackTrace}");
                    }
                    if (mTask == null)
                        continue;
                }

                try
                {
                    if (!mTask.IsStop)
                    {
                        mThreadPool.AddToStartList(mTask);
                        mTask.Process();
                        mThreadPool.AddToFinishList(mTask);
                    }
                }
                catch (Exception e)
                {
                    s_mLogger.Value?.Warn($"exception:{e.Message}, stack:{e.StackTrace}");
                }
            }
        }

        public void CloseThread()
        {
            mFlag = false;
        }
    }
}