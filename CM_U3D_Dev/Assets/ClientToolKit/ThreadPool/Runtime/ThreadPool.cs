using System;
using System.Collections.Generic;
using System.Threading;

namespace MTool.ThreadPool.Runtime
{
    public class ThreadPool
    {
        private Queue<ThreadTask> mTaskQueue;
        private Queue<WorkerThread> mThreads;

        private LinkedList<ThreadTask> mFinishList = new LinkedList<ThreadTask>();
        private LinkedList<ThreadTask> mStartedList = new LinkedList<ThreadTask>();

        //private AutoResetEvent mEvent = new AutoResetEvent(false);
        private static ThreadPool mInstance;

        public static ThreadPool Instance
        {
            get
            {
                if (mInstance == null)
                {
                    mInstance = new ThreadPool();
                    //mInstance.Start();
                }

                return mInstance;
            }
        }


        public static bool HasInstance()
        {
            return mInstance != null;
        }

        ThreadPool()
        {
            mTaskQueue = new Queue<ThreadTask>();
        }


        void Start(int threadNum = 5)
        {
            CreateThreadPool(threadNum);
        }

        public void AddTask(ThreadTask task)
        {
            if (task == null)
                return;

            lock (mTaskQueue)
            {
                mTaskQueue.Enqueue(task);
            }

            //mEvent.Reset();
        }

        public void Close()
        {
            while (mThreads.Count != 0)
            {
                try
                {
                    WorkerThread workthread = mThreads.Dequeue();
                    workthread.CloseThread();
                    continue;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                break;
            }
        }

        public bool IsClose()
        {
            if (mThreads == null || mThreads.Count == 0)
                return true;
            return false;
        }

        public void ReStart()
        {
            Start();
        }

        //public void Wait()
        //{
        //    mEvent.WaitOne();
        //}

        public void AddToStartList(ThreadTask task)
        {
            lock (mStartedList)
            {
                mStartedList.AddLast(task);
            }
        }

        public void AddToFinishList(ThreadTask task)
        {
            lock (mFinishList)
            {
                mFinishList.AddLast(task);
            }
        }

        ThreadTask PopFinish()
        {
            ThreadTask ret = null;
            lock (mFinishList)
            {
                if (mFinishList.Count > 0)
                {
                    ret = mFinishList.First.Value;
                    mFinishList.RemoveFirst();
                }
            }

            return ret;
        }

        ThreadTask PopStarted()
        {
            ThreadTask ret = null;
            lock (mStartedList)
            {
                if (mStartedList.Count > 0)
                {
                    ret = mStartedList.First.Value;
                    mStartedList.RemoveFirst();
                }
            }

            return ret;
        }

        public void Update()
        {
            int numProceed = 0;
            while (true)
            {
                var f = PopStarted();
                if (f != null)
                {
                    f.OnStart();
                    if (++numProceed >= 8) // process 8 max per update
                        break;

                }
                else
                {
                    break;
                }
            }

            numProceed = 0;

            while (true)
            {
                var f = PopFinish();
                if (f != null)
                {
                    f.Finish();
                    if (++numProceed >= 8) // process 8 max per update
                        break;

                }
                else
                {
                    break;
                }
            }
        }

        private void CreateThreadPool(int threadNum)
        {
            if (mThreads == null)
                mThreads = new Queue<WorkerThread>();

            for (int i = 0; i < threadNum; i++)
            {
                WorkerThread thread = new WorkerThread(this, ref mTaskQueue, i + 1);
                mThreads.Enqueue(thread);
            }
        }
    }
}