using MTool.LoggerModule.Runtime;

namespace MTool.Core.FSM
{
    public enum StateInternalCode
    {
        UN_KNOW,//未知
        IN_USEING,//正在被使用
        NOT_IN_USEING,//已经被回收
    }

    public abstract class State<T> where T : IFSMOwner, new()
    {
        protected abstract ILogger Logger { get; }

        private StateInternalCode mInternalCode = StateInternalCode.UN_KNOW;

        public StateInternalCode internalCode
        {
            set { mInternalCode = value; }
            get { return mInternalCode; }
        }


        private T mTarget;

        /// <summary>
        /// 持有该状态的实体对象
        /// </summary>
        public T Target
        {
            set { mTarget = value; }
            get { return mTarget; }
        }

        /// <summary>
        /// 进入该状态时调用，该状态会在实体进入该状态时调用1次
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Enter(T entity, params object[] args)
        {
            Logger?.Debug(string.Format("State : {0} is enter!", GetType().Name));
        }

        /// <summary>
        /// 处在该状态下，实体会在每个循自动环调用一次该方法
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Execute(T entity) { }

        /// <summary>
        /// 退出该状态时调用
        /// </summary>
        /// <param name="entity"></param>
        public virtual void Exit(T entity)
        {
            Logger?.Debug(string.Format("State : {0} is exit!", GetType().Name));
        }

        /// <summary>
        /// 消息处理
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="telegram"></param>
        /// <returns></returns>
        public virtual bool OnMessage(T entity, in IRoutedEventArgs eventArgs) { return false; }

        /// <summary>
        /// 状态重置
        /// </summary>
        public virtual void Reset() { }

    }
}
