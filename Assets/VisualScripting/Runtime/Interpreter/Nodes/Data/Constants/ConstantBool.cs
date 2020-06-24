using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Output user defined boolean value that can't be changed when executed. If checked value is true, if not checked value is false.")]
    public struct ConstantBool : IConstantNode<bool>
    {
        public bool Value;

        [PortDescription(ValueType.Bool, Description = "Return True or False value.")]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
