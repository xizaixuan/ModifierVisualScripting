using System;
using System.Linq;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct OnEvent : IEventReceiverNode
    {
        public OutputTriggerPort Output;
        public OutputDataMultiPort Values;

        [PortDescription(ValueType.Entity)]
        public OutputDataPort Source;

        [SerializeField]
        ulong m_EventId;

        public ulong EventId
        {
            get => m_EventId;
            set => m_EventId = value;
        }

        public Execution Execute<TCtx>(TCtx ctx, EventNodeData data) where TCtx : IGraphInstance
        {
            if (EventId == data.Id)
            {
                for (var i = 0; i < Values.DataCount; ++i)
                {
                    ctx.Write(Values.SelectPort((uint)i), data.Values.ElementAt(i));
                }

                ctx.Write(Source, data.Source);
                ctx.Trigger(Output);
            }

            return Execution.Done;
        }
    }
}