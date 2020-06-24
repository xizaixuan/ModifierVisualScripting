using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Set the rotation of the GameObject. The rotation is set in Euler like in the inspector")]
    public struct RotateBy : IFlowNode
    {
        [PortDescription(Description = "Trigger the rotation of the GameObject.")]
        public InputTriggerPort Input;
        [PortDescription(Description = "Execute next action after the GameObject rotation is set.")]
        public OutputTriggerPort Output;
        [PortDescription(ValueType.Entity, Description = "The GameObject that will rotate.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float3, Description = "The rotation value in euler like in the inspector(x,y,z)")]
        public InputDataPort Value;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                quaternion currentRotation;
                // Make sure that the entity has a Rotation
                if (!ctx.EntityManager.HasComponent<Rotation>(entity))
                {
                    ctx.EntityManager.AddComponent<Rotation>(entity);
                    currentRotation = quaternion.identity;
                }
                else
                {
                    currentRotation = ctx.EntityManager.GetComponentData<Rotation>(entity).Value;
                }

                currentRotation *= Quaternion.Euler(ctx.ReadFloat3(Value));
                ctx.EntityManager.SetComponentData(entity, new Rotation {Value = currentRotation});
            }

            ctx.Trigger(Output);
        }
    }
}
