#if UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#endif

namespace KV.Events
{
    public static class EventBusSystem
    {
#if UNITY_2019_1_OR_NEWER
        private static readonly Type updateType = typeof(PreLateUpdate);
        private static readonly HashSet<IEventBus> eventBuses = new HashSet<IEventBus>();
        
        public static void Register(IEventBus eventBus)
        {
            eventBuses.Add(eventBus);
        }
        
        public static bool Unregister(IEventBus eventBus)
        {
            return eventBuses.Remove(eventBus);
        }

        private static void Update()
        {
            foreach (var eventBus in eventBuses)
            {
                eventBus.HandleEvents();
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            var root = PlayerLoop.GetCurrentPlayerLoop();

            if (root.subSystemList == null)
            {
                root.subSystemList = new[]
                {
                    new PlayerLoopSystem()
                    {
                        updateDelegate = Update,
                        type = typeof(EventBusUpdate),
                    }
                };
                PlayerLoop.SetPlayerLoop(root);
                return;
            }
            
            var systems = root.subSystemList.ToList();
            var foundPreLateUpdate = false;
            for (var systemIndex = 0; systemIndex < systems.Count; ++systemIndex)
            {
                if (systems[systemIndex].type != updateType)
                {
                    continue;
                }

                systems.Insert(2, new PlayerLoopSystem()
                {
                    updateDelegate = Update,
                    type = typeof(EventBusUpdate),
                });
                foundPreLateUpdate = true;
                break;
            }

            if (!foundPreLateUpdate)
            {
                systems.Insert(0, new PlayerLoopSystem()
                {
                    updateDelegate = Update,
                    type = typeof(EventBusUpdate),
                });
            }
            root.subSystemList = systems.ToArray();
            PlayerLoop.SetPlayerLoop(root);
        }
#endif
    }
}