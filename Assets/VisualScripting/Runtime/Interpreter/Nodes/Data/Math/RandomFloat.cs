using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return a random float number between Min [inclusive] and Max [exclusive]. Note: Random with Min = 0.0 and Max = to 1.0, can't return 1.0f.")]
    public struct RandomFloat : IDataNode
    {
        [PortDescription(ValueType.Float, Description = "The minimum number used for the random generation.")]
        public InputDataPort Min;
        [PortDescription(ValueType.Float, DefaultValue = 1.0f, Description = "The maximum number used for the random generation. This number will never be picked")]
        public InputDataPort Max;
        [PortDescription(ValueType.Float, Description = "Return the random float number generated.")]
        public OutputDataPort Result;
        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            float minFloat = ctx.ReadFloat(Min);
            float maxFloat = ctx.ReadFloat(Max);
            ctx.Write(Result, ctx.Random.NextFloat(minFloat, maxFloat));
        }
    }
}
