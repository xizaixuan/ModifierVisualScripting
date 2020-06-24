using Modifier.Runtime.Nodes;
using System;
using UnityEngine;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription(KeyEventType.Down, "The **On Key Down** event node is triggered when the user press the chosen key from the visual scripting inspector. This event will process only one time.")]
    [NodeDescription(KeyEventType.Up, "The **On Key Up** event node is triggered when the user release the chosen key from the visual scripting inspector. This event will process only one time.")]
    [NodeDescription(KeyEventType.Hold, "The **On Key Hold** event node is triggered at every frame when the user Hold the chosen key from the visual scripting inspector.")]
    public struct OnKey : IEntryPointNode, IHasExecutionType<OnKey.KeyEventType>
    {
        public enum KeyEventType
        {
            Down,
            Up,
            Hold
        }

        public KeyCode KeyCode;
        public KeyEventType EventType;

        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "If **Enabled** is set to true (Checked), the **On Key** event will be triggered when the user press/hold or release a key.")]
        public InputDataPort Enabled;
        [PortDescription("", Description = "When the user key is in the good state, this port will trigger the next action you want to execute by connecting it to any Input Execution Flow port.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            if (!ctx.ReadBool(Enabled))
                return;

            if (EventType == KeyEventType.Down && Input.GetKeyDown(KeyCode) ||
                EventType == KeyEventType.Up && Input.GetKeyUp(KeyCode) ||
                EventType == KeyEventType.Hold && Input.GetKey(KeyCode))
                ctx.Trigger(Output);
        }

        public KeyEventType Type
        {
            get { return EventType; }
            set { EventType = value; }
        }
    }
}