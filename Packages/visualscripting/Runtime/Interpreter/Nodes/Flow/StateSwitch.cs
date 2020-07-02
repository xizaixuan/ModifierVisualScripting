using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **State Switch** node will change a boolean value depending of the input port that is executed.")]
    public struct StateSwitch : IFlowNode
    {
        [PortDescription("Set True", Description = "Trigger the state change to true.")]
        public InputTriggerPort SetTrue;
        [PortDescription("Set False", Description = "Trigger the state change to false.")]
        public InputTriggerPort SetFalse;
        [PortDescription("", Description = "Execute next action after the state is sate to true or false.")]
        public OutputTriggerPort Done;
        [PortDescription(ValueType.Bool, name: "State", DefaultValue = false, Description = "Return the State value (true or false) depending of the input port that was Executed.")]
        public OutputDataPort State;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ctx.Write(State, port.GetPort().Index == SetTrue.GetPort().Index);
            ctx.Trigger(Done);
        }
    }
}
