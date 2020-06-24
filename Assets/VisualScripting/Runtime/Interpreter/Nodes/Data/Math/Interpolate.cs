using System;
using Modifier.Runtime.Nodes;
using Unity.Mathematics;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct Interpolate : IDataNode, IHasExecutionType<InterpolationType>
    {
        [PortDescription(ValueType.Float)]
        public InputDataPort From;

        [PortDescription(ValueType.Float)]
        public InputDataPort To;

        [PortDescription(ValueType.Float)]
        public InputDataPort Progress;

        [PortDescription(ValueType.Float, "", DefaultValue = 0f)]
        public OutputDataPort Result;

        [SerializeField]
        InterpolationType m_Type;

        public InterpolationType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var result = Type == InterpolationType.Linear
                ? math.lerp(ctx.ReadFloat(From), ctx.ReadFloat(To), math.clamp(ctx.ReadFloat(Progress), 0, 1))
                : Mathf.SmoothStep(ctx.ReadFloat(From), ctx.ReadFloat(To), ctx.ReadFloat(Progress));
            ctx.Write(Result, result);
        }
    }
}
