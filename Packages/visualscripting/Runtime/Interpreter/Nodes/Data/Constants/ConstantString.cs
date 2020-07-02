using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantString : IConstantNode<StringReference>
    {
        public StringReference Value;

        [PortDescription(ValueType.StringReference)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}