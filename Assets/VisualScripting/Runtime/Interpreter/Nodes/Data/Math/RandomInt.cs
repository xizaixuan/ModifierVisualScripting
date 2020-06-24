using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("Return a random integer number between Min [inclusive] and Max [exclusive]. Note: Random with Min = 0 and Max = 10 can return a number between 0 and 9.")]
    public struct RandomInt : IDataNode
    {
        [PortDescription(ValueType.Int, Description = "The minimum number used for the random generation.")]
        public InputDataPort Min;
        [PortDescription(ValueType.Int, DefaultValue = int.MaxValue, Description = "The maximum number used for the random generation. This number will never be picked")]
        public InputDataPort Max;
        [PortDescription(ValueType.Int, Description = "Return the random integer number generated.")]
        public OutputDataPort Result;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            int minInt = ctx.ReadInt(Min);
            int maxInt = ctx.ReadInt(Max);
            ctx.Write(Result, ctx.Random.NextInt(minInt, maxInt));
        }
    }
}
