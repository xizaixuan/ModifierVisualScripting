using System;
using Modifier.Runtime.Nodes;
using Unity.Entities;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct SetComponent : IFlowNode, IHasExecutionType<TypeReference>
    {
        [PortDescription("")]
        public InputTriggerPort Set;
        [PortDescription("")]
        public OutputTriggerPort OnSet;

        [PortDescription("Game Object", ValueType.Entity)]
        public InputDataPort GameObject;

        [SerializeField]
        TypeReference m_ComponentType;

        public TypeReference Type
        {
            get => m_ComponentType;
            set => m_ComponentType = value;
        }

        public InputDataMultiPort ComponentData;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity == Entity.Null)
                entity = ctx.CurrentEntity;

            if (ctx.EntityManager.HasComponent(entity, Type.GetComponentType()))
            {
                for (int i = 0; i < ComponentData.DataCount; ++i)
                {
                    if (ctx.HasConnectedValue(ComponentData.SelectPort((uint)i)))
                        ctx.SetComponentValue(entity, Type, i, ctx.ReadValue(ComponentData.SelectPort((uint)i)));
                }
            }
            ctx.Trigger(OnSet);
        }
    }
}
