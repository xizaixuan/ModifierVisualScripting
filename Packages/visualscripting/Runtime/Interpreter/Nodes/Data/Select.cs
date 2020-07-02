using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Output data depending on input condition")]
    public struct Select : IDataNode
    {
        public ValueType DataValueType;

        [PortDescription(ValueType.Bool)]
        public InputDataPort Condition;
        public InputDataPort IfTrue;
        public InputDataPort Else;
        [PortDescription("", Description = "returns IfTrue value if Condition is true, Else value otherwise")]
        public OutputDataPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var condition = ctx.ReadBool(Condition);
            ctx.Write(Output, ctx.ReadValueOfType(condition ? IfTrue : Else, DataValueType));
        }
    }
}
