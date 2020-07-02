using System;
using UnityEngine;

namespace Modifier.Runtime
{
    // Node that inspects some data every frame and will fire its output pin when the observed data changes.
    [Serializable]
    [NodeDescription("The **On Change** node inspects the Input data every frame and execute its output port when the observed data changes.\n" +
        "\n" +
        "**How to use:** To use this node, you will need to Start it with the data you want to validate. When the node is started, it will always execute you should not Continue to trigger the Start port as it will override the Input value you track and won't validate a modification of data as it was restarted. When connecting an edge in the Stop port, you will stop the validation of the Input value modification.\n" +
        "\n" +
        "**Warning**: Linking On Update to the Start port will restart the node and will never validate that the Input data changed.")]
    public struct OnChange : IFlowNode<OnChange.State>
    {
        public struct State : INodeState
        {
            public Value LastValue;
        }

        [PortDescription(Description = "Trigger signal to start observing the data.")]
        public InputTriggerPort Start;          // The signal to start observing the data.
        [PortDescription(Description = "Trigger signal to stop observing the data.")]
        public InputTriggerPort Stop;           // The signal to stop observing the data.
        [PortDescription("", Description = "Execute next action when the Input observed data changed.")]
        public OutputTriggerPort OnChanged;      // Fires when the observed data changed.
        [PortDescription(Description = "The data we want to validate. Can be of any type.")]
        public InputDataPort Input;          // The data to observe.

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            Execution Result = Execution.Done;
            ref State state = ref ctx.GetState(this);

            if (port == Start)
            {
                ctx.Log("NodeId.OnChange StartObserving");
                Result = Execution.Running;
                state.LastValue = ctx.ReadValue(Input);
            }
            else
            {
                ctx.Log("NodeId.StopWatch Stop");
            }

            return Result;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);

            Runtime.Value Temp = ctx.ReadValue(Input);
            if (!Temp.Equals(state.LastValue))
                ctx.Trigger(OnChanged);
            state.LastValue = Temp;

            return Execution.Running;
        }
    }
}
