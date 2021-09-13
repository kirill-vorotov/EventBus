namespace KV.Events
{
    public interface IEventHandler
    {
    }
    
    public interface IEventHandler<TEvent> : IEventHandler
    {
        void Handle(in TEvent @event);
    }
}