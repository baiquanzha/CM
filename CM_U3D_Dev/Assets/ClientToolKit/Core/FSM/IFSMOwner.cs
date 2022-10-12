namespace MTool.Core.FSM
{

    public interface IFSMOwner { }

    public interface IFSMOwner<T> where T : IFSMOwner, new()
    {

        StateMachine<T> FSM { get; }

        void InitializeFSM();

        void Update();

        bool HandleMessage(in IRoutedEventArgs msg);
    }
}
