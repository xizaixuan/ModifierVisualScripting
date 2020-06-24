using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return the total amount of child in the parent GameObject.\n" +
        "\n" +
        "Note: Only the first level of children will be in the count.")]
    public struct GetChildrenCount : IDataNode
    {
        [PortDescription(ValueType.Entity, "", Description = "The GameObject used to get the amount of children.")]
        public InputDataPort GameObject;

        [PortDescription(ValueType.Int, "", Description = "Return the total number of children directly contained in the GameObject.")]
        public OutputDataPort ChildrenCount;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity == Entity.Null)
                entity = ctx.CurrentEntity;

            var result = 0;

            if (ctx.EntityManager.HasComponent<Child>(entity))
            {
                var children = ctx.EntityManager.GetBuffer<Child>(entity);
                result = children.Length;
            }

            ctx.Write(ChildrenCount, result);
        }
    }
}
