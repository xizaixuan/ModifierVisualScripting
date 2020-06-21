using System.Collections.Generic;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class PanToNodeAction : IAction
    {
        public readonly GUID nodeGuid;

        public PanToNodeAction(GUID nodeGuid)
        {
            this.nodeGuid = nodeGuid;
        }
    }

    public class ResetElementColorAction : IAction
    {
        public readonly IReadOnlyCollection<NodeModel> NodeModels;
        public readonly IReadOnlyCollection<PlacematModel> PlacematModels;

        public ResetElementColorAction(
            IReadOnlyCollection<NodeModel> nodeModels,
            IReadOnlyCollection<PlacematModel> placematModels)
        {
            NodeModels = nodeModels;
            PlacematModels = placematModels;
        }
    }

    public class ChangeElementColorAction : IAction
    {
        public readonly IReadOnlyCollection<NodeModel> NodeModels;
        public readonly IReadOnlyCollection<PlacematModel> PlacematModels;
        public readonly Color Color;

        public ChangeElementColorAction(Color color,
                                        IReadOnlyCollection<NodeModel> nodeModels,
                                        IReadOnlyCollection<PlacematModel> placematModels)
        {
            NodeModels = nodeModels;
            PlacematModels = placematModels;
            Color = color;
        }
    }
}