namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IHasMainExecutionInputPort : INodeModel
    {
        IPortModel ExecutionInputPort { get; }
    }
}
