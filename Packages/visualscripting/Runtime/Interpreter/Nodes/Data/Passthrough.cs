using System;

namespace Modifier.Runtime
{
    [Serializable]
    public struct Passthrough : IDataNode
    {
        public InputDataPort Input;
        public OutputDataPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(Output, ctx.ReadValue(Input));
        }
    }
}
