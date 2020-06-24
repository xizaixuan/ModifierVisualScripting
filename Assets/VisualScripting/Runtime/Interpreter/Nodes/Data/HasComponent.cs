using System;
using Unity.Entities;
using UnityEditor.Modifier.VisualScripting.Runtime;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return a boolean value if the component is on the GameObject.")]
    public struct HasComponent : IDataNode
    {
        [PortDescription(name: "", ValueType.Entity, Description = "GameObject used to see if the component is on it.")]
        public InputDataPort Entity;

        [ComponentSearcher]
        public TypeReference Type;

        [PortDescription(ValueType.Bool, Description = "Return True if the component is on the GameObject and False if not on it.")]
        public OutputDataPort Result;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Entity);
            if (entity == Unity.Entities.Entity.Null)
                entity = ctx.CurrentEntity;

            ctx.Write(Result, ctx.EntityManager.HasComponent(entity, Type.GetComponentType()));
        }
    }
}
