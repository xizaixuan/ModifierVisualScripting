using System;
using Modifier.Runtime.Nodes;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription(TimeType.DeltaTime, "Return the time between the current and previous frame.")]
    [NodeDescription(TimeType.ElapsedTime, "Return the length of the current session, in seconds.")]
    [NodeDescription(TimeType.FrameCount, "Return the total number of frames that have passed.")]
    public struct Time : IDataNode, IHasExecutionType<Time.TimeType>
    {
        public enum TimeType
        {
            DeltaTime,
            ElapsedTime,
            FrameCount,
        }
        [PortDescription(ValueType.Float, Description = "Delta", ExecutionType = TimeType.DeltaTime)]
        [PortDescription(ValueType.Float, Description = "Elapsed", ExecutionType = TimeType.ElapsedTime)]
        [PortDescription(ValueType.Float, Description = "FrameCount", ExecutionType = TimeType.FrameCount)]
        public OutputDataPort Value;
        [SerializeField]
        TimeType m_Type;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            switch (m_Type)
            {
                case TimeType.DeltaTime:
                    ctx.Write(Value, ctx.Time.DeltaTime);
                    break;
                case TimeType.ElapsedTime:
                    ctx.Write(Value, (float)ctx.Time.ElapsedTime);
                    break;
                case TimeType.FrameCount:
                    // TODO why ain't that available in DOTS ?
                    ctx.Write(Value, UnityEngine.Time.frameCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public TimeType Type
        {
            get => m_Type;
            set => m_Type = value;
        }
    }
}
