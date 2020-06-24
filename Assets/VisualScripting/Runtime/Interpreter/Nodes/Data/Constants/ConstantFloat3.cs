using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantFloat3 : IConstantNode<float3>
    {
        public float3 Value;

        [PortDescription(ValueType.Float3)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
