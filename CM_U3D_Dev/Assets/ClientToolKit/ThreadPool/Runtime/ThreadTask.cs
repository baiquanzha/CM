namespace MTool.ThreadPool.Runtime
{
    public enum ThreadTaskPriority
    {
        Urgent,
        Ordinary,
        Low,
    }

    public abstract class ThreadTask
    {
        public delegate void OnCallback(ThreadTask tb);
        public OnCallback OnInit;
        public OnCallback OnProcess;
        public OnCallback OnFinishProcess;
        public OnCallback OnError;
        public OnCallback OnStartProcess;

        public volatile bool IsStop = false;
        public bool IsError = false;

        public ThreadTaskPriority _Priority = ThreadTaskPriority.Urgent;

        public virtual void Init()
        {
            OnInit?.Invoke(this);
        }

        public virtual void Process()
        {
            OnProcess?.Invoke(this);
        }

        public virtual void Finish(bool isByClose = false)
        {
            if (IsError)
                OnError?.Invoke(this);
            else
                OnFinishProcess?.Invoke(this);
        }

        public virtual void OnStart()
        {
            OnStartProcess?.Invoke(this);
        }

        public void Error()
        {
            IsError = true;
        }

        public virtual void Stop(bool call_by_finish = false)
        {
            IsStop = true;
        }
    }
}
