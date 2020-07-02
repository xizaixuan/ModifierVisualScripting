using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantFloat4 : IConstantNode<float4>
    {
        public float4 Value;

        [PortDescription(ValueType.Float4)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
