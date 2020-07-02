using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphToolsFoundation.Model;
using UnityEditor.Modifier.EditorCommon.Redux;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEngine;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    public class CreateNodeFromExecutionPortAction : IAction
    {
        public readonly IPortModel PortModel;
        public readonly Vector2 Position;
        public readonly IEnumerable<IGTFEdgeModel> EdgesToDelete;

        public CreateNodeFromExecutionPortAction(IPortModel portModel, Vector2 position, IEnumerable<IGTFEdgeModel> edgesToDelete = null)
        {
            PortModel = portModel;
            Position = position;
            EdgesToDelete = edgesToDelete ?? Enumerable.Empty<IGTFEdgeModel>();
        }
    }

    public class CreateNodeFromInputPortAction : IAction
    {
        public readonly IPortModel PortModel;
        public readonly IEnumerable<IGTFEdgeModel> EdgesToDelete;
        public readonly Vector2 Position;
        public readonly GraphNodeModelSearcherItem SelectedItem;

        public CreateNodeFromInputPortAction(IPortModel portModel, Vector2 position,
                                             GraphNodeModelSearcherItem selectedItem, IEnumerable<IGTFEdgeModel> edgesToDelete = null)
        {
            PortModel = portModel;
            Position = position;
            SelectedItem = selectedItem;
            EdgesToDelete = edgesToDelete ?? Enumerable.Empty<IGTFEdgeModel>();
        }
    }

    public class CreateStackedNodeFromOutputPortAction : IAction
    {
        public readonly IPortModel PortModel;
        public readonly IStackModel StackModel;
        public readonly int Index;
        public readonly StackNodeModelSearcherItem SelectedItem;
        public readonly IEnumerable<IGTFEdgeModel> EdgesToDelete;

        public CreateStackedNodeFromOutputPortAction(IPortModel portModel, IStackModel stackModel, int index,
                                                     StackNodeModelSearcherItem selectedItem, IEnumerable<IGTFEdgeModel> edgesToDelete = null)
        {
            PortModel = portModel;
            StackModel = stackModel;
            Index = index;
            SelectedItem = selectedItem;
            EdgesToDelete = edgesToDelete ?? Enumerable.Empty<IGTFEdgeModel>();
        }
    }

    public class CreateNodeFromOutputPortAction : IAction
    {
        public readonly IPortModel PortModel;
        public readonly Vector2 Position;
        public readonly GraphNodeModelSearcherItem SelectedItem;
        public readonly IEnumerable<IGTFEdgeModel> EdgesToDelete;

        public CreateNodeFromOutputPortAction(IPortModel portModel, Vector2 position,
                                              GraphNodeModelSearcherItem selectedItem, IEnumerable<IGTFEdgeModel> edgesToDelete = null)
        {
            PortModel = portModel;
            Position = position;
            SelectedItem = selectedItem;
            EdgesToDelete = edgesToDelete ?? Enumerable.Empty<IGTFEdgeModel>();
        }
    }

    public class SplitEdgeAndInsertNodeAction : IAction
    {
        public readonly IEdgeModel EdgeModel;
        public readonly INodeModel NodeModel;

        public SplitEdgeAndInsertNodeAction(IEdgeModel edgeModel, INodeModel nodeModel)
        {
            EdgeModel = edgeModel;
            NodeModel = nodeModel;
        }
    }

    public class CreateNodeOnEdgeAction : IAction
    {
        public readonly IEdgeModel EdgeModel;
        public readonly Vector2 Position;
        public readonly GraphNodeModelSearcherItem SelectedItem;
        public readonly GUID? Guid;

        public CreateNodeOnEdgeAction(IEdgeModel edgeModel, Vector2 position,
                                      GraphNodeModelSearcherItem selectedItem, GUID? guid = null)
        {
            EdgeModel = edgeModel;
            Position = position;
            SelectedItem = selectedItem;
            Guid = guid;
        }
    }

    public class ConvertEdgesToPortalsAction : IAction
    {
        public readonly IReadOnlyCollection<(IEdgeModel edge, Vector2 startPortPos, Vector2 endPortPos)> EdgeData;

        public ConvertEdgesToPortalsAction(IReadOnlyCollection<(IEdgeModel, Vector2, Vector2)> edgeData)
        {
            EdgeData = edgeData;
        }
    }
}