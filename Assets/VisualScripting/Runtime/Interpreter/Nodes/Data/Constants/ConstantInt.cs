using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantInt : IConstantNode<int>
    {
        public int Value;

        [PortDescription(ValueType.Int)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
