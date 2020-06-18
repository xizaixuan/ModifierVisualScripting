namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IHasMainOutputPort : INodeModel
    {
        IPortModel OutputPort { get; }
    }
}