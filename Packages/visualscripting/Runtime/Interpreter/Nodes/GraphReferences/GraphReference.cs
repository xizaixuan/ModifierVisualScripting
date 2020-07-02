using Unity.Assertions;
using Unity.Entities;

namespace Modifier.Runtime
{
    public struct GraphReference : IFlowNode<GraphReference.State>, INodeReportProgress
    {
        [PortDescription(ValueType.Entity)]
        public InputDataPort Target;
        public InputTriggerMultiPort Inputs;
        public OutputTriggerMultiPort Outputs;
        public InputDataMultiPort DataInputs;
        public OutputDataMultiPort DataOutputs;

        public struct State : INodeState
        {
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            Entity target = ctx.ReadEntity(Target);
            Assert.AreNotEqual(Entity.Null, target, "Referenced graph must have an entity");
            int index = ctx.GetTriggeredIndex(Inputs, port);
            return ctx.RunNestedGraph(this, target, index);
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            Entity target = ctx.ReadEntity(Target);
            return ctx.RunNestedGraph(this, target, -1);
        }

        public byte GetProgress<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            return 1;
        }
    }
}
