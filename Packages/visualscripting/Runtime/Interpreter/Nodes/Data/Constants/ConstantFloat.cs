using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantFloat : IConstantNode<float>
    {
        public float Value;

        [PortDescription(ValueType.Float)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
