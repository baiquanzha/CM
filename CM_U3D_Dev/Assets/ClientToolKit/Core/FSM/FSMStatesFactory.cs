using System;
using System.Collections.Generic;

namespace MTool.Core.FSM
{
    public sealed class FSMStatesFactory<T> where T : IFSMOwner , new()
    {

        private static readonly Dictionary<Type, Queue<State<T>>> mDic = new Dictionary<Type, Queue<State<T>>>();

        public static V CreateState<V>() where V : State<T>, new()
        {
            V v = null;

            Type type = typeof(V);

            if (mDic.ContainsKey(type))
            {
                Queue<State<T>> queue = mDic[type];

                if (queue.Count > 0)
                {
                    v = queue.Dequeue() as V;
                }
            }

            if (v != null)
            {
                v.Reset();
            }
            else
            {
                v = new V();
            }

            v.internalCode = StateInternalCode.IN_USEING;

            return v;
        }

        public static State<T> CreateState(System.Type type)
        {
            State<T> s = null;
            if (mDic.ContainsKey(type))
            {
                Queue<State<T>> queue = mDic[type];

                if (queue.Count > 0)
                {
                    s = queue.Dequeue() as State<T>;
                }
            }

            if (s != null)
            {
                s.Reset();
            }
            else
            {
                s = System.Activator.CreateInstance(type) as State<T>;
            }

            s.internalCode = StateInternalCode.IN_USEING;

            return s;
        }


        public static void ReturnState(State<T> state)
        {
            if (state == null)
            {
                return;
            }

            Type type = state.GetType();

            if (!mDic.ContainsKey(type))
            {
                Queue<State<T>> queue = new Queue<State<T>>();

                queue.Enqueue(state);

                mDic.Add(type, queue);
            }
            else
            {
                Queue<State<T>> queue = mDic[type];

                queue.Enqueue(state);
            }

            state.internalCode = StateInternalCode.NOT_IN_USEING;
        }

        public static void Clear()
        {
            int count = mDic.Count;

            if (count > 0)
            {
                foreach (var kvp in mDic)
                {
                    kvp.Value.Clear();
                }

                mDic.Clear();
            }
        }

    }
}
