using System;

namespace KV.Events
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
    public class EventAttribute : Attribute
    {
    }
}