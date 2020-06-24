using System;
using Modifier.Runtime.Nodes;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct MathBinaryNumberBool : IDataNode, IHasExecutionType<MathBinaryNumberBool.BinaryNumberType>
    {
        public enum BinaryNumberType : byte
        {
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual,
        }

        [SerializeField]
        BinaryNumberType m_Type;

        [PortDescription("", ValueType.Float)]
        public InputDataPort A;
        [PortDescription("", ValueType.Float)]
        public InputDataPort B;
        [PortDescription("", ValueType.Bool)]
        public OutputDataPort Result;

        public BinaryNumberType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var a = ctx.ReadFloat(A);
            var b = ctx.ReadFloat(B);
            bool value;
            switch (Type)
            {
                case BinaryNumberType.GreaterThan:
                    value = a > b;
                    break;
                case BinaryNumberType.GreaterThanOrEqual:
                    value = a >= b;
                    break;
                case BinaryNumberType.LessThan:
                    value = a < b;
                    break;
                case BinaryNumberType.LessThanOrEqual:
                    value = a <= b;
                    break;
                default:
                    throw new NotImplementedException();
            }

            ctx.Write(Result, value);
        }
    }
}
