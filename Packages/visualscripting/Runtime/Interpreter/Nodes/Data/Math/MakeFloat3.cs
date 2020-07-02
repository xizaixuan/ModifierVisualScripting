using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Creates a new vector3 with given [X, Y, Z] values.")]
    public struct MakeFloat3 : IDataNode
    {
        [PortDescription(ValueType.Float, Description = "Float value for the first value X.")]
        public InputDataPort X;
        [PortDescription(ValueType.Float, Description = "Float value for the second value Y.")]
        public InputDataPort Y;
        [PortDescription(ValueType.Float, Description = "Float value for last value Z.")]
        public InputDataPort Z;
        [PortDescription(ValueType.Float3, Description = "Return a new vector3 composed with [X, Y, Z].")]
        public OutputDataPort Value;
        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(Value, new float3(ctx.ReadFloat(X), ctx.ReadFloat(Y), ctx.ReadFloat(Z)));
        }
    }

    [Serializable]
    public struct SplitFloat3 : IDataNode
    {
        [PortDescription(ValueType.Float3, Description = "Vector3 that will be split.")]
        public InputDataPort Value;
        [PortDescription(ValueType.Float, Description = "Return a float from the first value X in the vector3.")]
        public OutputDataPort X;
        [PortDescription(ValueType.Float, Description = "Return a float from the second value Y in the vector3.")]
        public OutputDataPort Y;
        [PortDescription(ValueType.Float, Description = "Return a float from the last value Z in the vector3.")]
        public OutputDataPort Z;
        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            var f = ctx.ReadFloat3(Value);
            ctx.Write(X, f.x);
            ctx.Write(Y, f.y);
            ctx.Write(Z, f.z);
        }
    }
}
