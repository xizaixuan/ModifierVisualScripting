using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Disable the GameObject.\n" +
        " **Note:** Children of the GameObject won't be disabled.")]
    public struct Disable : IFlowNode
    {
        [PortDescription("", Description = "Trigger the GameObject disable state.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute next action after the GameObject is disabled.")]
        public OutputTriggerPort Output;
        [PortDescription("", ValueType.Entity, Description = "GameObject to disable.")]
        public InputDataPort Entity;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);
            if (entity != Unity.Entities.Entity.Null)
                ctx.EntityManager.SetEnabled(entity, false);
            ctx.Trigger(Output);
        }
    }
}
