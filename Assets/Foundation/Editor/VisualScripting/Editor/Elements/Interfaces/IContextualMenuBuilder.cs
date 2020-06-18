using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IContextualMenuBuilder
    {
        void BuildContextualMenu(ContextualMenuPopulateEvent evt);
    }
}