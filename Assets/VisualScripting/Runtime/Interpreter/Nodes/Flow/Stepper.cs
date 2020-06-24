using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    /// <summary>
    /// node that takes a single input trigger and that will fire a different user defined output, sequentially, every time an input signal is received to
    /// that I can control the executions of a sequence of node.
    /// For example, given the user defined output 1, 2 and 3:
    ///
    /// - Upon getting the input a first time, the node would fire output 1
    /// - Upon getting the input a second time, the node would fire output 2
    /// - Upon getting the input a third time, the node would fire output 3
    /// - Upon getting the input a fourth time, the node would fire output 1 again
    ///
    /// Expected execution with Hold:
    ///     1, 2, 3, 3, (...), 3
    ///
    /// Expected execution with Ping Pong:
    ///     1, 2, 3, 2, 1, 2, 3, 2, 1, ...
    /// </summary>
    [Serializable]
    [NodeDescription("The **Stepper** node will Execute a different Output flow every time the input port In is executed. The order of execution of the ports will change depending of the selected mode." +
        "\n" +
        "**Mode:**" +
        "Mode can be changed in the Visual script inspector.\n" +
        "- Hold\n" +
        "    Every time the Stepper node input port is triggered, it will pass through the next execution port until the last one, were it will be stuck on it until the Reset input port is triggered.\n" +
        "    Expected execution with Hold: 1, 2, 3, 3, (...), 3\n" +
        "- Loop\n" +
        "    Every time the Stepper node input port is triggered, it will pass through the next execution port. When the last port is executed, it restart from the first one.\n" +
        "    Expected execution with Hold: 1, 2, 3, 1, 2, 3\n" +
        "- PingPong\n" +
        "    Every time the Stepper node input port is triggered, it will pass through the next execution port. When the last port is executed, the order is inverted. The order will change like an infinite exchange in a ping pong game.\n" +
        "    Expected execution with Ping Pong: 1, 2, 3, 2, 1, 2, 3, 2, 1, ...\n" +
        "\n" +
        "**Note:** You can add or remove execution ports by right clicking on the node and selecting ( Add Step, Remove Step).\n")]
    public struct Stepper : IFlowNode<Stepper.State>
    {
        public struct State : INodeState
        {
            public uint _index;
        }

        public enum OrderMode
        {
            Hold, Loop, PingPong
        }

        public OrderMode Mode;
        [PortDescription(Description = "Trigger the next execution port based on the selected mode.")]
        public InputTriggerPort In;
        [PortDescription(Description = "Trigger the first execution port.")]
        public InputTriggerPort Reset;
        [PortDescription(Description = "Execute next action from the current execution port.")]
        public OutputTriggerMultiPort Step;

        public uint MaxStepIndex => (uint)Step.GetDataCount();

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);

            if (port == In)
            {
                ctx.Log($"NodeId.Sequence In - internal index: {state._index}");
                switch (Mode)
                {
                    case OrderMode.Hold:
                        ctx.Trigger(Step.SelectPort(state._index));
                        state._index = math.min(state._index + 1, MaxStepIndex - 1);
                        break;
                    case OrderMode.Loop:
                        ctx.Trigger(Step.SelectPort(state._index));
                        state._index = (state._index + 1) % MaxStepIndex;
                        break;
                    case OrderMode.PingPong:
                        var index = state._index;
                        if (index >= MaxStepIndex)
                            index = (MaxStepIndex - 2) - (index % MaxStepIndex);
                        ctx.Trigger(Step.SelectPort(index));
                        state._index = (state._index + 1) % (MaxStepIndex * 2 - 2);
                        break;
                }
            }
            else // Reset
            {
                ctx.Log("NodeId.Sequence Reset");
                state._index = 0;
            }

            return Execution.Done;;
        }

        // Update should never be called
        public Execution Update<TCtx>(TCtx _) where TCtx : IGraphInstance
        {
            return Execution.Done;
        }
    }
}
