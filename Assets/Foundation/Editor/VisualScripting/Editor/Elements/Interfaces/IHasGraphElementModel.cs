using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IHasGraphElementModel
    {
        IGraphElementModel GraphElementModel { get; }
    }
}