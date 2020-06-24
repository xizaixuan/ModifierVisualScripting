using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Set the position of the GameObject. The position is set in world space.")]
    public struct SetPosition : IFlowNode
    {
        [PortDescription(Description = "Trigger the positioning of the GameObject.")]
        public InputTriggerPort Input;
        [PortDescription(Description = "Execute next action after the GameObject position is set.")]
        public OutputTriggerPort Output;
        [PortDescription(ValueType.Entity, Description = "The GameObject that will change its position.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float3, Description = "The new position in world space.")]
        public InputDataPort Value;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                var t = ctx.EntityManager.GetComponentData<Translation>(entity);
                t.Value = ctx.ReadFloat3(Value);
                ctx.EntityManager.SetComponentData(entity, t);
            }

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Get the position of the GameObject. The position value is in world space.")]
    public struct GetPosition : IDataNode
    {
        [PortDescription(ValueType.Entity, Description = "The GameObject that will return its position.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float3, Description = "Return the position of the GameObject in world space.")]
        public OutputDataPort Value;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                var t = ctx.EntityManager.GetComponentData<Translation>(entity);
                ctx.Write(Value, t.Value);
            }
        }
    }
}
