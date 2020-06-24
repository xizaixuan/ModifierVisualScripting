using System;
using Modifier.Runtime.Nodes;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct ComparisonBinary : IDataNode, IHasExecutionType<ComparisonBinary.ComparisonBinaryType>
    {
        public enum ComparisonBinaryType : byte
        {
            Equals,
            NotEquals,
        }

        [SerializeField]
        ComparisonBinaryType m_Type;

        [PortDescription("", ValueType.Unknown)]
        public InputDataPort A;
        [PortDescription("", ValueType.Unknown)]
        public InputDataPort B;
        [PortDescription("", ValueType.Bool)]
        public OutputDataPort Result;

        public ComparisonBinaryType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var a = ctx.ReadValue(A);
            var b = ctx.ReadValue(B);

            bool value = (Type == ComparisonBinaryType.Equals) == a.Equals(b);
            ctx.Write(Result, value);
        }
    }
}
