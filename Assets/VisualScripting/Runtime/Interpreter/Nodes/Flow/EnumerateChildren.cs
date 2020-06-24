using System;
using Unity.Entities;
using Unity.Transforms;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **EnumarateChildren** node get all the direct children in the hierarchy of the GameObject connected to it. It will execute code for child one by one every time you pass by the Input port.")]
    public struct EnumerateChildren : IFlowNode<EnumerateChildren.State>
    {
        [PortDescription("", Description = "Trigger indicate to the node to provide the next child.")]
        public InputTriggerPort NextChild;
        [PortDescription(Description = "Reset the enumeration so next time the main input is triggered, the first child will be provided.")]
        public InputTriggerPort Reset;

        [PortDescription("", Description = "Execute when a new child has been provided through the main input. Each time it execute, it process a new child in the list.")]
        public OutputTriggerPort Out;
        [PortDescription(Description = "Execute when the main input is triggered and there is no more child.")]
        public OutputTriggerPort Done;

        [PortDescription(ValueType.Entity, "", Description = "The parent GameObject that will return its children.")]
        public InputDataPort GameObject;

        [PortDescription(ValueType.Entity, Description = "The current child GameObject of the parent GameObject if any left. Will be null if none left.")]
        public OutputDataPort Child;

        [PortDescription(ValueType.Int, "Child Index", Description = "The index of the current child GameObject in the parent Game Object if any left, or -1 otherwise.")]
        public OutputDataPort ChildIndex;

        public struct State : INodeState
        {
            public int Index;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var go = ctx.ReadEntity(GameObject);
            if (go == Entity.Null)
                go = ctx.CurrentEntity;

            if (!ctx.EntityManager.HasComponent<Child>(go))
            {
                ctx.Write(Child, Entity.Null);
                ctx.Write(ChildIndex, -1);
                ctx.Trigger(Done);
                return Execution.Done;
            }

            ref State state = ref ctx.GetState(this);

            if (port == NextChild)
            {
                var children = ctx.EntityManager.GetBuffer<Child>(go);
                if (state.Index < children.Length)
                {
                    var child = children[state.Index];
                    ctx.Write(Child, child.Value);
                    ctx.Write(ChildIndex, state.Index);
                    ctx.Trigger(Out);
                    state.Index++;
                    return Execution.Done;
                }
            }

            ctx.Write(Child, Entity.Null);
            ctx.Write(ChildIndex, -1);

            if (port == Reset)
            {
                state.Index = 0;
                ctx.Trigger(Out);
                return Execution.Done;
            }

            ctx.Trigger(Done);
            return Execution.Done;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            throw new NotImplementedException();
        }
    }
}
