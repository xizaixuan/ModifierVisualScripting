using System;

namespace Modifier.Runtime
{
    public struct GraphTriggerOutput : IFlowNode<GraphTriggerOutput.EmptyState>
    {
        public struct EmptyState : INodeState { }

        public InputTriggerPort Input;
        public uint OutputIndex;

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            throw new NotImplementedException();
        }

        Execution IStateFlowNode.Execute<TCtx>(TCtx ctx, InputTriggerPort port)
        {
            return ctx.TriggerGraphOutput(OutputIndex);
        }
    }
}