namespace Modifier.Runtime
{
    public struct GraphTriggerInput : IEntryPointNode
    {
        public OutputTriggerPort Output;
        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ctx.Trigger(Output);
        }
    }
}