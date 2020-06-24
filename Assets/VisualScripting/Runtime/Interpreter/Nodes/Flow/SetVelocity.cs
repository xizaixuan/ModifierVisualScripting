#if VS_DOTS_PHYSICS_EXISTS
using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;


namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Change the linear velocity on the GameObject and/or the angular velocity on the **Physics Body** component. The Linear Velocity will push the object in one direction in world space with the force of the value and decrease over time.\n" +
        "\n" +
        "Ex: a value of (x= 0, y= 10, z= 0) will push the object in the air like if it was jumping with physic no matter the rotation of the object.\n" +
        " - Angular velocity will exercise a rotation force represented in degrees per time on the object in local space this velocity will decrease at every frame until it touch 0.\n" +
        "**Note**: Velocity is the speed at which something moves in one direction")]
    public struct SetVelocities : IFlowNode
    {
        [PortDescription("", Description = "Trigger the Velocity modification of the GameObject **Physics Body** component.")]
        public InputTriggerPort Input;
        [PortDescription("", Description = "Execute the next action after setting the Linear Velocity and Angular Velocity to the GameObject **Physics Body** component.")]
        public OutputTriggerPort Output;
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject that will be affected.")]
        public InputDataPort Entity;
        [PortDescription(ValueType.Float3, Description = "Value to push the object in one direction in world space with the force of the value and decrease over time")]
        public InputDataPort Linear;
        [PortDescription(ValueType.Float3, Description = "Value for the rotation force represented in degrees per time on the object in local space this velocity will decrease at every frame until it touch 0")]
        public InputDataPort Angular;

        public unsafe void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);

            if (entity == Unity.Entities.Entity.Null)
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsCollider>(entity))
                return;

            if (!ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
                return;

            PhysicsCollider physicsCollider = ctx.EntityManager.GetComponentData<PhysicsCollider>(entity);

            Collider* colliderPtr = (Collider*)physicsCollider.Value.GetUnsafePtr();

            float3 linearVelocity = ctx.ReadFloat3(Linear);
            float3 angularVelocity = ctx.ReadFloat3(Angular);

            // TODO: MBRIAU: Make sure to understand exactly what's going on here
            // Calculate the angular velocity in local space from rotation and world angular velocity
            float3 angularVelocityLocal = math.mul(math.inverse(colliderPtr->MassProperties.MassDistribution.Transform.rot), angularVelocity);

            ctx.EntityManager.SetComponentData(entity, new PhysicsVelocity()
            {
                Linear = linearVelocity,
                Angular = angularVelocityLocal
            });

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Get the linear velocity on the GameObject and/or the angular velocity on the **Physics Body** component.")]
    public struct GetVelocities : IDataNode
    {
        [PortDescription("Instance", ValueType.Entity, Description = "The GameObject from which the values will be taken.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float3, Description = "Return the current frame linear velocity value.")]
        public OutputDataPort Linear;
        [PortDescription(ValueType.Float3, Description = "Return the current frame angular velocity value.")]
        public OutputDataPort Angular;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                if (ctx.EntityManager.HasComponent<PhysicsVelocity>(entity))
                {
                    PhysicsVelocity physicsVelocity = ctx.EntityManager.GetComponentData<PhysicsVelocity>(entity);

                    ctx.Write(Linear, physicsVelocity.Linear);
                    ctx.Write(Angular, physicsVelocity.Angular);
                }
                else
                {
                    ctx.Write(Linear, float3.zero);
                    ctx.Write(Angular, float3.zero);
                }
            }
        }
    }
}

#endif
