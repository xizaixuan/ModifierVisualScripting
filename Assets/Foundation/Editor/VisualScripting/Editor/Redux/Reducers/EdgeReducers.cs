using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Modifier.GraphElements;
using UnityEditor.Modifier.VisualScripting.Editor.SmartSearch;
using UnityEditor.Modifier.VisualScripting.GraphViewModel;
using UnityEditor.Modifier.VisualScripting.Model;
using UnityEditor.Modifier.VisualScripting.Model.Stencils;
using UnityEngine;
using UnityEngine.Assertions;
using static UnityEditor.Modifier.VisualScripting.Model.VSPreferences;

namespace UnityEditor.Modifier.VisualScripting.Editor
{
    static class EdgeReducers
    {
        const int k_NodeOffset = 60;
        const int k_StackOffset = 120;

        public static void Register(Store store)
        {
            store.Register<CreateNodeFromLoopPortAction>(CreateNodeFromLoopPort);
            store.Register<CreateInsertLoopNodeAction>(CreateInsertLoopNode);
            store.Register<CreateNodeFromExecutionPortAction>(CreateNodeFromExecutionPort);
            store.Register<CreateNodeFromInputPortAction>(CreateGraphNodeFromInputPort);
            store.Register<CreateStackedNodeFromOutputPortAction>(CreateStackedNodeFromOutputPort);
            store.Register<CreateNodeFromOutputPortAction>(CreateNodeFromOutputPort);
            store.Register<CreateEdgeAction>(CreateEdge);
            store.Register<SplitEdgeAndInsertNodeAction>(SplitEdgeAndInsertNode);
            store.Register<CreateNodeOnEdgeAction>(CreateNodeOnEdge);
            store.Register<AddControlPointOnEdgeAction>(AddControlPointOnEdge);
            store.Register<MoveEdgeControlPointAction>(MoveEdgeControlPoint);
            store.Register<RemoveEdgeControlPointAction>(RemoveEdgeControlPoint);
            store.Register<SetEdgeEditModeAction>(SetEdgeEditMode);
        }

        static State CreateNodeFromLoopPort(State previousState, CreateNodeFromLoopPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete);

            var stackPosition = action.Position - Vector2.right * k_StackOffset;

            if (action.PortModel.NodeModel is LoopNodeModel loopNodeModel)
            {
                var loopStackType = loopNodeModel.MatchingStackType;
                var loopStack = graphModel.CreateLoopStack(loopStackType, stackPosition);

                graphModel.CreateEdge(loopStack.InputPort, action.PortModel);
            }
            else
            {
                var stack = graphModel.CreateStack(null, stackPosition);
                graphModel.CreateEdge(stack.InputPorts[0], action.PortModel);
            }

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateInsertLoopNode(State previousState, CreateInsertLoopNodeAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            Undo.RegisterCompleteObjectUndo((Object)graphModel.AssetModel, "Create InsertLoop Node");
            Assert.IsTrue(graphModel.AssetModel as Object);
            graphModel.DeleteEdges(action.EdgesToDelete);

            var loopNode = ((StackBaseModel)action.StackModel).CreateStackedNode(
                action.LoopStackModel.MatchingStackedNodeType, "", action.Index);

            graphModel.CreateEdge(action.PortModel, loopNode.OutputsByDisplayOrder.First());
            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateNodeFromExecutionPort(State previousState, CreateNodeFromExecutionPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete);

            var stackPosition = action.Position - Vector2.right * k_StackOffset;
            var stack = graphModel.CreateStack(string.Empty, stackPosition);

            if (action.PortModel.Direction == Direction.Output)
                graphModel.CreateEdge(stack.InputPorts[0], action.PortModel);
            else
                graphModel.CreateEdge(action.PortModel, stack.OutputPorts[0]);

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateGraphNodeFromInputPort(State previousState, CreateNodeFromInputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete);

            var position = action.Position - Vector2.up * k_NodeOffset;
            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var outputPortModel = action.PortModel.DataType == TypeHandle.Unknown
                ? selectedNodeModel.OutputsByDisplayOrder.FirstOrDefault()
                : GetFirstPortModelOfType(action.PortModel.DataType, selectedNodeModel.OutputsByDisplayOrder);

            if (outputPortModel != null)
            {
                var newEdge = graphModel.CreateEdge(action.PortModel, outputPortModel);
                if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                    graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);
            }

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateStackedNodeFromOutputPort(State previousState, CreateStackedNodeFromOutputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            Undo.RegisterCompleteObjectUndo((Object)graphModel.AssetModel, "Create Node From Output Port");
            graphModel.DeleteEdges(action.EdgesToDelete);

            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new StackNodeCreationData(action.StackModel, action.Index));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var outputPortModel = action.PortModel;
            var newInput = selectedNodeModel.InputsByDisplayOrder.FirstOrDefault();
            if (newInput != null)
            {
                CreateItemizedNode(previousState, graphModel, ref outputPortModel);
                var newEdge = graphModel.CreateEdge(newInput, outputPortModel);
                if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                    graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);
            }

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateNodeFromOutputPort(State previousState, CreateNodeFromOutputPortAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;
            graphModel.DeleteEdges(action.EdgesToDelete);

            var position = action.Position - Vector2.up * k_NodeOffset;
            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position));

            if (!elementModels.Any() || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            var inputPortModel = action.PortModel.DataType == TypeHandle.Unknown
                ? selectedNodeModel.InputsByDisplayOrder.FirstOrDefault()
                : GetFirstPortModelOfType(action.PortModel.DataType, selectedNodeModel.InputsByDisplayOrder);

            if (inputPortModel == null)
                return previousState;

            var outputPortModel = action.PortModel;

            CreateItemizedNode(previousState, graphModel, ref outputPortModel);
            var newEdge = graphModel.CreateEdge(inputPortModel, outputPortModel);

            if (newEdge != null && previousState.Preferences.GetBool(BoolPref.AutoAlignDraggedEdges))
                graphModel.LastChanges?.ModelsToAutoAlign.Add(newEdge);

            graphModel.LastChanges?.ChangedElements.Add(action.PortModel.NodeModel);

            return previousState;
        }

        static State CreateNodeOnEdge(State previousState, CreateNodeOnEdgeAction action)
        {
            var edgeInput = action.EdgeModel.InputPortModel;
            var edgeOutput = action.EdgeModel.OutputPortModel;

            // Instantiate node
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;

            var position = action.Position - Vector2.up * k_NodeOffset;

            List<GUID> guids = action.Guid.HasValue ? new List<GUID> { action.Guid.Value } : null;

            var elementModels = action.SelectedItem.CreateElements.Invoke(
                new GraphNodeCreationData(graphModel, position, guids: guids));

            if (elementModels.Length == 0 || !(elementModels[0] is INodeModel selectedNodeModel))
                return previousState;

            // Delete old edge
            graphModel.DeleteEdge(action.EdgeModel);

            // Connect input port
            var inputPortModel = selectedNodeModel is FunctionCallNodeModel
                ? selectedNodeModel.InputsByDisplayOrder.FirstOrDefault(p =>
                p.DataType.Equals(edgeOutput.DataType))
                : selectedNodeModel.InputsByDisplayOrder.FirstOrDefault();

            if (inputPortModel != null)
                graphModel.CreateEdge(inputPortModel, edgeOutput);

            // Find first matching output type and connect it
            var outputPortModel = GetFirstPortModelOfType(edgeInput.DataType,
                selectedNodeModel.OutputsByDisplayOrder);

            if (outputPortModel != null)
                graphModel.CreateEdge(edgeInput, outputPortModel);

            return previousState;
        }

        static State CreateEdge(State previousState, CreateEdgeAction action)
        {
            var graphModel = (VSGraphModel)previousState.CurrentGraphModel;

            if (action.EdgeModelsToDelete != null)
                graphModel.DeleteEdges(action.EdgeModelsToDelete);

            IPortModel outputPortModel = action.OutputPortModel;
            IPortModel inputPortModel = action.InputPortModel;

            if (inputPortModel.NodeModel is LoopStackModel loopStackModel)
            {
                if (!loopStackModel.MatchingStackedNodeType.IsInstanceOfType(outputPortModel.NodeModel))
                    return previousState;
            }

            CreateItemizedNode(previousState, graphModel, ref outputPortModel);
            graphModel.CreateEdge(inputPortModel, outputPortModel);

            if (action.PortAlignment.HasFlag(CreateEdgeAction.PortAlignmentType.Input))
                graphModel.LastChanges.ModelsToAutoAlign.Add(inputPortModel.NodeModel);
            if (action.PortAlignment.HasFlag(CreateEdgeAction.PortAlignmentType.Output))
                graphModel.LastChanges.ModelsToAutoAlign.Add(outputPortModel.NodeModel);

            return previousState;
        }

        static State SplitEdgeAndInsertNode(State previousState, SplitEdgeAndInsertNodeAction action)
        {
            Assert.IsTrue(action.NodeModel.InputsById.Count > 0);
            Assert.IsTrue(action.NodeModel.OutputsById.Count > 0);

            var graphModel = ((VSGraphModel)previousState.CurrentGraphModel);
            var edgeInput = action.EdgeModel.InputPortModel;
            var edgeOutput = action.EdgeModel.OutputPortModel;
            graphModel.DeleteEdge(action.EdgeModel);
            graphModel.CreateEdge(edgeInput, action.NodeModel.OutputsByDisplayOrder.First());
            graphModel.CreateEdge(action.NodeModel.InputsByDisplayOrder.First(), edgeOutput);

            return previousState;
        }

        [CanBeNull]
        static IPortModel GetFirstPortModelOfType(TypeHandle typeHandle, IEnumerable<IPortModel> portModels)
        {
            Stencil stencil = portModels.First().GraphModel.Stencil;
            IPortModel unknownPortModel = null;

            // Return the first matching Input portModel
            // If no match was found, return the first Unknown typed portModel
            // Else return null.
            foreach (IPortModel portModel in portModels)
            {
                if (portModel.DataType == TypeHandle.Unknown && unknownPortModel == null)
                {
                    unknownPortModel = portModel;
                }

                if (typeHandle.IsAssignableFrom(portModel.DataType, stencil))
                {
                    return portModel;
                }
            }
            return unknownPortModel;
        }

        static void CreateItemizedNode(State state, VSGraphModel graphModel, ref IPortModel outputPortModel)
        {
            ItemizeOptions currentItemizeOptions = state.Preferences.CurrentItemizeOptions;

            // automatically itemize, i.e. duplicate variables as they get connected
            if (!outputPortModel.Connected || currentItemizeOptions == ItemizeOptions.Nothing)
                return;

            INodeModel nodeToConnect = outputPortModel.NodeModel;

            bool itemizeContant = currentItemizeOptions.HasFlag(ItemizeOptions.Constants)
                && nodeToConnect is ConstantNodeModel;
            bool itemizeVariable = currentItemizeOptions.HasFlag(ItemizeOptions.Variables)
                && (nodeToConnect is VariableNodeModel || nodeToConnect is ThisNodeModel);
            bool itemizeSystemConstant = currentItemizeOptions.HasFlag(ItemizeOptions.SystemConstants) &&
                nodeToConnect is SystemConstantNodeModel;
            if (itemizeContant || itemizeVariable || itemizeSystemConstant)
            {
                Vector2 offset = Vector2.up * k_NodeOffset;
                nodeToConnect = graphModel.DuplicateUnstackedNode(outputPortModel.NodeModel, new Dictionary<INodeModel, NodeModel>(), offset);
                outputPortModel = nodeToConnect.OutputsById[outputPortModel.UniqueId];
            }
        }

        static State AddControlPointOnEdge(State previousState, AddControlPointOnEdgeAction action)
        {
            action.EdgeModel.InsertEdgeControlPoint(action.AtIndex, action.Position, 100);
            return previousState;
        }

        static State MoveEdgeControlPoint(State previousState, MoveEdgeControlPointAction action)
        {
            action.EdgeModel.ModifyEdgeControlPoint(action.EdgeIndex, action.NewPosition, action.NewTightness);
            return previousState;
        }

        static State RemoveEdgeControlPoint(State previousState, RemoveEdgeControlPointAction action)
        {
            action.EdgeModel.RemoveEdgeControlPoint(action.EdgeIndex);
            return previousState;
        }

        static State SetEdgeEditMode(State previousState, SetEdgeEditModeAction action)
        {
            var graphModel = previousState.CurrentGraphModel;
            if (action.Value)
            {
                foreach (var edge in graphModel.EdgeModels)
                {
                    if (edge.EditMode)
                    {
                        edge.EditMode = false;
                        graphModel.LastChanges?.ChangedElements.Add(edge);
                    }
                }
            }

            action.EdgeModel.EditMode = action.Value;
            graphModel.LastChanges?.ChangedElements.Add(action.EdgeModel);
            return previousState;
        }
    }
}