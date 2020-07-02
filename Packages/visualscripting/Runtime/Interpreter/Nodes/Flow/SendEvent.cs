using System;
using System.Collections.Generic;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct SendEvent : IEventDispatcherNode
    {
        public InputTriggerPort Input;

        [PortDescription(ValueType.Entity)]
        public InputDataPort Target;

        public InputDataMultiPort Values;
        public OutputTriggerPort Output;

        [SerializeField]
        int m_EventTypeSize;

        [SerializeField]
        ulong m_EventId;

        public ulong EventId
        {
            get => m_EventId;
            set => m_EventId = value;
        }

        public int EventTypeSize
        {
            get => m_EventTypeSize;
            set => m_EventTypeSize = value;
        }

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var values = new List<Value>();
            for (uint i = 0; i < Values.DataCount; ++i)
            {
                var val = ctx.ReadValue(Values.SelectPort(i));
                if (val.Type == ValueType.StringReference)
                {
                    var s = ctx.GetString128(val.StringReference);
                    var index = EventDataBridge.NativeStrings128.Count;
                    EventDataBridge.NativeStrings128.Add(s);
                    val = new StringReference(index, StringReference.Storage.Unmanaged128);
                }
                values.Add(val);
            }

            if (ctx.HasConnectedValue(Target))
            {
                var target = ctx.ReadEntity(Target);
                if (target != Unity.Entities.Entity.Null)
                    ctx.DispatchEvent(new EventNodeData(EventId, values, target, ctx.CurrentEntity), EventTypeSize);
            }
            else
            {
                ctx.DispatchEvent(new EventNodeData(EventId, values, source: ctx.CurrentEntity), EventTypeSize);
            }

            ctx.Trigger(Output);
        }
    }
}
