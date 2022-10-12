using MTool.Core.Collections;
using MTool.LoggerModule.Runtime;
using System;

namespace MTool.Core.ResourcePools
{
    public class ResourcePool<T> : ResourcePool where T : class
    {

        private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(
            ()=>LoggerManager.GetLogger(nameof(ResourcePool<T>)));

        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private readonly FastStack<T> _stack;
        private readonly Func<T> _create;
        private readonly Action<T> _initialize;
        private readonly Action<T> _uninitialize;
        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------


        public ResourcePool(Func<T> create , Action<T> initialize , Action<T> uninitialize)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }

            this._create = create;
            this._initialize = initialize;
            this._uninitialize = uninitialize;
            this._stack = new FastStack<T>();

            Register(this);
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public T Obtain()
        {
            // Re-use existing item or create a new item.
            T item = null;

            if (Enabled)
            {
                lock (_stack)
                {
                    item = _stack.Pop();
                }
            }

            if (item == null)
                item = _create();

            // Initialize item if necessary.
            _initialize?.Invoke(item);

            return item;
        }

        public void Recycle(T item)
        {
            if (item == null)
            {
                s_mLogger.Value.Error("ResourcePool.Recycle(item) should not be called with null.");
            }

            // Reset item if necessary.
            _uninitialize?.Invoke(item);

            if (Enabled)
            {
                lock (_stack)
                {
                    if (_stack.Contains(item))
                    {
                        s_mLogger.Value.Error("Cannot recycle item. Item is already in the resource pool.");
                    }
                    _stack.Push(item);
                }
            }
        }

        public override void Clear()
        {
            lock (_stack)
            {
                if (this._stack.Count > 0)
                {
                    this._stack.Clear();
                }
            }
        }


        public override string ToString()
        {
            return $"{this.GetType().Name} cache count : {this._stack.Count}";
        }

        #endregion


    }
}
