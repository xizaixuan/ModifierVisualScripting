using Object = UnityEngine.Object;
namespace UnityEditor.Modifier.VisualScripting.GraphViewModel
{
    public interface IObjectReference
    {
        Object ReferencedObject { get; }
    }
}