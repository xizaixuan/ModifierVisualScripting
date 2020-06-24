using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Modifier.Runtime
{
    struct VisualScriptingEventData
    {
        public bool IsFromGraph;
        public ulong EventTypeHash;
        public IntPtr EventPtr;
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public unsafe class VisualScriptingEventSystem : ComponentSystem
    {
        Dictionary<int, List<VisualScriptingEventStream>> m_EventsPerFrame;
        NativeList<VisualScriptingEventData> m_Events;
        JobHandle m_ProducerHandle;

        internal NativeList<VisualScriptingEventData> Events => m_Events;

        protected override void OnCreate()
        {
            m_EventsPerFrame = new Dictionary<int, List<VisualScriptingEventStream>>();
            m_Events = new NativeList<VisualScriptingEventData>(Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            DisposePreviousFrameStreams();
            BuildEvents();
            m_ProducerHandle.Complete();
            m_ProducerHandle = default;
        }

        protected override void OnDestroy()
        {
            m_ProducerHandle.Complete();

            foreach (var stream in m_EventsPerFrame.Values.SelectMany(streams => streams))
            {
                stream.Dispose();
            }

            DisposeEvents();
        }

        internal byte* WriteFromNode(ulong typeHash, int typeSize)
        {
            var writer = CreateEventWriter(1);
            return writer.Allocate(typeHash, typeSize, true, 0);
        }

        public VisualScriptingEventStream.Writer CreateEventWriter(int forEachCount)
        {
            var stream = new VisualScriptingEventStream(forEachCount);
            var currentFrame = UnityEngine.Time.frameCount;

            if (m_EventsPerFrame.TryGetValue(currentFrame, out var streams))
                streams.Add(stream);
            else
                m_EventsPerFrame.Add(currentFrame, new List<VisualScriptingEventStream> { stream });

            return stream.AsWriter();
        }

        public JobHandle AddJobHandleForProducer(JobHandle producerJob)
        {
            m_ProducerHandle = JobHandle.CombineDependencies(m_ProducerHandle, producerJob);
            return m_ProducerHandle;
        }

        void BuildEvents()
        {
            DisposeEvents();

            m_Events = new NativeList<VisualScriptingEventData>(Allocator.Persistent);

            if (m_EventsPerFrame.TryGetValue(UnityEngine.Time.frameCount, out var streams))
            {
                foreach (var stream in streams)
                {
                    var reader = stream.AsReader();
                    for (var i = 0; i < reader.ForEachCount; i++)
                    {
                        reader.BeginForEachIndex(i);
                        while (reader.RemainingItemCount > 0)
                        {
                            var hash = reader.Read<ulong>();
                            var size = reader.Read<int>();
                            var isFromGraph = reader.Read<bool>();
                            var evt = reader.ReadUnsafePtr(size);

                            m_Events.Add(new VisualScriptingEventData
                            {
                                EventPtr = new IntPtr(evt),
                                EventTypeHash = hash,
                                IsFromGraph = isFromGraph
                            });
                        }
                        reader.EndForEachIndex();
                    }
                }
            }
        }

        void DisposeEvents()
        {
            if (m_Events.IsCreated)
                m_Events.Dispose();
        }

        void DisposePreviousFrameStreams()
        {
            var previousFrame = UnityEngine.Time.frameCount - 1;
            if (previousFrame == 0)
                return;

            if (m_EventsPerFrame.TryGetValue(previousFrame, out var streams))
            {
                foreach (var stream in streams)
                {
                    stream.Dispose();
                }
            }

            m_EventsPerFrame.Remove(previousFrame);
        }
    }
}