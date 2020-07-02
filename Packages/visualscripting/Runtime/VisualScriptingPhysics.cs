#if VS_DOTS_PHYSICS_EXISTS
using Modifier.Runtime;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine.Modifier.VisualScripting;

namespace Modifier.VisualScripting.Physics
{
    public static class VisualScriptingPhysics
    {
        [Hidden]
        struct VisualScriptingTriggerEvent : IVisualScriptingEvent { }

        [Hidden]
        struct VisualScriptingCollisionEvent : IVisualScriptingEvent { }

        public static ulong TriggerEventId => TypeHash.CalculateStableTypeHash(typeof(VisualScriptingTriggerEvent));
        public static ulong CollisionEventId => TypeHash.CalculateStableTypeHash(typeof(VisualScriptingCollisionEvent));

        public static NativeArray<CollisionTriggerData>? CollectCollisionTriggerData(int frame, ref NativeMultiHashMap<Entity, CollisionTriggerData> nativeMultiHashMap, Entity e)
        {
            var count = nativeMultiHashMap.CountValuesForKey(e);
            NativeArray<CollisionTriggerData> collisions = new NativeArray<CollisionTriggerData>(count, Allocator.Temp);
            int idx = 0;
            if (nativeMultiHashMap.TryGetFirstValue(e, out var collInfo1, out var it))
            {
                do
                {
                    if (collInfo1.Frame < frame)
                    {
                        collInfo1.State = CollisionState.Exit;
                        nativeMultiHashMap.Remove(it);
                    }
                    else if (collInfo1.Frame == frame)
                    {
                        // MBRIAU: What are we trying to clean up here?
                        collInfo1.State = collInfo1.State == CollisionState.Stay ? CollisionState.Stay : CollisionState.Enter;
                    }
                    collisions[idx++] = collInfo1;
                }
                while (nativeMultiHashMap.TryGetNextValue(out collInfo1, ref it));
            }

            return collisions;
        }

        [Flags]
        public enum EventType
        {
            Collision = 1,
            Trigger = 2
        }

        public static void SetupCollisionTriggerData(EntityManager entityManager, int frame, ref NativeMultiHashMap<Entity, CollisionTriggerData> nativeMultiHashMap, EntityQuery q, EventType collisionMode)
        {
            var calculateEntityCount = q.CalculateEntityCount();
            if (nativeMultiHashMap.Capacity < calculateEntityCount)
                nativeMultiHashMap.Capacity = calculateEntityCount;

            var job = new CollectCollisionsJob
            {
                Frame = frame,
                CollInfos = nativeMultiHashMap,
                ThisMask = entityManager.GetEntityQueryMask(q),
                OtherMask = entityManager.GetEntityQueryMask(entityManager.UniversalQuery),
            };

            var buildPhysicsWorldSystem = entityManager.World.GetOrCreateSystem<BuildPhysicsWorld>();
            var stepPhysicsWorldSystem = entityManager.World.GetOrCreateSystem<StepPhysicsWorld>();
            if (collisionMode == EventType.Collision)
            {
                job.EventType = EventType.Collision;
                ICollisionEventJobExtensions
                    .Schedule(job, stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, new JobHandle())
                    .Complete();
            }
            else if (collisionMode == EventType.Trigger)
            {
                job.EventType = EventType.Trigger;
                ITriggerEventJobExtensions
                    .Schedule(job, stepPhysicsWorldSystem.Simulation, ref buildPhysicsWorldSystem.PhysicsWorld, new JobHandle())
                    .Complete();
            }
        }

        public struct CollisionTriggerData : IEquatable<CollisionTriggerData>
        {
            public Entity Other;
            public int Frame;
            public CollisionState State;
            public EventType EventType;

            public bool Equals(CollisionTriggerData other)
            {
                return Other.Equals(other.Other);
            }

            public override bool Equals(object obj)
            {
                return obj is CollisionTriggerData other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Other.GetHashCode();
            }
        }

        public enum CollisionState
        {
            None,
            Enter,
            Stay,
            Exit,
        }

        struct CollectCollisionsJob : ITriggerEventsJob, ICollisionEventsJob
        {
            public EntityQueryMask ThisMask;
            public EntityQueryMask OtherMask;
            public int Frame;

            public NativeMultiHashMap<Entity, CollisionTriggerData> CollInfos;
            public EventType EventType;

            public void Execute(TriggerEvent triggerEvent)
            {
                var ea = triggerEvent.Entities.EntityA;
                var eb = triggerEvent.Entities.EntityB;
                Process(ea, eb);
            }

            public void Execute(CollisionEvent triggerEvent)
            {
                var ea = triggerEvent.Entities.EntityA;
                var eb = triggerEvent.Entities.EntityB;
                Process(ea, eb);
            }

            void Process(Entity ea, Entity eb)
            {
                Entity self, other;

                if (ea.Index > eb.Index)
                {
                    var tmp = ea;
                    ea = eb;
                    eb = tmp;
                }

                if (ThisMask.Matches(ea) && OtherMask.Matches(eb))
                {
                    self = ea;
                    other = eb;
                }
                else if (ThisMask.Matches(eb) && OtherMask.Matches(ea))
                {
                    self = eb;
                    other = ea;
                }
                else
                {
                    return;
                }

                bool found = false;
                if (CollInfos.TryGetFirstValue(self, out var collInfo, out var it))
                {
                    do
                    {
                        if (collInfo.Other != other)// || (collInfo.EventType & EventType) == 0)
                            continue;

                        found = true;
                        if (collInfo.Frame == Frame - 1) // had a collision during the prev frame
                        {
                            collInfo.State = CollisionState.Stay;
                            collInfo.Frame = Frame;
                            CollInfos.SetValue(collInfo, it);
                        }

                        break;
                    }
                    while (CollInfos.TryGetNextValue(out collInfo, ref it));
                }

                if (!found) // new collision
                {
                    CollInfos.Add(self, new CollisionTriggerData { Other = other, Frame = Frame, State = CollisionState.Enter, EventType = EventType });
                    CollInfos.Add(other, new CollisionTriggerData { Other = self, Frame = Frame, State = CollisionState.Enter, EventType = EventType });
                }
            }
        }
    }
}
#endif