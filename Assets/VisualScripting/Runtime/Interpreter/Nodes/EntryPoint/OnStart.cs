using System;

namespace Modifier.Runtime
{
    [Serializable]
    [NodeDescription("The **On Start** event node is triggered when the object containing the script is enabled and runs only one time. Use the **On Update** event if your action needs to run at every frame.",
        Example = @"![](OnStart/OnStartExample_01.gif)

In this example, we have a game object (CubeAlive) containing a visual script. When we go in play mode, the script will log ""**I'm Alive!**"" in the console 2 seconds after starting.",
        DataSetup = @"Game object needs the following components

- Convert to Entity. Set to **Convert And Destroy**
- Scripting Graph Authoring pointing on our visual script.

![](OnStart/SimpleExample_01.png)")]
    public struct OnStart : IEntryPointNode
    {
        [PortDescription(ValueType.Bool, DefaultValue = true, Description = "If **Enabled** is set to true (Checked), the **On Start** event will be triggered when the object with the visual script get enabled.")]
        public InputDataPort Enabled;
        [PortDescription("", Description = "When the object is enabled, this port will trigger the next action you want to execute by connecting it to any Input Execution Flow port.")]
        public OutputTriggerPort Output;

        public void Execute<TCtx>(TCtx ctx) where TCtx : IGraphInstance
        {
            if (ctx.ReadBool(Enabled))
                ctx.Trigger(Output);
        }
    }
}