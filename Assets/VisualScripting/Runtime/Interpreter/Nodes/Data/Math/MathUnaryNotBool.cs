using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct MathUnaryNotBool : IDataNode
    {
        [PortDescription(ValueType.Bool)]
        public InputDataPort Value;
        [PortDescription(ValueType.Bool)]
        public OutputDataPort Result;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var input = ctx.ReadBool(Value);

            ctx.Write(Result, !input);
        }
    }
}
