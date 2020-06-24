using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantFloat2 : IConstantNode<float2>
    {
        public float2 Value;

        [PortDescription(ValueType.Float2)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
