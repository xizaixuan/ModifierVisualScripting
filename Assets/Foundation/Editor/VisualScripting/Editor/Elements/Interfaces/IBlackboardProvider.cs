using System.Collections.Generic;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public interface IBlackboardProvider
    {
        IEnumerable<BlackboardSection> CreateSections();
        string GetSubTitle();
        void AddItemRequested<TAction>(Store store, TAction action) where TAction : IAction;
        void MoveItemRequested(Store store, int index, VisualElement field);
        void RebuildSections(Blackboard blackboard);
        void DisplayAppropriateSearcher(Vector2 mousePosition, Blackboard blackboard);
        bool CanAddItems { get; }
        void BuildContextualMenu(DropdownMenu evtMenu, VisualElement visualElement, Store store, Vector2 mousePosition);
    }
}