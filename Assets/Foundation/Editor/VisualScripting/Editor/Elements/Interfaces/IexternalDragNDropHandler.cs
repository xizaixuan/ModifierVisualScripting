using JetBrains.Annotations;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    [PublicAPI]
    public enum DragNDropContext
    {
        Blackboard, Graph
    }

    [PublicAPI]
    public interface IExternalDragNDropHandler
    {
        void HandleDragUpdated(DragUpdatedEvent e, DragNDropContext ctx);
        void HandleDragPerform(DragPerformEvent e, Store store, DragNDropContext ctx, VisualElement element);
    }
}