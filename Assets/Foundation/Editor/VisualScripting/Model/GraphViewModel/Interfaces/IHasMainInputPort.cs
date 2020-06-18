namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IHasMainInputPort : INodeModel
    {
        IPortModel InputPort { get; }
    }
}