using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IVisualScriptingField
    {
        IGraphElementModel GraphElementModel { get; }
        IGraphElementModel ExpandableGraphElementModel { get; }
        void Expand();
        bool CanInstantiateInGraph();
    }
}