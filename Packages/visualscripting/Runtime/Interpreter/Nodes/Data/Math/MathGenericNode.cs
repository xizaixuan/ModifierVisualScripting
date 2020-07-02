using System;
using Modifier.Runtime.Mathematics;
using Modifier.Runtime.Nodes;
using Unity.Assertions;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct MathGenericNode : IDataNode, IHasExecutionType<MathGeneratedFunction>
    {
        // just to distinguish that from a common ulong in the Properties visitor for the Node Inspector
        [Serializable]
        public struct MathGeneratedNodeSerializable
        {
            [SerializeField] private ulong _Function;
            public MathGeneratedFunction Function
            {
                get => (MathGeneratedFunction)_Function;
                set => _Function = (ulong)value;
            }
        }

        [HideInInspector]
        [SerializeField]
        int m_GenerationVersion;
        public int GenerationVersion
        {
            get => m_GenerationVersion;
            set => m_GenerationVersion = value;
        }

        [SerializeField]
        private MathGeneratedNodeSerializable _Function;

        public MathGeneratedFunction Function
        {
            get => _Function.Function;
            set => _Function.Function = value;
        }

        [PortDescription("", ValueType.Float)]
        public InputDataMultiPort Inputs;
        [PortDescription("", ValueType.Float)]
        public OutputDataPort Result;

        public MathGeneratedFunction Type
        {
            get => Function;
            set => Function = value;
        }

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            Assert.AreEqual(GenerationVersion, MathGeneratedDelegates.GenerationVersion);
            var result = ctx.ApplyBinMath(Inputs, Function);
            ctx.Write(Result, result);
        }
    }
}
