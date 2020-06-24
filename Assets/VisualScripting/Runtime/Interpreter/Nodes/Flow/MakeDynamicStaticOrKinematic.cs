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
    [NodeDescription("Change the Motion Type to Static on the GameObject **Physics Body** component.\n" +
        "\n" +
        "**Static:** The object is fixed in place. ")]
    public struct MakeStatic : IFlowNode
    {
        [PortDescription("", Description = "Trigger the modification of the current Motion Type to **Static** in the **Physics Body** component of the inputted GameObject.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after changing the Motion Type to **Static**.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject that will change the Motion Type to **Static**. Need a **Physics Body** component.")]
        public InputDataPort Entity;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            if (ctx.EntityManager.HasComponent<PhysicsGravityFactor>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsGravityFactor>(entity);

            if (ctx.EntityManager.HasComponent<PhysicsMass>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsMass>(entity);

            if (ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsVelocity>(entity);

            if (ctx.EntityManager.HasComponent<PhysicsDamping>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsDamping>(entity);

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Change the Motion Type to Kinematic on the GameObject **Physics Body** component.\n" +
        "\n" +
        "**Kinematic:** The object is moved directly and not physically simulated. the object will not be driven by the physics engine, and can only be manipulated by its Transform. This is useful for moving platforms or if you want to animate a **Rigidbody** or **Physics Body** that has a HingeJoint attached.")]
    public struct MakeKinematic : IFlowNode
    {
        [PortDescription("", Description = "Trigger the modification of the current Motion Type to **Kinematic** in the **Physics Body** component of the inputted GameObject.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after changing the Motion Type to **Kinematic**.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject that will change the Motion Type to **Kinematic**. Need a **Physics Body** component.")]
        public InputDataPort Entity;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            // No mass (infinite)
            if (ctx.EntityManager.HasComponent<PhysicsMass>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsMass>(entity);

            // No damping (animated)
            if (ctx.EntityManager.HasComponent<PhysicsDamping>(entity))
                ctx.EntityManager.RemoveComponent<PhysicsDamping>(entity);

            // No gravity (actually need a component to "remove" the gravity)
            if (!ctx.EntityManager.HasComponent<PhysicsGravityFactor>(entity))
            {
                ctx.EntityManager.AddComponent<PhysicsGravityFactor>(entity);
            }
            ctx.EntityManager.SetComponentData(entity, new PhysicsGravityFactor() {Value = 0});

            // Let's set the Velocity to zero only if it hasn't been set yet (MBriau: not sure if it should always be set to zero or maybe add an option)
            if (!ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
            {
                ctx.EntityManager.AddComponentData(entity, new PhysicsVelocity(){ Linear = float3.zero, Angular = float3.zero});
            }

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Change the Motion Type to Dynamic on the GameObject **Physics Body** component.\n" +
        "\n" +
        "**Dynamic:** The object is fully physically simulated.")]
    public struct MakeDynamic : IFlowNode
    {
        [PortDescription("", Description = "Trigger the modification of the current Motion Type to **Dynamic** in the **Physics Body** component of the inputted GameObject.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after changing Motion Type to **Dynamic** and setting its parameters.")]
        public OutputTriggerPort Output;
        [PortDescription("GameObject", ValueType.Entity, Description = "The GameObject that will change the Motion Type to **Dynamic**. Need a **Physics Body** component.")]
        public InputDataPort Entity;
        [PortDescription(ValueType.Float, DefaultValue = 1f, Description = "(The Mass of th GameObject in kilograms.)")]
        public InputDataPort Mass;
        [PortDescription(ValueType.Float, Description = "How much air resistance affects the object when moving from forces. 0 means no air resistance, and infinity makes the object stop moving immediately.")]
        public InputDataPort Drag;
        [PortDescription(ValueType.Float, DefaultValue = 0.05f, Description = "How much air resistance affects the object when rotating from torque. 0 means no air resistance. Note that you cannot make the object stop rotating just by setting its Angular Drag to infinity.")]
        public InputDataPort AngularDrag;
        [PortDescription(ValueType.Float, DefaultValue = 1f, Description = "")]
        public InputDataPort GravityFactor;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            PhysicsCollider physicsCollider = ctx.EntityManager.GetComponentData<PhysicsCollider>(entity);
            Collider* colliderPtr = (Collider*)physicsCollider.Value.GetUnsafePtr();

            if (!ctx.EntityManager.HasComponent<PhysicsGravityFactor>(entity))
            {
                ctx.EntityManager.AddComponent<PhysicsGravityFactor>(entity);
            }
            float gravityFactor = ctx.ReadFloat(GravityFactor);
            ctx.EntityManager.SetComponentData(entity, new PhysicsGravityFactor(){Value = gravityFactor});

            if (!ctx.EntityManager.HasComponent<PhysicsMass>(entity))
            {
                ctx.EntityManager.AddComponent<PhysicsMass>(entity);
            }
            float mass = ctx.ReadFloat(Mass);
            ctx.EntityManager.SetComponentData(entity, PhysicsMass.CreateDynamic(colliderPtr->MassProperties, mass));

            if (!ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
            {
                ctx.EntityManager.AddComponent<PhysicsVelocity>(entity);
            }
            ctx.EntityManager.SetComponentData(entity, new PhysicsVelocity {Linear = float3.zero, Angular = float3.zero});

            float drag = ctx.ReadFloat(Drag);
            float angularDrag = ctx.ReadFloat(AngularDrag);
            if (!ctx.EntityManager.HasComponent<PhysicsDamping>(entity))
            {
                ctx.EntityManager.AddComponent<PhysicsDamping>(entity);
            }
            ctx.EntityManager.SetComponentData(entity, new PhysicsDamping {Linear = drag, Angular = angularDrag});

            ctx.Trigger(Output);
        }
    }
}
#endif
