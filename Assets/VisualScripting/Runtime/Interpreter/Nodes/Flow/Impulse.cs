#if VS_DOTS_PHYSICS_EXISTS
using System;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;


namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Add an impulse (add force) to the GameObject based on world space coordinates. The GameObject need a **Physics Body** component to be affected. The mass of the object will affect how strong the impulse is.")]
    public struct SimpleImpulse : IFlowNode
    {
        [PortDescription("", Description = "Trigger the impulse action.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after impulse is done.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject that will get the impulse.")]
        public InputDataPort Entity;
        [PortDescription(name: "Force", ValueType.Float3, Description = "The impulse force applied to the GameObject in world space. Ex: Setting Y to 5 will push the object up. Setting the Y to -5 will push the object down.")]
        public InputDataPort Value;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsVelocity>(entity) || !ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
                return;

            PhysicsVelocity physicsVelocity = ctx.EntityManager.GetComponentData<PhysicsVelocity>(entity);
            PhysicsMass physicsMass = ctx.EntityManager.GetComponentData<PhysicsMass>(entity);

            physicsVelocity.ApplyLinearImpulse(physicsMass, ctx.ReadFloat3(Value));

            ctx.EntityManager.SetComponentData(entity, physicsVelocity);
        }
    }

    [Serializable]
    [NodeDescription("Add an impulse (add force) to the GameObject based on world space coordinates. The GameObject need a **Physics Body** component to be affected. The mass of the object will affect how strong the impulse is.")]
    public struct Impulse : IFlowNode
    {
        // We can avoid a "SimpleImpulse" and only use this one if this is clear enough. It could be interesting to gray out the Point port when this is true...
        public bool LinearOnly;

        [PortDescription("", Description = "Trigger the impulse action.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after impulse is done.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject that will get the impulse.")]
        public InputDataPort Entity;
        [PortDescription(name: "Force", ValueType.Float3, Description = "The impulse force applied to the GameObject in world space. Ex: Setting Y to 5 will push the object up. Setting the Y to -5 will push the object down.")]
        public InputDataPort Value;
        [PortDescription(ValueType.Float3, Description = "The center of Mass of the GameObject Physic Body. Changing its position will change how the object rotate after the impulse.")]
        public InputDataPort Point;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsMass>(entity) || !ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
                return;

            PhysicsMass physicsMass = ctx.EntityManager.GetComponentData<PhysicsMass>(entity);
            PhysicsVelocity physicsVelocity = ctx.EntityManager.GetComponentData<PhysicsVelocity>(entity);

            if (LinearOnly)
            {
                physicsVelocity.ApplyLinearImpulse(physicsMass, ctx.ReadFloat3(Value));
            }
            else
            {
                Translation t = ctx.EntityManager.GetComponentData<Translation>(entity);
                Rotation r = ctx.EntityManager.GetComponentData<Rotation>(entity);

                ComponentExtensions.ApplyImpulse(ref physicsVelocity, physicsMass, t, r, ctx.ReadFloat3(Value), ctx.ReadFloat3(Point));
            }

            ctx.EntityManager.SetComponentData(entity, physicsVelocity);
        }
    }
}

#endif
