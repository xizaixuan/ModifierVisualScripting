using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Change the value of a variable.")]
    public struct SetVariable : IFlowNode
    {
        [PortDescription(Description = "Trigger the modification of the variable value.")]
        public InputTriggerPort Input;
        [PortDescription(Description = "Execute the next action after changing the value of the variable.")]
        public OutputTriggerPort Output;
        [PortDescription(Description = "The value that will replace the previous one in the variable.")]
        public InputDataPort Value;
        [PortDescription("")]
        public OutputDataPort OutValue;
        public ValueType VariableType;
        public VariableKind VariableKind;

        public void Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            var readValue = ctx.ReadValueOfType(Value, VariableType);
            ctx.Write(OutValue, readValue);
            ctx.Trigger(Output);
            if (VariableKind == VariableKind.OutputData)
                ctx.TriggerGraphOutput(UInt32.MaxValue);
        }
    }

    public enum VariableKind : byte
    {
        GraphVariable,
        OutputData,
    }
}