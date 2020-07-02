using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class StickyNote : Unity.Modifier.GraphElements.StickyNote, IHasGraphElementModel
    {
        public IGraphElementModel GraphElementModel => Model as IGraphElementModel;
    }
}