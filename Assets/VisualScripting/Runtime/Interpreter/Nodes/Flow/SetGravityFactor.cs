#if VS_DOTS_PHYSICS_EXISTS
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine.Internal;


namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Change the gravity scale on the GameObject **Physics Body** component.")]
    public struct SetGravityFactor : IFlowNode
    {
        [PortDescription("", Description = "Trigger the gravity scale change.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after changing the gravity scale.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject containing the **Physics Body** component we want to affect.")]
        public InputDataPort Entity;
        [PortDescription(ValueType.Float, Description = "The new gravity scale value.")]
        public InputDataPort Value;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsGravityFactor>(entity))
                ctx.EntityManager.AddComponent<PhysicsGravityFactor>(entity);

            float gravityFactor = ctx.ReadFloat(Value);

            ctx.EntityManager.SetComponentData(entity, new PhysicsGravityFactor()
            {
                Value = gravityFactor
            });

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Return the gravity scale from the GameObject **Physics Body** component.")]
    public struct GetGravityFactor : IDataNode
    {
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject containing the **Physics Body** component that will return its gravity factor.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float, Description = "Return the gravity factor value.")]
        public OutputDataPort Value;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                if (ctx.EntityManager.HasComponent<PhysicsGravityFactor>(entity))
                {
                    PhysicsGravityFactor physicsGravityFactor = ctx.EntityManager.GetComponentData<PhysicsGravityFactor>(entity);

                    ctx.Write(Value, physicsGravityFactor.Value);
                }
                else
                {
                    ctx.Write(Value, 1.0f);
                }
            }
        }
    }
}

#endif
