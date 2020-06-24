using System;
using Unity.Mathematics;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    public struct CombineQuaternionRotations : IDataNode
    {
        [PortDescription("", ValueType.Quaternion)]
        public InputDataPort A;
        [PortDescription("", ValueType.Quaternion)]
        public InputDataPort B;
        [PortDescription(ValueType.Quaternion)]
        public OutputDataPort Result;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var a = ctx.ReadQuaternion(A);
            var b = ctx.ReadQuaternion(B);
            Value result = math.mul(a, b);
            ctx.Write(Result, result);
        }
    }
}
