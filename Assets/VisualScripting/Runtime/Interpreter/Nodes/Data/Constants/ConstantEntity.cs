using System;
using Unity.Entities;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ConstantEntity : IConstantNode<Entity>
    {
        public Entity Value;

        [PortDescription(ValueType.Entity)]
        public OutputDataPort ValuePort;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(ValuePort, Value);
        }
    }
}
