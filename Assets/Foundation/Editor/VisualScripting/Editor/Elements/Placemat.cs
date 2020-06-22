using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    class Placemat : Unity.Modifier.GraphElements.Placemat, IHasGraphElementModel, IContextualMenuBuilder, ICustomColor
    {
        public IGraphElementModel GraphElementModel => Model as IGraphElementModel;

        void IContextualMenuBuilder.BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            BuildContextualMenu(evt);
            var index = evt.menu.MenuItems().FindIndex(item => (item as DropdownMenuAction)?.name == "Change Color...");
            evt.menu.RemoveItemAt(index);
            // Also remove separator.
            evt.menu.RemoveItemAt(index);
        }
    }
}