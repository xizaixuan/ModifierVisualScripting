using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Modifier.Runtime
{
    [JobProducerType(typeof(ExecuteUserEventReceiverJobs<,>))]
    public interface IVisualScriptingEventReceiverJob<in T> where T : struct, IVisualScriptingEvent
    {
        void Execute(T visualScriptingEvent);
    }

    [JobProducerType(typeof(ExecuteUserEventPtrReceiverJobs<>))]
    interface IVisualScriptingEventPtrReceiverJob
    {
        void Execute(VisualScriptingEventData data);
    }

    // ReSharper disable once InconsistentNaming
    public static class IVisualScriptingEventReceiverJobExtensions
    {
        public static unsafe JobHandle Schedule<TJob, TEvent>(
            this TJob job,
            VisualScriptingEventSystem eventSystem,
            JobHandle inputDeps = default)
            where TJob : struct, IVisualScriptingEventReceiverJob<TEvent>
            where TEvent : struct, IVisualScriptingEvent
        {
            var data = new EventReceiverJobData<TJob, TEvent>
            {
                UserJob = job,
                EventData = eventSystem.Events
            };

            var handle = eventSystem.AddJobHandleForProducer(inputDeps);
            var parameters = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref data),
                ExecuteUserEventReceiverJobs<TJob, TEvent>.Initialize(),
                handle,
                ScheduleMode.Batched);

            return JobsUtility.Schedule(ref parameters);
        }

        internal static unsafe JobHandle Schedule<T>(
            this T job,
            VisualScriptingEventSystem eventSystem,
            JobHandle inputDeps = default)
            where T : struct, IVisualScriptingEventPtrReceiverJob
        {
            var data = new EventPtrReceiverJobData<T>
            {
                UserJob = job,
                EventData = eventSystem.Events
            };

            var handle = eventSystem.AddJobHandleForProducer(inputDeps);
            var parameters = new JobsUtility.JobScheduleParameters(
                UnsafeUtility.AddressOf(ref data),
                ExecuteUserEventPtrReceiverJobs<T>.Initialize(),
                handle,
                ScheduleMode.Batched);

            return JobsUtility.Schedule(ref parameters);
        }
    }

    struct EventReceiverJobData<TJob, TEvent>
        where TJob : struct, IVisualScriptingEventReceiverJob<TEvent>
        where TEvent : struct, IVisualScriptingEvent
    {
        public TJob UserJob;

        [NativeDisableContainerSafetyRestriction]
        public NativeList<VisualScriptingEventData> EventData;
    }

    struct EventPtrReceiverJobData<T> where T : struct, IVisualScriptingEventPtrReceiverJob
    {
        public T UserJob;

        [NativeDisableContainerSafetyRestriction]
        public NativeList<VisualScriptingEventData> EventData;
    }

    struct ExecuteUserEventReceiverJobs<TJob, TEvent>
        where TJob : struct, IVisualScriptingEventReceiverJob<TEvent>
        where TEvent : struct, IVisualScriptingEvent
    {
        static IntPtr s_JobReflectionData;
        static readonly ulong k_TypeHash = TypeHash.CalculateStableTypeHash(typeof(TEvent));

        delegate void ExecuteDelegate(ref EventReceiverJobData<TJob, TEvent> jobData);

        public static IntPtr Initialize()
        {
            if (s_JobReflectionData == IntPtr.Zero)
            {
                s_JobReflectionData = JobsUtility.CreateJobReflectionData(
                    typeof(EventReceiverJobData<TJob, TEvent>),
                    typeof(TJob),
                    JobType.Single,
                    (ExecuteDelegate)Execute);
            }

            return s_JobReflectionData;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // Has to be public to be found by Burst reflection
        public static void Execute(ref EventReceiverJobData<TJob, TEvent> data)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Burst doesn't support foreach
            for (var i = 0; i < data.EventData.Length; i++)
            {
                var eventData = data.EventData[i];
                if (eventData.EventTypeHash == k_TypeHash)
                {
                    var evt = Marshal.PtrToStructure<TEvent>(eventData.EventPtr);
                    data.UserJob.Execute(evt);
                }
            }
        }
    }

    struct ExecuteUserEventPtrReceiverJobs<T> where T : struct, IVisualScriptingEventPtrReceiverJob
    {
        static IntPtr s_JobReflectionData;

        delegate void ExecuteDelegate(ref EventPtrReceiverJobData<T> jobData);

        public static IntPtr Initialize()
        {
            if (s_JobReflectionData == IntPtr.Zero)
            {
                s_JobReflectionData = JobsUtility.CreateJobReflectionData(
                    typeof(EventPtrReceiverJobData<T>),
                    typeof(T),
                    JobType.Single,
                    (ExecuteDelegate)Execute);
            }

            return s_JobReflectionData;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // Has to be public to be found by Burst reflection
        public static void Execute(ref EventPtrReceiverJobData<T> data)
        {
            // ReSharper disable once ForCanBeConvertedToForeach
            // Burst doesn't support foreach
            for (var i = 0; i < data.EventData.Length; i++)
            {
                data.UserJob.Execute(data.EventData[i]);
            }
        }
    }
}