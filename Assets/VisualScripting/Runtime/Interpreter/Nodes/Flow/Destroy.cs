using System;
using System.ComponentModel;
using JetBrains.Annotations;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Destroy the GameObject that is linked to the Instance input port.")]
    public struct Destroy : IFlowNode
    {
        [UsedImplicitly]
        [PortDescription("", Description = "Trigger the GameObject destruction.")]
        public InputTriggerPort Input;

        [PortDescription("", ValueType.Entity, Description = "GameObject to destroy.")]
        public InputDataPort Entity;

        [PortDescription("", Description = "Execute next action after destruction.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);
            if (entity != Unity.Entities.Entity.Null)
                ctx.EntityManager.DestroyEntity(entity);
            ctx.Trigger(Output);
        }
    }
}
