namespace UnityEditor.Modifier.VisualScripting.Editor.Plugins
{
    public enum TracingStepType : byte
    {
        None,
        ExecutedNode,
        TriggeredPort,
        WrittenValue,
        ReadValue,
    }
}