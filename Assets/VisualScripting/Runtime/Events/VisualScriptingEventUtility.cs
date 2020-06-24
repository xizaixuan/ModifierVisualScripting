using System.Collections.Generic;
using Unity.Collections;

namespace Modifier.Runtime
{
    static class VisualScriptingEventUtility
    {
        public static unsafe IEnumerable<EventNodeData> GetEventsFromApi(
            VisualScriptingEventSystem eventSystem,
            IReadOnlyDictionary<ulong, List<FieldDescription>> fieldDescriptions)
        {
            using (var events = new NativeList<VisualScriptingEventData>(Allocator.TempJob))
            {
                new CollectDispatchedEventsJob { Events = events }.Schedule(eventSystem).Complete();
                var data = new List<EventNodeData>();

                foreach (var evt in events)
                {
                    var values = new List<Value>();
                    if (fieldDescriptions.TryGetValue(evt.EventTypeHash, out var descriptions))
                    {
                        foreach (var description in descriptions)
                        {
                            var fieldPtr = (byte*)evt.EventPtr.ToPointer() + description.Offset;
                            if (description.FieldValueType == ValueType.StringReference)
                            {
                                var stringRefIndex = EventDataBridge.NativeStrings128.Count;
                                EventDataBridge.NativeStrings128.Add(*(NativeString128*)fieldPtr);
                                values.Add(new StringReference(stringRefIndex, StringReference.Storage.Unmanaged128));
                                continue;
                            }
                            values.Add(Value.FromPtr(fieldPtr, description.FieldValueType));
                        }
                    }
                    data.Add(new EventNodeData(evt.EventTypeHash, values));
                }

                return data;
            }
        }

        struct CollectDispatchedEventsJob : IVisualScriptingEventPtrReceiverJob
        {
            public NativeList<VisualScriptingEventData> Events;

            public void Execute(VisualScriptingEventData eventPtr)
            {
                // Don't add nodes that are dispatched by graphs as they're already stored
                // in GraphInstance.DispatchedEvents
                if (!eventPtr.IsFromGraph)
                    Events.Add(eventPtr);
            }
        }
    }
}