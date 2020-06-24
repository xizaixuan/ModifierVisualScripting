#if VS_DOTS_PHYSICS_EXISTS
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Internal;
using Collider = Unity.Physics.Collider;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Disable the GameObject component **Physics Shape** or any **Collider**.")]
    public struct DisableCollisions : IFlowNode
    {
        public struct CollisionFilterBeforeBeingDisabled : IComponentData
        {
            public CollisionFilter Value;
        }

        [PortDescription("", Description = "Trigger the GameObject component **Physics Shape** or **Collider** disable state.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute next action after the GameObject component **Physics Shape** or **Collider** is disabled.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject with the component **Physics Shape** or **Collider** that will be disabled.")]
        public InputDataPort Entity;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            PhysicsCollider physicsCollider = ctx.EntityManager.GetComponentData<PhysicsCollider>(entity);
            Collider* colliderPtr = (Collider*)physicsCollider.Value.GetUnsafePtr();

            if (port == Input)
            {
                CollisionFilter currentFilter = colliderPtr->Filter;

                if (currentFilter.BelongsTo != CollisionFilter.Zero.BelongsTo || currentFilter.CollidesWith != CollisionFilter.Zero.CollidesWith || currentFilter.GroupIndex != CollisionFilter.Zero.GroupIndex)
                {
                    ctx.EntityManager.AddComponentData(entity, new CollisionFilterBeforeBeingDisabled {Value = currentFilter});
                    colliderPtr->Filter = CollisionFilter.Zero;
                }

                ctx.Trigger(Output);
            }
        }
    }
}
#endif
