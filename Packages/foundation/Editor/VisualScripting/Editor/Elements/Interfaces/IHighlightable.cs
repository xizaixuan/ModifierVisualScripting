using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IHighlightable
    {
        bool Highlighted { get; set; }
        bool ShouldHighlightItemUsage(IGraphElementModel graphElementModel);
    }
}