using System;

namespace MTool.Core.FSM
{
    public class StateMachine<T> where T : IFSMOwner, new()
    {
        /// <summary>
        /// 实体持有的状态机实例 
        /// </summary>
        private T m_pOwner;

        /// <summary>
        /// 实体当前状态
        /// </summary>
        private State<T> m_pCurrentState;

        public State<T> CurrentState
        {
            set { m_pCurrentState = value; }
            get { return m_pCurrentState; }
        }

        /// <summary>
        /// 实体上一次状态
        /// </summary>
        private State<T> m_pPreviousState;

        public State<T> PreviousState
        {
            set { m_pPreviousState = value; }
            get { return m_pPreviousState; }
        }

        /// <summary>
        /// 实体全局状态
        /// </summary>
        private State<T> m_pGlobalState;

        public State<T> GlobalState
        {
            set { m_pGlobalState = value; }
            get { return m_pGlobalState; }
        }


        public StateMachine(T owner)
        {
            m_pOwner = owner;

            m_pCurrentState = null;

            m_pPreviousState = null;

            m_pGlobalState = null;
        }


        public void SetCurrentState(State<T> s)
        {
            m_pCurrentState = s;

            m_pCurrentState.Target = m_pOwner;

            m_pCurrentState.Enter(m_pOwner);
        }

        public void SetCurrentState<V>() where V : State<T>, new()
        {
            V s = FSMStatesFactory<T>.CreateState<V>();

            SetCurrentState(s);
        }

        public void SetCurrentState(Type stateType)
        {
            if (!(stateType is State<T>))
            {
                //Log error!
                return;
            }

            State<T> state = FSMStatesFactory<T>.CreateState(stateType);

            SetCurrentState(state);
        }


        public void SetGlobalState(State<T> s)
        {
            m_pGlobalState = s;

            m_pGlobalState.Target = m_pOwner;

            m_pGlobalState.Enter(m_pOwner);
        }

        public void SetGlobalState<V>() where V : State<T>, new()
        {
            State<T> s = FSMStatesFactory<T>.CreateState<V>();

            SetGlobalState(s);
        }


        public void SetGlobalState(Type stateType)
        {
            if (!(stateType is State<T>))
            {
                //Log error!
                return;
            }

            State<T> state = FSMStatesFactory<T>.CreateState(stateType);

            SetGlobalState(state);
        }



        public void Update()
        {
            if (m_pGlobalState != null)
            {
                m_pGlobalState.Execute(m_pOwner);
            }

            if (m_pCurrentState != null)
            {
                m_pCurrentState.Execute(m_pOwner);
            }
        }

        public bool HandleMessage(in IRoutedEventArgs msg)
        {
            if (m_pGlobalState != null && m_pGlobalState.OnMessage(m_pOwner, msg))
            {
                return true;
            }

            if (m_pCurrentState != null && m_pCurrentState.OnMessage(m_pOwner, msg))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 变换状态用
        /// </summary>
        /// <param name="pNewState"></param>
        public void ChangeState(State<T> pNewState, params object[] args)
        {

            FSMStatesFactory<T>.ReturnState(m_pCurrentState);

            m_pPreviousState = m_pCurrentState;

            m_pCurrentState.Exit(m_pOwner);

            m_pCurrentState = pNewState;

            m_pCurrentState.Target = m_pOwner;

            m_pCurrentState.Enter(m_pOwner, args);

        }


        /// <summary>
        /// 变换状态用
        /// </summary>
        /// <param name="pNewState"></param>
        public void ChangeState<V>(params object[] args) where V : State<T>, new()
        {
            V pNewState = FSMStatesFactory<T>.CreateState<V>();

            ChangeState(pNewState, args);
        }

        /// <summary>
        /// 变换状态用
        /// </summary>
        /// <param name="type"></param>
        /// <param name="args"></param>
        public void ChangeState(Type type, params object[] args)
        {

            State<T> s = FSMStatesFactory<T>.CreateState(type);

            ChangeState(s, args);
        }


        /// <summary>
        /// 状态翻转用
        /// </summary>
        public void RevertToPreviousState()
        {
            ChangeState(m_pPreviousState);
        }

    }
}
