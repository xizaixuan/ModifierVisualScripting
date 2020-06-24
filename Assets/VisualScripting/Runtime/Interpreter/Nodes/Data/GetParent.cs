using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return the GameObject that is the parent of the inputted GameObject.")]
    public struct GetParent : IDataNode
    {
        [PortDescription(ValueType.Entity, "", Description = "The GameObject we want to use to find it's parent GameObject.")]
        public InputDataPort GameObject;

        [PortDescription(ValueType.Entity, "", Description = "Return the GameObject that is the parent of the selected GameObject. Return Null if none.")]
        public OutputDataPort Parent;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var go = ctx.ReadEntity(GameObject);
            var result = Entity.Null;

            if (go != Entity.Null && ctx.EntityManager.HasComponent<Parent>(go))
            {
                var parent = ctx.EntityManager.GetComponentData<Parent>(go);
                result = parent.Value;
            }

            ctx.Write(Parent, result);
        }
    }
}
