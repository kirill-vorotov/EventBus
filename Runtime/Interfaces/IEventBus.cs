namespace KV.Events
{
    public interface IEventBus
    {
        IEventBus RegisterHandler(IEventHandler eventHandler);
        IEventBus UnregisterHandler(IEventHandler eventHandler);
        void Enqueue<TEvent>(TEvent @event);
        void HandleEvents();
    }
}