namespace MTool.Core.FSM
{

    public interface IRoutedEventArgs
    {
        int EventType { set; get; }
    }

    public struct RoutedEventArgs : IRoutedEventArgs
    {
        public int EventType { set; get; }
    }

    public struct RoutedEventArgs<T> : IRoutedEventArgs
    {
        public int EventType { set; get; }

        public T arg { set; get; }
    }
}
