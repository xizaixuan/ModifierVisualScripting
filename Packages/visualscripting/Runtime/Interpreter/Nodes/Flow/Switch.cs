using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **Switch** node let the flow change its execution port when the case integer value of the selector is the same as the case value.\n" +
        "\n" +
        "**Note:** To add or remove case, right click on the node and select (Add Case or Remove Case)." +
        "\n" +
        "**Tip:** The Default output port will be executed if the selector value is in none of the Case value.")]
    public struct Switch : IFlowNode
    {
        [PortDescription(name: "", Description = "Trigger the validation of the Execution port that will execute.")]
        public InputTriggerPort Input;

        [PortDescription(Description = "Execute next action if none of the switch values match the selector value")]
        public OutputTriggerPort Default;

        [PortDescription(ValueType.Int, Description = "The value that define which Exec port will be executed based on the Case value.")]
        public InputDataPort Selector;

        [PortDescription(ValueType.Int, Description = "The value used to execute the matching Exec port.")]
        public InputDataMultiPort SwitchValues;

        [PortDescription(Description = "Execute next action if its Matching Case # condition value is met.")]
        public OutputTriggerMultiPort SwitchTriggers;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            bool anyMatched = false;
            var val = ctx.ReadInt(Selector);
            for (uint i = 0; i < SwitchValues.DataCount; i++)
            {
                if (ctx.ReadInt(SwitchValues.SelectPort(i)) == val)
                {
                    ctx.Trigger(SwitchTriggers.SelectPort(i));
                    anyMatched = true;
                }
            }
            if (!anyMatched)
                ctx.Trigger(Default);
        }
    }
}
