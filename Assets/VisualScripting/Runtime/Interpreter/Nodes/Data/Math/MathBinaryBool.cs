using System;
using Modifier.Runtime.Nodes;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct MathBinaryBool : IDataNode, IHasExecutionType<MathBinaryBool.BinaryBoolType>
    {
        public enum BinaryBoolType : byte
        {
            LogicalAnd,
            LogicalOr,
            Xor,
        }

        [SerializeField]
        BinaryBoolType m_Type;

        [PortDescription("", ValueType.Bool)]
        public InputDataMultiPort Inputs;
        [PortDescription("", ValueType.Bool)]
        public OutputDataPort Result;

        public BinaryBoolType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            if (Inputs.DataCount == 0)
            {
                ctx.Write(Result, 0);
            }
            else
            {
                bool result = ctx.ReadBool(Inputs.SelectPort(0));
                for (uint i = 1; i < Inputs.DataCount; ++i)
                    result = Op(result, ctx.ReadBool(Inputs.SelectPort(i)));
                ctx.Write(Result, result);
            }
        }

        bool Op(bool a, bool b)
        {
            switch (Type)
            {
                case BinaryBoolType.LogicalAnd:
                    return a && b;
                case BinaryBoolType.LogicalOr:
                    return a || b;
                case BinaryBoolType.Xor:
                    return a ^ b;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
