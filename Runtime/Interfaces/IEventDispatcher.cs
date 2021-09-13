namespace KV.Events
{
    public interface IEventDispatcher
    {
        void AddEventHandler(IEventHandler eventHandler);
        bool RemoveEventHandler(IEventHandler eventHandler);
        void DispatchNext();
        void Clear();
        void TrimExcess();
    }
    
    public interface IEventDispatcher<TEvent> : IEventDispatcher
    {
        void Enqueue(TEvent @event);
    }
}