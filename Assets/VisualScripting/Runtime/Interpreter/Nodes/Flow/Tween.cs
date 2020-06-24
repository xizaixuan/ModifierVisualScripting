using System;
using Modifier.Runtime.Nodes;
using Unity.Mathematics;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription(InterpolationType.Linear, "Linear interpolation between 2 values within a time interval.")]
    [NodeDescription(InterpolationType.SmoothStep, "Smooth step interpolation between 2 values within a time interval.")]
    public struct Tween : IFlowNode<Tween.State>, IHasExecutionType<InterpolationType>, INodeReportProgress
    {
        public enum ETweenValueType
        {
            Float,
            Vector2,
            Vector3,
            Vector4,
            Int,
        }

        public struct State : INodeState
        {
            public float Elapsed;
            public bool Running;
        }

        [PortDescription(Description = "Trigger to start the interpolation.")]
        public InputTriggerPort Start;

        [PortDescription(Description = "Trigger to stop and reset the interpolation.")]
        public InputTriggerPort Stop;

        [PortDescription(Description = "Trigger to pause the interpolation.")]
        public InputTriggerPort Pause;

        [PortDescription(Description = "Resets the internal timer.")]
        public InputTriggerPort Reset;

        [PortDescription(ValueType.Float, Description = "The value to interpolate from.")]
        public InputDataPort From;

        [PortDescription(ValueType.Float, Description = "The value to interpolate to.")]
        public InputDataPort To;

        [PortDescription(ValueType.Float, Description = "The duration of the interpolation (in seconds).")]
        public InputDataPort Duration;

        [PortDescription(Description = "Execute next action at every frame during the interpolation (i.e. not while paused).")]
        public OutputTriggerPort OnFrame;

        [PortDescription("On Done", Description = "Execute next action when the interpolation runs to completion (i.e. Stop is not triggered).")]
        public OutputTriggerPort OnDone;

        [PortDescription(ValueType.Float, "Current", DefaultValue = 0f, Description = "Return the interpolated value between " +
                "To and From at the current time.")]
        public OutputDataPort Result;

        [SerializeField]
        InterpolationType m_Type;
        public ETweenValueType TweenValueType;

        public InterpolationType Type
        {
            get => m_Type;
            set => m_Type = value;
        }

        public Execution Execute<TCtx>(TCtx ctx, InputTriggerPort port) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            StartStopPauseResetBehaviour.Execute(in port, in Start, in Stop, in Pause, in Reset, ref state.Running, ref state.Elapsed);
            var exec = StartStopPauseResetBehaviour.CheckCompletion(ref state.Running, ref state.Elapsed, duration,
                out bool finished);
            if (finished)
            {
                ctx.Write(Result, ctx.ReadValue(To));
                ctx.Trigger(OnDone);
            }
            else if (state.Running)
            {
                var progress = duration > 0 ? state.Elapsed / duration : 1.0f;
                ctx.Write(Result, Interpolate(TweenValueType, Type, progress, ctx.ReadValue(From), ctx.ReadValue(To)));
                ctx.Trigger(OnFrame);
            }

            return exec;
        }

        public Execution Update<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref var state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);

            var res = StartStopPauseResetBehaviour.Update(ref state.Running, ref state.Elapsed, ctx.Time.DeltaTime, duration,
                out bool updated, out bool finished);

            if (finished)
            {
                ctx.Write(Result, ctx.ReadValue(To));
                ctx.Trigger(OnFrame);
                ctx.Trigger(OnDone);
            }
            else if (updated)
            {
                var progress = duration > 0 ? state.Elapsed / duration : 1.0f;
                ctx.Write(Result, Interpolate(TweenValueType, Type, progress, ctx.ReadValue(From), ctx.ReadValue(To)));
                ctx.Trigger(OnFrame);
            }

            return res;
        }

        internal static Value Interpolate(ETweenValueType tweenValueType, InterpolationType type, float progress, Value fromValue,
            Value toValue)
        {
            switch (tweenValueType)
            {
                case ETweenValueType.Float:
                    return type == InterpolationType.Linear
                        ? math.lerp(fromValue.Float, toValue.Float, progress)
                        : SmoothStep(fromValue.Float, toValue.Float, progress);
                case ETweenValueType.Vector2:
                    return type == InterpolationType.Linear
                        ? math.lerp(fromValue.Float2, toValue.Float2, progress)
                        : SmoothStep(fromValue.Float2, toValue.Float2, progress);
                case ETweenValueType.Vector3:
                    return type == InterpolationType.Linear
                        ? math.lerp(fromValue.Float3, toValue.Float3, progress)
                        : SmoothStep(fromValue.Float3, toValue.Float3, progress);
                case ETweenValueType.Vector4:
                    return type == InterpolationType.Linear
                        ? math.lerp(fromValue.Float4, toValue.Float4, progress)
                        : SmoothStep(fromValue.Float4, toValue.Float4, progress);
                case ETweenValueType.Int:
                    return type == InterpolationType.Linear
                        ? (int)math.lerp(fromValue.Int, toValue.Int, progress)
                        : (int)SmoothStep(fromValue.Int, toValue.Int, progress);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public byte GetProgress<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            ref State state = ref ctx.GetState(this);
            var duration = ctx.ReadFloat(Duration);
            return (byte)(duration <= 0 ? 0 : byte.MaxValue * state.Elapsed / duration);
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = math.saturate(t);
            t = -2.0f * t * t * t + 3.0f * t * t;
            return (float)(to * (double)t + @from * (1.0f - t));
        }

        public static float2 SmoothStep(float2 from, float2 to, float t)
        {
            t = math.saturate(t);
            t = -2.0f * t * t * t + 3.0f * t * t;
            return to * t + from * (1.0f - t);
        }

        public static float3 SmoothStep(float3 from, float3 to, float t)
        {
            t = math.saturate(t);
            t = -2.0f * t * t * t + 3.0f * t * t;
            return to * t + from * (1.0f - t);
        }

        public static float4 SmoothStep(float4 from, float4 to, float t)
        {
            t = math.saturate(t);
            t = -2.0f * t * t * t + 3.0f * t * t;
            return to * t + from * (1.0f - t);
        }
    }
}
