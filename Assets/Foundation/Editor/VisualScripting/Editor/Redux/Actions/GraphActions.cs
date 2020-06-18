using System.Collections.Generic;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreateFunctionAction : IAction
    {
        public readonly string Name;
        public readonly Vector2 Position;

        public CreateFunctionAction(string name, Vector2 position)
        {
            Name = name;
            Position = position;
        }
    }

    public class DeleteElementsAction : IAction
    {
        public readonly IReadOnlyCollection<IGraphElementModel> ElementsToRemove;

        public DeleteElementsAction(params IGraphElementModel[] elementsToRemove)
        {
            ElementsToRemove = elementsToRemove;
        }
    }

    public class RenameElementAction : IAction
    {
        public readonly IRenamableModel RenamableModel;
        public readonly string Name;

        public RenameElementAction(IRenamableModel renamableModel, string name)
        {
            RenamableModel = renamableModel;
            Name = name;
        }
    }

    public class MoveElementsAction : IAction
    {
        public readonly IReadOnlyCollection<NodeModel> NodeModels;
        public readonly IReadOnlyCollection<StickyNoteModel> StickyModels;
        public readonly Vector2 Delta;
        public readonly IReadOnlyCollection<IEdgeModel> EdgeModels;

        public readonly IReadOnlyCollection<PlacematModel> PlacematModels;

        public MoveElementsAction(Vector2 delta,
                                  IReadOnlyCollection<NodeModel> nodeModels,
                                  IReadOnlyCollection<PlacematModel> placematModels,
                                  IReadOnlyCollection<StickyNoteModel> stickyModels,
                                  IReadOnlyCollection<IEdgeModel> edgeModels)
        {
            NodeModels = nodeModels;
            PlacematModels = placematModels;
            StickyModels = stickyModels;
            EdgeModels = edgeModels;
            Delta = delta;
        }
    }

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