#if VS_DOTS_PHYSICS_EXISTS
using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;


namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Change the mass on the GameObject **Physics Body** component.")]
    public struct SetMass : IFlowNode
    {
        [PortDescription("", Description = "Trigger the mass change.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after changing the mass.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject containing the **Physics Body** component we want to affect.")]
        public InputDataPort Entity;
        [PortDescription(ValueType.Float, Description = "The new mass value.")]
        public InputDataPort Mass;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsMass>(entity) || !ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            PhysicsCollider physicsCollider = ctx.EntityManager.GetComponentData<PhysicsCollider>(entity);

            float newMass = ctx.ReadFloat(Mass);

            Collider* colliderPtr = (Collider*)physicsCollider.Value.GetUnsafePtr();

            ctx.EntityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, newMass));
        }
    }

    [Serializable]
    [NodeDescription("Return the mass of the GameObject **Physics Body** component.")]
    public struct GetMass : IDataNode
    {
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject containing the **Physics Body** component that will return its mass.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float, Description = "Return the mass value.")]
        public OutputDataPort Value;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                if (ctx.EntityManager.HasComponent<PhysicsMass>(entity))
                {
                    PhysicsMass physicsMass = ctx.EntityManager.GetComponentData<PhysicsMass>(entity);

                    ctx.Write(Value, 1.0f / physicsMass.InverseMass);
                }
                else
                {
                    ctx.Write(Value, 0);
                }
            }
        }
    }
}

#endif
