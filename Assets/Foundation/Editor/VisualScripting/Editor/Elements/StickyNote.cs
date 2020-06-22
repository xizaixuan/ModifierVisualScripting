using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class StickyNote : Unity.Modifier.GraphElements.StickyNote, IHasGraphElementModel
    {
        public IGraphElementModel GraphElementModel => Model as IGraphElementModel;
    }
}