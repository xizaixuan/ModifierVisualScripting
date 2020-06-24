using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **On Update** event node starts to trigger right after the **On Start** event node. From there, it will execute anything chained to it, during one frame and for any consecutive frames.",
        Example = @"![](OnUpdate/OnUpdateExample_01.gif)

In this example, we have a Cube containing a visual script that will:

1. Trigger the On Start event when entering play mode

   1. Wait 2 seconds.
   2. Set the variable **CanUpdate** to **True**.

2. The On Update will execute When the variable **CanUpdate** is **True**.

   1. The Elapsed time will be logged until:

      - The object is destroyed.
      - The object is disabled.
      - The component with the script is destroyed.
      - The component with the script is disabled.
      - The variable CanUpdate is false.")]
    public struct OnUpdate : IEntryPointNode
    {
        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "If **Enabled** is set to true (Checked), the **On Update** Event will be triggered when the GameObject with the visual script get enabled.")]
        public InputDataPort Enabled;
        [PortDescription("", Description = "When the GameObject is enabled, this port will trigger the next action you want to execute by connecting it to any Input Execution Flow port.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            if (ctx.ReadBool(Enabled))
                ctx.Trigger(Output);
        }
    }
}
