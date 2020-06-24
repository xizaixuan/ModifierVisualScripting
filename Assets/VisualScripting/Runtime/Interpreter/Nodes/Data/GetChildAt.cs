using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return the child GameObject from the GameObject parent input at the given index position. If the child index doesn't exist, Null will be returned.\n" +
        "\n" +
        "**Tip:** Index 0 represent the first child. Warning: The children order may change when going in play mode.")]
    public struct GetChildAt : IDataNode
    {
        [PortDescription(ValueType.Entity, "", Description = "Parent GameObject used to get the child.")]
        public InputDataPort GameObject;

        [PortDescription(ValueType.Int, Description = "The integer number representing the position of the GameObject you want to get from the parent GameObject. 0 is the first child.")]
        public InputDataPort Index;

        [PortDescription(ValueType.Entity, "", Description = "Return the child found at the given index number. Return Null if nothing is found at the given index.")]
        public OutputDataPort Child;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(GameObject);
            if (entity == Entity.Null)
                entity = ctx.CurrentEntity;

            var result = Entity.Null;
            var index = ctx.ReadInt(Index);

            if (ctx.EntityManager.HasComponent<Child>(entity))
            {
                var children = ctx.EntityManager.GetBuffer<Child>(entity);
                if (index < children.Length)
                {
                    var child = children[index];
                    result = child.Value;
                }
            }

            ctx.Write(Child, result);
        }
    }
}
