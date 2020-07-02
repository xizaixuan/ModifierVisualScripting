using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **Wait** node let the flow execute when time duration is reached. When duration is reached, the node will reset and restart if start is triggered another time.")]
    public struct Wait : IFlowNode<Wait.State>, INodeReportProgress
    {
        public struct State : INodeState
        {
            public float Elapsed;
            public bool Running;
        }

        [PortDescription(Description = "Trigger the start of the wait time if not finished. Restart the wait if duration time is reached and the trigger is executed.")]
        public InputTriggerPort Start;
        [PortDescription(Description = "Trigger stop and reset the current wait time. Won't Execute On Done action.")]
        public InputTriggerPort Stop;
        [PortDescription(Description = "Pauses the wait time. Will start from current wait time when triggering the Start input again.")]
        public InputTriggerPort Pause;
        [PortDescription(Description = "Trigger Reset the wait time to 0. If the current wait time is not finish and wait time is not paused, it will restart the wait action.")]
        public InputTriggerPort Reset;
        [PortDescription("On Done", Description = "Execute next action when the wait time duration is reached.")]
        public OutputTriggerPort OnDone;
        [PortDescription(ValueType.Float, Description = "Wait time duration in seconds")]
        public InputDataPort Duration;

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            StartStopPauseResetBehaviour.Execute(in port, in Start, in Stop, in Pause, in Reset, ref state.Running, ref state.Elapsed);
            var exec = StartStopPauseResetBehaviour.CheckCompletion(ref state.Running, ref state.Elapsed, duration,
                out bool finished);
            if (finished)
            {
                ctx.Trigger(OnDone);
            }
            return exec;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref var state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            var res = StartStopPauseResetBehaviour.Update(ref state.Running, ref state.Elapsed, ctx.Time.DeltaTime, duration,
                out _, out bool finished);

            if (finished)
            {
                ctx.Trigger(OnDone);
            }

            return res;
        }

        public byte GetProgress<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            var seconds = ctx.ReadFloat(Duration);
            return (byte)(seconds <= 0 ? 0 : (byte.MaxValue * state.Elapsed / seconds));
        }
    }
}
