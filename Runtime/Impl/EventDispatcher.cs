using System.Collections.Generic;
using System.Diagnostics;

namespace KV.Events
{
    public class EventDispatcher<TEvent> : IEventDispatcher<TEvent>
    {
        internal const int InitialQueueCapacity = 4;
        
        private readonly List<IEventHandler<TEvent>> _handlers = new List<IEventHandler<TEvent>>();
        private readonly Queue<TEvent> _queue = new Queue<TEvent>(InitialQueueCapacity);
        
        public void Enqueue(TEvent @event)
        {
            _queue.Enqueue(@event);
        }

        public void AddEventHandler(IEventHandler eventHandler)
        {
            Debug.Assert(eventHandler != null);
            if (eventHandler is IEventHandler<TEvent> handler)
            {
                _handlers.Add(handler);
            }
        }

        public bool RemoveEventHandler(IEventHandler eventHandler)
        {
            Debug.Assert(eventHandler != null);
            if (eventHandler is IEventHandler<TEvent> handler)
            {
                return _handlers.Remove(handler);
            }

            return false;
        }

        public void DispatchNext()
        {
            if (_queue.Count <= 0)
            {
                return;
            }

            var @event = _queue.Dequeue();
            foreach (var handler in _handlers)
            {
                handler.Handle(@event);
            }
        }

        public void Clear()
        {
            _queue.Clear();
        }

        public void TrimExcess()
        {
            _queue.TrimExcess();
        }
    }
}