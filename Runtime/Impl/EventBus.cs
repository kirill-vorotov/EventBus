using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KV.Events
{
    public class EventBus : IEventBus
    {
        internal const int EventBusQueueInitialCapacity = 64;
        
        private static readonly Dictionary<Type, int> eventIds = GetEventIds();
        
        private readonly IEventDispatcher[] _dispatchers = CreateDispatchers();
        private readonly List<IEventHandler> _handlersToAdd = new List<IEventHandler>();
        private readonly List<IEventHandler> _handlersToRemove = new List<IEventHandler>();
        private Queue<int> _writeQueue = new Queue<int>(EventBusQueueInitialCapacity);
        private Queue<int> _readQueue = new Queue<int>(EventBusQueueInitialCapacity);
        
        public IEventBus RegisterHandler(IEventHandler eventHandler)
        {
            _handlersToAdd.Add(eventHandler);
            return this;
        }

        public IEventBus UnregisterHandler(IEventHandler eventHandler)
        {
            _handlersToRemove.Add(eventHandler);
            return this;
        }

        public void Enqueue<TEvent>(TEvent @event)
        {
            var eventId = GetEventId<TEvent>();
            if (eventId < 0)
            {
                return;
            }

            var dispatcher = GetDispatcher<TEvent>(eventId);
            if (dispatcher == null)
            {
                return;
            }

            _writeQueue.Enqueue(eventId);
            dispatcher.Enqueue(@event);
        }

        public void HandleEvents()
        {
            foreach (var handler in _handlersToAdd)
            {
                AddHandler(handler);
            }
            _handlersToAdd.Clear();
            
            foreach (var handler in _handlersToRemove)
            {
                RemoveHandler(handler);
            }
            _handlersToRemove.Clear();
            
            (_readQueue, _writeQueue) = (_writeQueue, _readQueue);
            _writeQueue.Clear();
            while (_readQueue.Count > 0)
            {
                var eventId = _readQueue.Dequeue();
                var dispatcher = GetDispatcher(eventId);
                dispatcher.DispatchNext();
            }
        }

        public static int GetEventId(Type eventType)
        {
            if (eventIds.TryGetValue(eventType, out var eventId))
            {
                return eventId;
            }

            return -1;
        }

        public static int GetEventId<TEvent>()
        {
            return GetEventId(typeof(TEvent));
        }
        
        private IEventDispatcher GetDispatcher(int eventId)
        {
            Debug.Assert(eventId >= 0 && eventId < _dispatchers.Length);
            return _dispatchers[eventId];
        }

        private IEventDispatcher<TEvent> GetDispatcher<TEvent>(int eventId)
        {
            Debug.Assert(eventId >= 0 && eventId < _dispatchers.Length);
            return _dispatchers[eventId] as IEventDispatcher<TEvent>;
        }

        private void AddHandler(IEventHandler eventHandler)
        {
            var eventTypes = GetHandlerEventTypes(eventHandler);
            foreach (var eventType in eventTypes)
            {
                var eventId = GetEventId(eventType);
                if (eventId < 0 || eventId >= _dispatchers.Length)
                {
                    continue;
                }

                var dispatcher = GetDispatcher(eventId);
                dispatcher.AddEventHandler(eventHandler);
            }
        }
        
        private void RemoveHandler(IEventHandler eventHandler)
        {
            var eventTypes = GetHandlerEventTypes(eventHandler);
            foreach (var eventType in eventTypes)
            {
                var eventId = GetEventId(eventType);
                if (eventId < 0 || eventId >= _dispatchers.Length)
                {
                    continue;
                }

                var dispatcher = GetDispatcher(eventId);
                dispatcher.RemoveEventHandler(eventHandler);
            }
        }
        
        public static IEnumerable<Type> GetAllEventTypes()
        {
            return AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => Attribute.GetCustomAttribute(t, typeof(EventAttribute)) != null);
        }
        
        public static IEnumerable<Type> GetHandlerEventTypes(IEventHandler handler)
        {
            return handler.GetType()
                .GetInterfaces()
                .Where(t => t.IsGenericType)
                .Where(t => t.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                .Select(t => t.GenericTypeArguments.Single())
                .Where(t => Attribute.GetCustomAttribute(t, typeof(EventAttribute)) != null);
        }
        
        public static Dictionary<Type, int> GetEventIds()
        {
            var count = 0;
            return GetAllEventTypes().ToDictionary(t => t, _ => count++);
        }
        
        internal static IEventDispatcher[] CreateDispatchers()
        {
            var eventIds = GetEventIds();
            return GetAllEventTypes()
                .OrderBy(type => eventIds[type])
                .Select(type =>
                {
                    var dispatcherType = typeof(EventDispatcher<>);
                    var genericDispatcherType = dispatcherType.MakeGenericType(type);
                    return Activator.CreateInstance(genericDispatcherType);
                })
                .Cast<IEventDispatcher>()
                .ToArray();
        }
    }
}