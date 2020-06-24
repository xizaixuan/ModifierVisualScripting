using System;
using JetBrains.Annotations;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Enable the GameObject. Note: Children of the GameObject won't be enabled.")]
    public struct Enable : IFlowNode
    {
        [UsedImplicitly]
        [PortDescription("", Description = "Trigger the GameObject enable state.")]
        public InputTriggerPort Input;

        [PortDescription("", ValueType.Entity, Description = "GameObject to enable.")]
        public InputDataPort Entity;

        [PortDescription("", Description = "Execute next action after the GameObject is enabled.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);
            if (entity != Unity.Entities.Entity.Null)
                ctx.EntityManager.SetEnabled(entity, true);
            ctx.Trigger(Output);
        }
    }
}
