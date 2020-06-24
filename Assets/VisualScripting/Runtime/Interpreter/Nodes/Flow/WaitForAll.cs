using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **WaitForAll** node let the flow execute When all Input Triggers have been executed. When all ports got triggered, it will reset all input ports state, Execute the output port and restart to validate the state of all input ports..\n" +
        "\n" +
        "**Note:** You can add or remove Input by right clicking on the node and selecting (Add Input or Remove Input).")]
    public struct WaitForAll : IFlowNode<WaitForAll.State>
    {
        [PortDescription("", Description = "Trigger set its state to done.")]
        public InputTriggerMultiPort Input;
        [PortDescription(Description = "Trigger the reset of all Input port state.")]
        public InputTriggerPort Reset;
        [PortDescription(Description = "Execute next action when all input port got triggered.")]
        public OutputTriggerPort Output;


        public struct State : INodeState
        {
            public ulong Done;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);

            if (port == Reset)
            {
                state.Done = 0ul;
                return Execution.Done;
            }

            int portIndex = ctx.GetTriggeredIndex(Input, port);
            state.Done |= 1ul << portIndex;

            if (state.Done == (1ul << Input.DataCount) - 1ul)
            {
                ctx.Trigger(Output);
                state.Done = 0ul;
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
