using System.Collections.Generic;

namespace Unity.Modifier.GraphElements
{
    public interface ISelection
    {
        List<ISelectable> selection { get; }

        void AddToSelectioin(ISelectable selectable);
        void RemoveFromSelection(ISelectable selectable);
        void ClearSelection();
    }
}