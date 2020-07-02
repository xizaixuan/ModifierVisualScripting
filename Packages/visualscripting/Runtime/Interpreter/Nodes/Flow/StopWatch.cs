using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **Stop Watch** node let the flow execute when time duration is reached.")]
    public struct StopWatch : IFlowNode<StopWatch.WaitState>, INodeReportProgress
    {
        public struct WaitState : INodeState
        {
            public float Elapsed;
            public bool Running;
        }
        [PortDescription(Description = "Trigger the start of the timer if not finished. Restart the timer if duration time is reached and the trigger is executed.")]
        public InputTriggerPort Start;                // Start/Restart the timer
        [PortDescription(Description = "Trigger stop and reset the timer.")]
        public InputTriggerPort Stop;            // Start/Restart the timer
        [PortDescription(Description = "Pauses the timer. Will start from current time when triggering the Start input again.")]
        public InputTriggerPort Pause;               // Stop/Pause the timer
        [PortDescription(Description = "Trigger Reset the timer to 0. If the timer wasn't done, it will restart.")]
        public InputTriggerPort Reset;              // Reset the timer to 0
        [PortDescription(Description = "Execute next action even if duration is not reached.")]
        public OutputTriggerPort Output;            // Triggered every time
        [PortDescription("On Done", Description = "Execute next action when the duration is reached.")]
        public OutputTriggerPort Done;              // Triggered when Duration is reached
        [PortDescription(ValueType.Float, Description = "Duration time in seconds")]
        public InputDataPort Duration;               // Stopwatch duration
        [PortDescription(ValueType.Float, Description = "Return the seconds elapsed since the start of the Stop Watch")]
        public OutputDataPort Elapsed;              // Time elapsed since start
        [PortDescription(ValueType.Float, Description = "Return the percentage progress of the time. Value is between 0.0 to 1.0.")]
        public OutputDataPort Progress;             // Value from [0,1], where 0 is at reset time, and 1 when reaching Duration

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref WaitState state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            StartStopPauseResetBehaviour.Execute(in port, in Start, in Stop, in Pause, in Reset, ref state.Running, ref state.Elapsed);
            var exec = StartStopPauseResetBehaviour.CheckCompletion(ref state.Running, ref state.Elapsed, duration,
                out bool finished);
            if (finished)
            {
                ctx.Write(Elapsed, duration);
                ctx.Write(Progress, 1f);
                ctx.Trigger(Output);
                ctx.Trigger(Done);
            }
            return exec;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref WaitState state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            var res = StartStopPauseResetBehaviour.Update(ref state.Running, ref state.Elapsed, ctx.Time.DeltaTime, duration,
                out bool updated, out bool finished);

            if (finished)
            {
                ctx.Write(Elapsed, duration);
                ctx.Write(Progress, 1f);
                ctx.Trigger(Output);
                ctx.Trigger(Done);
            }
            else if (updated)
            {
                ctx.Write(Elapsed, state.Elapsed);
                ctx.Write(Progress, duration > 0f ? state.Elapsed / duration : 1f);
                ctx.Trigger(Output);
            }

            return res;
        }

        public byte GetProgress<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref WaitState state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);
            return (byte)(duration <= 0 ? 0 : (byte.MaxValue * state.Elapsed / duration));
        }
    }
}
