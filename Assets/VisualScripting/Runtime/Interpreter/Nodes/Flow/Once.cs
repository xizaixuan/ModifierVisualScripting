using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The Once node force anything passing through it to execute only one time. Ex: Placing this node after an On Update Event node, will make anything chained to it's output port execute only one time.")]
    public struct Once : IFlowNode<Once.State>
    {
        [PortDescription("", Description = "Trigger the execution of the node.")]
        public InputTriggerPort Input;
        [PortDescription(Description = "Reset the Done state and let its execute.")]
        public InputTriggerPort Reset;
        [PortDescription("", Description = "Execute next action after changing its state to Done.")]
        public OutputTriggerPort Output;

        public struct State : INodeState
        {
            public bool Done;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            if (port.Port == Input.Port)
            {
                if (!state.Done)
                {
                    state.Done = true;
                    ctx.Trigger(Output);
                }
            }
            else if (port.Port == Reset.Port)
            {
                state.Done = false;
            }

            return Execution.Done;
        }

        // Update should never be called
        public Execution Update<TCtx>(TCtx _) where TCtx : IGraphInstance
        {
            return Execution.Done;
        }
    }
}
