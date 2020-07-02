namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IHasInstancePort : INodeModel
    {
        IPortModel InstancePort { get; }
    }
}