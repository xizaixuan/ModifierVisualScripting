using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **Toggle** node change the State value from True to False every time the Input trigger get executed. The default value before getting executed is false")]
    public struct ToggleSwitch : IFlowNode<ToggleSwitch.StateData>
    {
        [PortDescription(Description = "Trigger the toggle of the State between True and False.")]
        public InputTriggerPort Toggle;
        [PortDescription(Description = "Execute next action after toggling the value of the State.")]
        public OutputTriggerPort Output;
        [PortDescription(Description = "Return the State True or False")]
        public OutputDataPort State;
        public struct StateData : INodeState
        {
            public bool On;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref StateData state = ref ctx.GetState(this);
            state.On = !state.On;
            ctx.Write(State, state.On);
            ctx.Trigger(Output);
            return Execution.Done;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            throw new NotImplementedException();
        }
    }
}
