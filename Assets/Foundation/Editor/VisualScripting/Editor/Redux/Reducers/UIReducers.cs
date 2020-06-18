using UnityEditor.Modifier.VisualScripting.Model;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class UIReducers
    {
        public static void Register(Store store)
        {
            store.Register<RefreshUIAction>(RefreshUI);
        }

        static State RefreshUI(State previousState, RefreshUIAction action)
        {
            previousState.MarkForUpdate(action.UpdateFlags);
            if (action.ChangedModels != null)
                ((VSGraphModel)previousState.CurrentGraphModel).LastChanges.ChangedElements.AddRange(action.ChangedModels);
            return previousState;
        }
    }
}