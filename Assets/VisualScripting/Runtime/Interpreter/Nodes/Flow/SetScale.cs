using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Set the scale of the GameObject.")]
    public struct SetScale : IFlowNode
    {
        [PortDescription(Description = "Trigger the scale change of the GameObject.")]
        public InputTriggerPort Input;
        [PortDescription(Description = "Execute next action after the GameObject scaling is set.")]
        public OutputTriggerPort Output;
        [PortDescription(ValueType.Entity, Description = "The GameObject that will change its scaling.")]
        public InputDataPort GameObject;
        [PortDescription(Description = "The new scaling value.")]
        public InputDataPort Value;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            // MBRIAU: NOTES:
            // If the scale is only modified in the editor, the info is only stored in the CompositeScale
            // If the scale is modified by Scale, it's stored in Scale and CompositeScale is updated with that value
            // If the scale is modified by NonUniformScale, it's stored in NonUniformScale and CompositeScale is updated with that value

            // WEIRD CASES
            // if both are set, the CompositeScale is not properly updated with what is rendered! We need to make sure to avoid setting both NonUniformScale and UniformScale
            // TODO: Would be interesting to try to add a Scale/NonUniformScale and then remove the component to see how the CompositeScale behaves

            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                // Important to read right now and get the right value before messing around with the scaling components of that entity
                Value v = ctx.ReadValue(Value);
                float3 newScale;
                if (v.Type == ValueType.Float)
                    newScale = new float3(v.Float, v.Float, v.Float);
                else if (v.Type == ValueType.Int)
                    newScale = new float3(v.Int, v.Int, v.Int);
                else if (v.Type == ValueType.Float3)
                    newScale = v.Float3;
                else
                {
                    // Simply return without triggering
                    // TODO: Should display a warning or only allow float and float3 to be connected
                    return;
                }

                NonUniformScale nus = new NonUniformScale {Value = newScale};

                if (ctx.EntityManager.HasComponent<Scale>(entity))
                    ctx.EntityManager.RemoveComponent<Scale>(entity);

                if (!ctx.EntityManager.HasComponent<NonUniformScale>(entity))
                    ctx.EntityManager.AddComponent<NonUniformScale>(entity);

                ctx.EntityManager.SetComponentData(entity, nus);
            }

            ctx.Trigger(Output);
        }
    }

    [Serializable]
    [NodeDescription("Get the scaling of the GameObject.")]
    public struct GetScale : IDataNode
    {
        [PortDescription(ValueType.Entity, Description = "The GameObject that will return its scaling.")]
        public InputDataPort GameObject;
        [PortDescription(ValueType.Float3, Description = "Return the scale value.")]
        public OutputDataPort Value;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity != Entity.Null)
            {
                if (ctx.EntityManager.HasComponent<NonUniformScale>(entity))
                {
                    NonUniformScale t = ctx.EntityManager.GetComponentData<NonUniformScale>(entity);
                    ctx.Write(Value, t.Value);
                }
                else if (ctx.EntityManager.HasComponent<Scale>(entity))
                {
                    Scale t = ctx.EntityManager.GetComponentData<Scale>(entity);
                    ctx.Write(Value, new float3(t.Value, t.Value, t.Value));
                }
                else if (ctx.EntityManager.HasComponent<CompositeScale>(entity))
                {
                    CompositeScale compositeScale = ctx.EntityManager.GetComponentData<CompositeScale>(entity);
                    float4x4 floatMatrix = compositeScale.Value;
                    ctx.Write(Value, new float3(floatMatrix.c0.x, floatMatrix.c1.y, floatMatrix.c2.z));
                }
                else
                {
                    ctx.Write(Value, new float3(1, 1, 1));
                }
            }
        }
    }
}
