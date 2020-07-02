using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Model;
using Object = UnityEngine.Object;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class PortalReducers
    {
        public static void Register(Store store)
        {
            store.Register<CreatePortalsOppositeAction>(CreatePortalOppositeAction);
        }

        static State CreatePortalOppositeAction(State previousState, CreatePortalsOppositeAction action)
        {
            if (action.PortalsToOpen == null)
                return previousState;
            var portalsToOpen = action.PortalsToOpen.Where(p => p.CanCreateOppositePortal()).ToList();
            if (!portalsToOpen.Any())
                return previousState;

            Undo.RegisterCompleteObjectUndo((Object)previousState.AssetModel, "Create Opposite Portals");
            EditorUtility.SetDirty((Object)previousState.AssetModel);

            foreach (var portalModel in portalsToOpen)
                ((VSGraphModel)previousState.CurrentGraphModel).CreateOppositePortal(portalModel);

            previousState.MarkForUpdate(UpdateFlags.GraphTopology);
            return previousState;
        }
    }
}