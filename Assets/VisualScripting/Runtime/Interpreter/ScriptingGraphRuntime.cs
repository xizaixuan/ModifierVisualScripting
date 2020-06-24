#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define VS_TRACING
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
#if VS_DOTS_PHYSICS_EXISTS
using Modifier.VisualScripting.Physics;
#endif

namespace Modifier.Runtime
{
    public struct ScriptingGraphInstance : ISystemStateComponentData { }
    public struct ScriptingGraphInstanceAlive : IComponentData { }

    [InternalBufferCapacity(8)]
    public struct ValueInput : IBufferElementData
    {
        public uint Index;
        public Value Value;
    }

    public struct ScriptingGraph : ISharedComponentData, IEquatable<ScriptingGraph>
    {
        public ScriptingGraphAsset ScriptingGraphAsset;

        public bool Equals(ScriptingGraph other)
        {
            return Equals(ScriptingGraphAsset, other.ScriptingGraphAsset);
        }

        public override bool Equals(object obj)
        {
            return obj is ScriptingGraph other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ScriptingGraphAsset != null ? ScriptingGraphAsset.GetHashCode() : 0;
        }
    }

    public class ScriptingGraphRuntime : ComponentSystem
    {
#if VS_TRACING
        public static bool s_TracingEnabled;
#endif
#if VS_DOTS_PHYSICS_EXISTS
        NativeMultiHashMap<Entity, VisualScriptingPhysics.CollisionTriggerData> m_TriggerData;
        NativeMultiHashMap<Entity, VisualScriptingPhysics.CollisionTriggerData> m_CollisionData;
#endif

        Dictionary<Entity, GraphInstance> m_Contexts;
        EntityQuery m_UninitializedQuery;
        EntityQuery m_Query;
        EntityQueryMask m_BeingDestroyedQueryMask;
        VisualScriptingEventSystem m_EventSystem;
        IReadOnlyDictionary<ulong, List<FieldDescription>> m_EventFieldDescriptions;
        List<EventNodeData> m_DispatchedEvents = new List<EventNodeData>();
        bool m_ShouldTriggerEventJob;

#if UNITY_EDITOR // Live edit
        public static int LastVersion;
        private int m_Version;
#endif

        protected override void OnCreate()
        {
            m_Contexts = new Dictionary<Entity, GraphInstance>();
            m_UninitializedQuery = GetEntityQuery(typeof(ScriptingGraph), ComponentType.Exclude<ScriptingGraphInstance>());
            m_Query = GetEntityQuery(typeof(ScriptingGraphInstance));
            var beingDestroyedQuery = GetEntityQuery(typeof(ScriptingGraphInstance), ComponentType.Exclude<ScriptingGraphInstanceAlive>());
            m_BeingDestroyedQueryMask = EntityManager.GetEntityQueryMask(beingDestroyedQuery);
            m_OutputTriggersPerEntityGraphActivated = new NativeMultiHashMap<Entity, uint>(100, Allocator.Persistent);
            m_OutputTriggersPerEntityGraphActivated2 = new NativeMultiHashMap<Entity, uint>(100, Allocator.Persistent);
            m_InputTriggersPerEntityGraphActivated = new NativeHashMap<Entity, uint>(100, Allocator.Persistent);
            m_EventSystem = World.GetOrCreateSystem<VisualScriptingEventSystem>();
            m_EventFieldDescriptions = new Dictionary<ulong, List<FieldDescription>>();

#if VS_DOTS_PHYSICS_EXISTS
            // TODO A FULL HUNDRED
            m_TriggerData = new NativeMultiHashMap<Entity, VisualScriptingPhysics.CollisionTriggerData>(100, Allocator.Persistent);
            m_CollisionData = new NativeMultiHashMap<Entity, VisualScriptingPhysics.CollisionTriggerData>(100, Allocator.Persistent);
#endif
        }

        protected override void OnDestroy()
        {
            m_OutputTriggersPerEntityGraphActivated.Dispose();
            m_OutputTriggersPerEntityGraphActivated2.Dispose();
            m_InputTriggersPerEntityGraphActivated.Dispose();
#if VS_DOTS_PHYSICS_EXISTS
            m_TriggerData.Dispose();
            m_CollisionData.Dispose();
#endif
            foreach (var keyValuePair in m_Contexts)
            {
                keyValuePair.Value?.Dispose();
            }
            base.OnDestroy();
        }

        public int? ForcedFrameCount;

        protected int FrameCount
        {
            get
            {
                if (ForcedFrameCount.HasValue)
                    return ForcedFrameCount.Value;
                else
                    return UnityEngine.Time.frameCount;
            }
        }

        protected override void OnUpdate()
        {
#if UNITY_EDITOR // Live edit
            if (EditorApplication.isPlaying && m_Version != LastVersion)
            {
                m_Version = LastVersion;
                foreach (var contextsValue in m_Contexts.Values)
                    contextsValue?.Dispose();
                m_Contexts.Clear();
                EntityManager.RemoveComponent<ScriptingGraphInstance>(m_Query);
            }
#endif
            Entities.With(m_UninitializedQuery).ForEach((Entity e, ScriptingGraph g) =>
            {
                // Start
                var inputs = EntityManager.HasComponent<ValueInput>(e)
                    ? EntityManager.GetBuffer<ValueInput>(e)
                    : new DynamicBuffer<ValueInput>();
                GraphInstance ctx = CreateEntityContext(inputs, e, g.ScriptingGraphAsset.Definition);
#if VS_TRACING
                ctx.ScriptingGraphAssetID = g.ScriptingGraphAsset.GetInstanceID();
#endif
                EntityManager.AddComponentData(e, new ScriptingGraphInstance());
                EntityManager.AddComponentData(e, new ScriptingGraphInstanceAlive());
#if !UNITY_EDITOR // keep it for live edit
                EntityManager.RemoveComponent<ScriptingGraph>(e);
#endif
                if (!m_ShouldTriggerEventJob)
                    m_ShouldTriggerEventJob = ctx.ContainsEventReceiver;
                m_EventFieldDescriptions = ctx.EventFields;
            });

#if VS_DOTS_PHYSICS_EXISTS
            VisualScriptingPhysics.SetupCollisionTriggerData(EntityManager, FrameCount, ref m_TriggerData, m_Query, VisualScriptingPhysics.EventType.Trigger);
            VisualScriptingPhysics.SetupCollisionTriggerData(EntityManager, FrameCount, ref m_CollisionData, m_Query, VisualScriptingPhysics.EventType.Collision);
#endif
            // A list: I assume the most common case is "the entity has not been destroyed"
            NativeList<Entity> destroyed = new NativeList<Entity>(Allocator.Temp);
            Entities.With(m_Query).ForEach((Entity e) =>
            {
                var beingDestroyed = m_BeingDestroyedQueryMask.Matches(e);

                if (!m_Contexts.TryGetValue(e, out var ctx))
                    return;

                // used for random seed
                ctx.LastSystemVersion = LastSystemVersion;

                ctx.ResetFrame();

#if VS_TRACING
                if (s_TracingEnabled && ctx.FrameTrace == null)
                    ctx.FrameTrace = new DotsFrameTrace(Allocator.Persistent);
#endif

                if (beingDestroyed)
                {
                    ctx.TriggerEntryPoints<OnDestroy>();
                }
                else
                {
                    // Start
                    if (ctx.IsStarting)
                    {
                        ctx.TriggerEntryPoints<OnStart>();
                        ctx.IsStarting = false;
                    }

                    // Update
                    ctx.TriggerEntryPoints<OnUpdate>();
                    ctx.TriggerEntryPoints<OnKey>();

#if VS_DOTS_PHYSICS_EXISTS
                    TriggerPhysicsEvents(e, ref m_TriggerData, VisualScriptingPhysics.TriggerEventId);
                    TriggerPhysicsEvents(e, ref m_CollisionData, VisualScriptingPhysics.CollisionEventId);
#endif
                }
            });

            // Retrieve events that are dispatched from code
            if (m_ShouldTriggerEventJob)
                m_DispatchedEvents.AddRange(
                    VisualScriptingEventUtility.GetEventsFromApi(m_EventSystem, m_EventFieldDescriptions));

            // run each graph while there's either:
            // - a graph reference whose output trigger has been activated
            // - a graph input trigger has been activated by another graph referencing it
            // - events dispatched
            var ocount = m_OutputTriggersPerEntityGraphActivated.Count();
            var icount = m_InputTriggersPerEntityGraphActivated.Count();
            int iteration = 0;
            bool secondOutputTriggersMap = true;
            var dispatchedEvents = new List<EventNodeData>();
            while (iteration == 0 || iteration < 100 && (ocount > 0 || icount > 0 || m_DispatchedEvents.Count > 0))
            {
                // LogIterationReason();

                dispatchedEvents.Clear();
                ocount = 0;

                Entities.With(m_Query).ForEach(e =>
                {
                    if (!m_Contexts.TryGetValue(e, out GraphInstance ctx))
                        return;

                    var beingDestroyed = m_BeingDestroyedQueryMask.Matches(e);

                    if (beingDestroyed)
                    {
                        destroyed.Add(e);
                        ctx.TriggerEntryPoints<OnDestroy>();
                    }
                    // swap two hash maps - one for the current iteration and one for the next one, clear and swap after each iteration.
                    // for some reason var tmp = h1; h1 = h2; h2 = tmp; didn't work

                    var events = ctx.GlobalToLocalEventData(m_DispatchedEvents).ToList();

                    for (var i = 0; i < events.Count; ++i)
                    {
                        bool eventsTriggered = false;
                        var evt = events[i];
#if VS_DOTS_PHYSICS_EXISTS
                        if (evt.Id == VisualScriptingPhysics.TriggerEventId)
                            eventsTriggered |= ctx.TriggerEvents<OnTrigger>();
                        else if (evt.Id == VisualScriptingPhysics.CollisionEventId)
                            eventsTriggered |= ctx.TriggerEvents<OnCollision>();
                        else
#endif
                        eventsTriggered |= ctx.TriggerEvents<OnEvent>();

                        // TODO: move that out of the loop once we remove the evt parameter of ResumeFrame and process events/inputs/graphrefs right at once
                        if (eventsTriggered)
                        {
                            // keep filling the same output map. events might trigger graph outputs
                            ctx.ResumeFrame(e, Time, evt, secondOutputTriggersMap
                                ? m_OutputTriggersPerEntityGraphActivated
                                : m_OutputTriggersPerEntityGraphActivated2);
                        }
                    }

                    // this call will remove used entries from the input map
                    var resumeInputs = ctx.TriggerGraphInputs(e, m_InputTriggersPerEntityGraphActivated);
                    // this one won't as we need to process them for each graph. use the output map previously filled by the first ResumeFrame call and maybe the subsequent ones in the event loop
                    var triggerGraphReferences = ctx.TriggerGraphReferences(e, secondOutputTriggersMap
                        ? m_OutputTriggersPerEntityGraphActivated
                        : m_OutputTriggersPerEntityGraphActivated2);

                    if (iteration == 0 || resumeInputs || triggerGraphReferences)
                    {
                        // fill the other output map.
                        ctx.ResumeFrame(e, Time, default, secondOutputTriggersMap
                            ? m_OutputTriggersPerEntityGraphActivated2
                            : m_OutputTriggersPerEntityGraphActivated);
                        // might dispatch events, keep them for the next iteration
                        dispatchedEvents.AddRange(ctx.DispatchedEvents);
                    }

                    if (beingDestroyed)
                    {
                        EntityManager.RemoveComponent<ScriptingGraphInstance>(e);
                        EntityManager.RemoveComponent<ScriptingGraph>(e);
                    }

                    events.Clear();
                    ctx.ClearDispatchedEvents();
                });


                // count the other too
                ocount = (secondOutputTriggersMap
                    ? m_OutputTriggersPerEntityGraphActivated2
                    : m_OutputTriggersPerEntityGraphActivated).Count();
                (secondOutputTriggersMap
                    ? m_OutputTriggersPerEntityGraphActivated
                    : m_OutputTriggersPerEntityGraphActivated2).Clear();
                secondOutputTriggersMap = !secondOutputTriggersMap;
                m_DispatchedEvents.Clear();
                m_DispatchedEvents.AddRange(dispatchedEvents);

                // should always be 0 ?
                icount = m_InputTriggersPerEntityGraphActivated.Count();

                iteration++;
            }

            if (iteration >= 100) // TODO user option
                Debug.LogError("Iteration overflow");

#if VS_TRACING
            foreach (var graphInstancePair in m_Contexts)
            {
                var graphInstance = graphInstancePair.Value;
                if (graphInstance?.FrameTrace != null)
                {
                    DotsFrameTrace.FlushFrameTrace(graphInstance.ScriptingGraphAssetID, UnityEngine.Time.frameCount,
                        graphInstance.CurrentEntity,
#if UNITY_EDITOR
                        EntityManager.GetName(graphInstance.CurrentEntity),
#else
                        graphInstance.CurrentEntity.Index.ToString(),
#endif
                        graphInstance.FrameTrace);
                    graphInstance.FrameTrace = null;
                }
            }
#endif
            for (var index = 0; index < destroyed.Length; index++)
            {
                var entity = destroyed[index];
                m_Contexts[entity].Dispose();
                m_Contexts.Remove(entity);
            }

            destroyed.Dispose();
            m_OutputTriggersPerEntityGraphActivated.Clear();
            m_OutputTriggersPerEntityGraphActivated2.Clear();

            // Keeping for debugging reasons
            // void LogIterationReason()
            // {
            //     string reason = "";
            //     if (ocount > 0)
            //         reason += " O" + ocount;
            //     if (icount > 0)
            //         reason += " I" + icount;
            //     if (m_DispatchedEvents.Count > 0)
            //         reason += " E" + m_DispatchedEvents.Count;
            //
            //     Debug.Log($"Iteration {iteration} reason: {reason}");
            // }

            EventDataBridge.NativeStrings128.Clear();
        }

        // look for callers referencing the key entity and activate the matching graph reference output trigger
        private NativeMultiHashMap<Entity, uint> m_OutputTriggersPerEntityGraphActivated;
        private NativeMultiHashMap<Entity, uint> m_OutputTriggersPerEntityGraphActivated2;
        // when processing the key entity, activate the matching graph input trigger
        private NativeHashMap<Entity, uint> m_InputTriggersPerEntityGraphActivated;

        public Execution RunNestedGraph(GraphInstance graphInstance, GraphReference graphReference, Entity target, int triggerIndex)
        {
            // we're past matching the beingDestroyedQuery as ScriptingGraphInstance and all have already been removed
            if (!EntityManager.Exists(target) || !m_Contexts.TryGetValue(target, out GraphInstance otherContext))
            {
                Debug.LogError($"Running nested graph on destroyed entity: {target}, aborting");
                return Execution.Done;
            }

            Assert.AreNotEqual(otherContext, graphInstance, "RunNested graph has been called with identical parent and child context");

            // Copy GraphReference Data Inputs to Graph Data Inputs
            for (uint i = 0; i < graphReference.DataInputs.DataCount; i++)
            {
                var readValue = graphInstance.ReadValue(graphReference.DataInputs.SelectPort(i));
                // only copy actual value, don't override the initialization value if this port is not connected
                if (readValue.Type != ValueType.Unknown)
                {
                    otherContext.WriteToInputData(i, otherContext.CopyValueFromGraphInstance(readValue, graphInstance));
                }
            }

            if (triggerIndex != -1) // -1 means we're updating the already running graph reference
            {
                otherContext.TriggerGraphInput(triggerIndex);
                m_InputTriggersPerEntityGraphActivated.TryAdd(target, 0);
            }
            return Execution.Done;
        }

        public void CopyGraphDataOutputsToGraphReferenceOutputs(GraphInstance referenceContext, GraphReference referenceNode, Entity targetEntity)
        {
            var targetContext = m_Contexts[targetEntity];

            for (uint i = 0; i < referenceNode.DataOutputs.DataCount; i++)
                referenceContext.Write(referenceNode.DataOutputs.SelectPort(i), referenceContext.CopyValueFromGraphInstance(targetContext.ReadGraphOutputValue((int)i), targetContext));
        }

        internal GraphInstance CreateEntityContext(DynamicBuffer<ValueInput> inputs,
            Entity e,
            GraphDefinition graphDefinition)
        {
            Unity.Assertions.Assert.AreNotEqual(Entity.Null, e);
            GraphInstance ctx;
            var graphContext = ctx = GraphInstance.Create(graphDefinition, EntityManager, inputs);
            ctx.ScriptingGraphRuntime = this;
            m_Contexts.Add(e, graphContext);
            foreach (var subgraphReference in graphDefinition.SubgraphReferences)
            {
                Value subgraphEntity = graphContext.ReadDataSlot(subgraphReference.SubgraphEntityDataIndex);
                CreateEntityContext(default,
                    subgraphEntity.Entity,
                    subgraphReference.Subgraph.Definition);
            }
            return ctx;
        }

#if VS_DOTS_PHYSICS_EXISTS
        void TriggerPhysicsEvents(Entity e,
            ref NativeMultiHashMap<Entity, VisualScriptingPhysics.CollisionTriggerData> data,
            ulong eventId)
        {
            if (data.ContainsKey(e))
            {
                var collectedData = VisualScriptingPhysics.CollectCollisionTriggerData(FrameCount, ref data, e);
                if (collectedData != null)
                {
                    foreach (var values in collectedData.Value.Select(t => new List<Value> { t.Other, (int)t.State }))
                    {
                        m_DispatchedEvents.Add(new EventNodeData(eventId, values, e));
                    }
                }
                collectedData?.Dispose();
            }
        }

#endif
    }
}