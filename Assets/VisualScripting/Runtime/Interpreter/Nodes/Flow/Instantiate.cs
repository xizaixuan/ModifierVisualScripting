using System;
using Unity.Entities;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Instantiate the GameObject.")]
    public struct Instantiate : IFlowNode
    {
        [PortDescription("", Description = "Trigger the GameObject instantiation.")]
        public InputTriggerPort Input;

        [PortDescription(ValueType.Entity, "", Description = "The GameObject to instantiate.")]
        public InputDataPort Prefab;

        [PortDescription("", Description = "Execute next action after the GameObject is instantiated.")]
        public OutputTriggerPort Output;

        [PortDescription(ValueType.Entity, "", Description = "Return the instantiated GameObject.")]
        public OutputDataPort Instantiated;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var entity = ctx.ReadEntity(Prefab);

            if (entity != Entity.Null)
            {
                var instantiated = ctx.EntityManager.Instantiate(entity);
                ctx.Write(Instantiated, instantiated);
            }

            ctx.Trigger(Output);
        }
    }
}
