using System;
using System.IO;

namespace Modifier.Runtime
{
    [Serializable]
    public struct Negate : IDataNode
    {
        public InputDataPort Input;
        public OutputDataPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var readValue = ctx.ReadValue(Input);
            switch (readValue.Type)
            {
                case ValueType.Int:
                    readValue = -readValue.Int;
                    break;
                case ValueType.Float:
                    readValue = -readValue.Float;
                    break;
                case ValueType.Float2:
                    readValue = -readValue.Float2;
                    break;
                case ValueType.Float3:
                    readValue = -readValue.Float3;
                    break;
                case ValueType.Float4:
                    readValue = -readValue.Float4;
                    break;
                default:
                    throw new InvalidDataException($"Cannot negate a value of type {readValue.Type}");
            }
            ctx.Write(Output, readValue);
        }
    }
}
