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
    [NodeDescription("Enable the GameObject component **Physics Shape** or any **Collider**.\n" +
        "\n" +
        "**Note**: The collider need to be enabled in the editor and to be disabled when the game was running to be enabled.")]
    public struct EnableCollisions : IFlowNode
    {
        [PortDescription("", Description = "Trigger the GameObject component **Physics Shape** or **Collider** enable state.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute next action after the GameObject component **Physics Shape** or **Collider** is enabled.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject with the component **Physics Shape** or **Collider** that will be enabled.")]
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

            CollisionFilter currentFilter = colliderPtr->Filter;

            if (currentFilter.BelongsTo != CollisionFilter.Zero.BelongsTo ||
                currentFilter.CollidesWith != CollisionFilter.Zero.CollidesWith ||
                currentFilter.GroupIndex != CollisionFilter.Zero.GroupIndex)
            {
                // If at least something is set, we consider that it's enabled in some way even if it would be possible to never collide with that filter
                // Could be a good thing so support GroupIndex being set at this point...
            }
            else
            {
                if (ctx.EntityManager.HasComponent<DisableCollisions.CollisionFilterBeforeBeingDisabled>(entity))
                {
                    colliderPtr->Filter = ctx.EntityManager.GetComponentData<DisableCollisions.CollisionFilterBeforeBeingDisabled>(entity).Value;
                    ctx.EntityManager.RemoveComponent<DisableCollisions.CollisionFilterBeforeBeingDisabled>(entity);
                }
                else
                {
                    colliderPtr->Filter = CollisionFilter.Default;
                }
            }

            ctx.Trigger(Output);
        }
    }

    // MBRIAU: Not exposed yet
    [Serializable]
    public struct SetCollisionFilter : IFlowNode
    {
        [PortDescription("")]
        public InputTriggerPort Input;
        [PortDescription("")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity)]
        public InputDataPort Entity;
        // MBRIAU: Would need to use UInt
        [PortDescription(ValueType.Int)]
        public InputDataPort BelongsTo;
        // MBRIAU: Would need to use UInt
        [PortDescription(ValueType.Int)]
        public InputDataPort CollidesWith;
        [PortDescription(ValueType.Int)]
        public InputDataPort GroupIndex;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            PhysicsCollider physicsCollider = ctx.EntityManager.GetComponentData<PhysicsCollider>(entity);
            Collider* colliderPtr = (Collider*)physicsCollider.Value.GetUnsafePtr();

            CollisionFilter newFilter = new CollisionFilter();

            newFilter.BelongsTo = (uint)ctx.ReadInt(this.BelongsTo);
            newFilter.CollidesWith = (uint)ctx.ReadInt(this.CollidesWith);
            newFilter.GroupIndex = ctx.ReadInt(this.GroupIndex);

            colliderPtr->Filter = newFilter;

            ctx.Trigger(Output);
        }
    }
}
#endif
