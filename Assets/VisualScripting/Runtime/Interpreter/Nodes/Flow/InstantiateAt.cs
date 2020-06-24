using System;
using JetBrains.Annotations;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Instantiate the GameObject and let the user set the position, rotation and scaling of the new instance at the same time. .")]
    public struct InstantiateAt : IFlowNode
    {
        [UsedImplicitly, PortDescription("", Description = "Trigger the GameObject instantiation.")]
        public InputTriggerPort Input;

        [PortDescription(ValueType.Entity, "", Description = "The GameObject to instantiate.")]
        public InputDataPort Prefab;

        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "Define if the instantiated GameObject will be activated. Set to True by default.")]
        public InputDataPort Activate;

        [PortDescription(ValueType.Float3, Description = "The position of the instantiated GameObject in world space.")]
        public InputDataPort Position;

        [PortDescription(ValueType.Quaternion, Description = "The rotation value of the instantiated GameObject.")]
        public InputDataPort Rotation;

        [PortDescription(ValueType.Float3, Description = "The Scale of the instantiated GameObject. Default Value (1,1,1)")]
        public InputDataPort Scale;

        [PortDescription("", Description = "Execute next action after the GameObject is instantiated.")]
        public OutputTriggerPort Output;

        [PortDescription(ValueType.Entity, "", Description = "Return the instantiated GameObject.")]
        public OutputDataPort Instantiated;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var prefab = ctx.ReadEntity(Prefab);
            if (prefab == Entity.Null)
            {
                ctx.Trigger(Output);
                return;
            }

            var entity = ctx.EntityManager.Instantiate(prefab);

            var activated = ctx.ReadBool(Activate);
            ctx.EntityManager.SetEnabled(entity, activated);

            AddComponent(ctx, entity, new Translation { Value = ctx.ReadFloat3(Position) });
            AddComponent(ctx, entity, new Rotation { Value = ctx.ReadQuaternion(Rotation) });

            var scale = ctx.ReadFloat3(Scale);
            var isUniformScale = scale.x.Equals(scale.y) && scale.x.Equals(scale.z);
            if (isUniformScale)
            {
                if (ctx.EntityManager.HasComponent<NonUniformScale>(entity))
                    ctx.EntityManager.RemoveComponent<NonUniformScale>(entity);
                AddComponent(ctx, entity, new Scale { Value = scale.x });
            }
            else
            {
                if (ctx.EntityManager.HasComponent<Scale>(entity))
                    ctx.EntityManager.RemoveComponent<Scale>(entity);
                AddComponent(ctx, entity, new NonUniformScale { Value = scale });
            }

            ctx.Write(Instantiated, entity);
            ctx.Trigger(Output);
        }

        void AddComponent<T>(IGraphInstance ctx, Entity entity, T componentData) where T : struct, IComponentData
        {
            if (!ctx.EntityManager.HasComponent<T>(entity))
                ctx.EntityManager.AddComponent<T>(entity);
            ctx.EntityManager.SetComponentData(entity, componentData);
        }
    }
}
