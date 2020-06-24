using System;
using Unity.Mathematics;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Convert the euler angles to a quaternion.")]
    public struct RotationEuler : IDataNode
    {
        [PortDescription(ValueType.Float3, Description = "Euler angles in degrees")]
        public InputDataPort Euler;
        [PortDescription(ValueType.Quaternion, Description = "Return the euler value in a quaternion.")]
        public OutputDataPort Value;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Write(Value, quaternion.Euler(math.radians(ctx.ReadFloat3(Euler))));
        }
    }
}
