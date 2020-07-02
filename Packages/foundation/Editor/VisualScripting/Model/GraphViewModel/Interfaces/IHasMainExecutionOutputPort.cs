namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IHasMainExecutionOutputPort : INodeModel
    {
        IPortModel ExecutionOutputPort { get; }
    }
}
