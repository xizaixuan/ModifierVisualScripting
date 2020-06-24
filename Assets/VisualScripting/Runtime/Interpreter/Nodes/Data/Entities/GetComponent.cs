using System;
using Modifier.Runtime.Nodes;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct GetComponent : IDataNode, IHasExecutionType<TypeReference>
    {
        [PortDescription("Game Object", ValueType.Entity)]
        public InputDataPort GameObject;

        // [ComponentSearcher(ComponentOptions.AnyComponent)]
        [SerializeField]
        TypeReference m_ComponentType;

        public TypeReference Type
        {
            get => m_ComponentType;
            set => m_ComponentType = value;
        }

        public OutputDataMultiPort ComponentData;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity == Unity.Entities.Entity.Null)
                entity = ctx.CurrentEntity;

            bool hasComponent = ctx.EntityManager.HasComponent(entity, Type.GetComponentType());
            for (int i = 0; i < ComponentData.DataCount; ++i)
            {
                ctx.Write(ComponentData.SelectPort((uint)i), hasComponent
                    ? ctx.GetComponentValue(entity, Type, i)
                    : ctx.GetComponentDefaultValue(Type, i));
            }
        }
    }
}
