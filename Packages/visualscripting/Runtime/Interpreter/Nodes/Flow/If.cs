using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **If** condition control flow node will change the flow by checking if the condition is true or false.")]
    public struct If : IFlowNode
    {
        [PortDescription("", Description = "Trigger the condition validation.")]
        public InputTriggerPort Input;

        [PortDescription(ValueType.Bool, "", Description = "The condition to validate. False by default.")]
        public InputDataPort Condition;

        [PortDescription("True", Description = "Execute next action when the condition is True.")]
        public OutputTriggerPort IfTrue;

        [PortDescription("False", Description = "Execute next action when the condition is False.")]
        public OutputTriggerPort IfFalse;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ctx.Trigger(ctx.ReadBool(Condition) ? IfTrue : IfFalse);
        }
    }
}
