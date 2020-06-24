#if VS_DOTS_PHYSICS_EXISTS
using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;
using Modifier.VisualScripting.Physics;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **On Collision** event node start to trigger when a GameObject enter in collision with the GameObject containing the visual script with the **On Collision** event. From there, it will execute anything chained to its control flow ports depending of the execution context you want to use.\n" +
        "\n" +
        "**Note:** To get the **On Collision** event work when working in a scene level, you absolutely need to setup your GameObjects like this:\n" +
        " 1- The object with the visual script.\n" +
        "  - Component: **Convert To Entity**.\n" +
        "  - Component: **Physics Shape**  or Any **collider**.\n" +
        "  - Component: **Scripting Graph Authoring (Script)** with our visual Script containing the On Collision Event.\n" +
        " 2- The GameObject that will Enter in collision with our object having the On Collision Event.:\n" +
        "  - Component: **Convert To Entity**.\n" +
        "  - Component: **Physics Shape** with the **Raises Collision Events** checked in the Advanced Section.\n" +
        "  - Component: **Physics Body** or **Rigidbody**")]
    public struct OnCollision : IEventReceiverNode
    {
        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "If **Enabled** is set to true (Checked), the **On Collision** Event will be triggered every time a game object collide with it or stop to collide.")]
        public InputDataPort Enabled;

        [PortDescription(ValueType.Entity, Description = "The GameObject you want to validate that is colliding with the GameObject containing this visual script.")]
        public InputDataPort Instance;

        [PortDescription(Description = "Execute next action when a GameObject enter in collision with the GameObject containing the visual script.")]
        public OutputTriggerPort Entered;
        [PortDescription(Description = "Execute next action when a GameObject stop to collide with the GameObject containing the visual script.")]
        public OutputTriggerPort Exited;
        [PortDescription(Description = "Execute every frame when a GameObject as entered in collision with the GameObject containing the visual script and is still colliding with it.")]
        public OutputTriggerPort Inside;

        [PortDescription(ValueType.Entity, Description = "Return the GameObject Interacting with the collision.")]
        public OutputDataPort Detected;

        [SerializeField]
        ulong m_EventId;

        public ulong EventId
        {
            get => m_EventId;
            set => m_EventId = value;
        }

        public Execution Execute<TCtx>(TCtx ctx, EventNodeData data) where TCtx : IGraphInstance
        {
            if (!ctx.ReadBool(Enabled))
                return Execution.Done;

            var detected = ctx.ReadEntity(Instance);
            var other = data.Values.ElementAt(0).Entity;
            if (detected != Entity.Null && detected != other)
                return Execution.Done;

            ctx.Write(Detected, other);

            var state = (VisualScriptingPhysics.CollisionState)data.Values.ElementAt(1).Int;
            switch (state)
            {
                case VisualScriptingPhysics.CollisionState.None:
                    break;
                case VisualScriptingPhysics.CollisionState.Enter:
                    // Triggers are handled as a stack so we put Inside first
                    ctx.Trigger(Inside);
                    ctx.Trigger(Entered);
                    break;
                case VisualScriptingPhysics.CollisionState.Stay:
                    ctx.Trigger(Inside);
                    break;
                case VisualScriptingPhysics.CollisionState.Exit:
                    ctx.Trigger(Exited);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Execution.Done;
        }
    }
}
#endif