using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription(@"The **On Destroy** event node is triggered when the object containing the script is destroyed.

Things you won't be able to do after an On Destroy:
- Get data from components within the destroyed object.
- Add Timed nodes such as **Wait**, **Stop Watch** as the script component will be destroyed when they process.
- On Update event won't process after.",
        Example = @"![](OnDestroy/OnDestroyExample_01.gif)

In this example, we have a Cube containing a visual script that will:

1. Trigger the **On Start** event when entering play mode
   1. Wait 2 seconds.
   2. Destroy the game object Cube.

2. When the Cube get destroyed, the On Destroy event will be triggered.
   1. The message Destroyed will now be written in the console.")]
    public struct OnDestroy : IEntryPointNode
    {
        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "If **Enabled** is set to true (Checked), the **On Destroy** event will be triggered when the object containing the script is destroyed.")]
        public InputDataPort Enabled;
        [PortDescription("", Description = "Execute next action after the GameObject is destroyed.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            if (ctx.ReadBool(Enabled))
                ctx.Trigger(Output);
        }
    }
}