using System.Collections.Generic;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreatePortalsOppositeAction : IAction
    {
        public readonly IEnumerable<IEdgePortalModel> PortalsToOpen;

        public CreatePortalsOppositeAction(IEnumerable<IEdgePortalModel> portalModels)
        {
            PortalsToOpen = portalModels;
        }
    }
}
