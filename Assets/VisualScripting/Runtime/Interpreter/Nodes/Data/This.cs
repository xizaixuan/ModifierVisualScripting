using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Retrieve the GameObject corresponding to this script.")]
    public struct This : IDataNode
    {
        [PortDescription("", ValueType.Entity, Description = "GameObject for this script")]
        public OutputDataPort ThisPort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ThisPort, ctx.CurrentEntity);
        }
    }
}
