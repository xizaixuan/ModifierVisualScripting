using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Unity.Modifier.GraphElements
{
    public interface IDroppable
    {
        bool IsDroppable();
    }

    public interface IDropTarget
    {
        bool CanAcceptDrop(List<ISelectable> selection);

        // evt.mousePosition will be in global coordinates.
        bool DragUpdated(DragUpdatedEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource);
        bool DragPerform(DragPerformEvent evt, IEnumerable<ISelectable> selection, IDropTarget dropTarget, ISelection dragSource);
        bool DragEnter(DragEnterEvent evt, IEnumerable<ISelectable> selection, IDropTarget enteredTarget, ISelection dragSource);
        bool DragLeave(DragLeaveEvent evt, IEnumerable<ISelectable> selection, IDropTarget leftTarget, ISelection dragSource);
        bool DragExited();
    }
}