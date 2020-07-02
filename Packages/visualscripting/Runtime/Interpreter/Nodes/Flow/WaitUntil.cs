using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **WaitUntil** node let the flow execute when the boolean condition is true.")]
    public struct WaitUntil : IFlowNode<WaitUntil.State>
    {
        public struct State : INodeState
        {
            public bool waiting;
        }

        [PortDescription("", Description = "Trigger the condition validation.")]
        public InputTriggerPort Start;
        [PortDescription(ValueType.Bool, Description = "The condition Value. If true, it can execute.")]
        public InputDataPort Condition;
        [PortDescription("", Description = "Execute the next action when the condition is true.")]
        public OutputTriggerPort OnDone;

        Execution CheckCompletion<TCtx>(TCtx ctx, ref State state) where TCtx : IGraphInstance
        {
            if (state.waiting && ctx.ReadBool(Condition))
            {
                state.waiting = false;
                ctx.Trigger(OnDone);
                return Execution.Done;
            }

            return state.waiting ? Execution.Running : Execution.Done;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);

            if (port == Start)
            {
                state.waiting = true;
            }

            return CheckCompletion(ctx, ref state);
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            return CheckCompletion(ctx, ref state);
        }
    }
}
